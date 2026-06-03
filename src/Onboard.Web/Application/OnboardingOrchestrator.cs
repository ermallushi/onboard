using Microsoft.Extensions.Options;
using Onboard.Web.Domain;
using Onboard.Web.Infrastructure;

namespace Onboard.Web.Application;

public sealed class OnboardingOrchestrator(
    InMemoryOnboardingStore store,
    IIdentityProviderAdapter identityProviderAdapter,
    IBiometricProviderAdapter biometricProviderAdapter,
    IRiskProviderAdapter riskProviderAdapter,
    ISignatureProviderAdapter signatureProviderAdapter,
    IOptions<DecisionRulesOptions> options) : IOnboardingOrchestrator
{
    private readonly InMemoryOnboardingStore _store = store;
    private readonly IIdentityProviderAdapter _identityProviderAdapter = identityProviderAdapter;
    private readonly IBiometricProviderAdapter _biometricProviderAdapter = biometricProviderAdapter;
    private readonly IRiskProviderAdapter _riskProviderAdapter = riskProviderAdapter;
    private readonly ISignatureProviderAdapter _signatureProviderAdapter = signatureProviderAdapter;
    private readonly DecisionRulesOptions _rules = options.Value;

    private static readonly Dictionary<CaseStatus, HashSet<CaseStatus>> AllowedTransitions = new()
    {
        [CaseStatus.Started] = [CaseStatus.IdentityCaptured, CaseStatus.Cancelled, CaseStatus.Expired],
        [CaseStatus.IdentityCaptured] = [CaseStatus.DocumentVerificationPending, CaseStatus.Cancelled],
        [CaseStatus.DocumentVerificationPending] = [CaseStatus.DocumentVerified, CaseStatus.DocumentRejected],
        [CaseStatus.DocumentVerified] = [CaseStatus.BiometricPending, CaseStatus.RiskPending],
        [CaseStatus.DocumentRejected] = [CaseStatus.Rejected],
        [CaseStatus.BiometricPending] = [CaseStatus.BiometricVerified, CaseStatus.BiometricFailed],
        [CaseStatus.BiometricVerified] = [CaseStatus.RiskPending],
        [CaseStatus.BiometricFailed] = [CaseStatus.Rejected],
        [CaseStatus.RiskPending] = [CaseStatus.RiskReview, CaseStatus.RiskRejected, CaseStatus.ApprovedForSigning],
        [CaseStatus.RiskReview] = [CaseStatus.ApprovedForSigning, CaseStatus.RiskRejected],
        [CaseStatus.RiskRejected] = [CaseStatus.Rejected],
        [CaseStatus.ApprovedForSigning] = [CaseStatus.SignaturePending],
        [CaseStatus.SignaturePending] = [CaseStatus.Signed, CaseStatus.Rejected],
        [CaseStatus.Signed] = [CaseStatus.Completed],
        [CaseStatus.Completed] = [],
        [CaseStatus.Rejected] = [],
        [CaseStatus.Expired] = [],
        [CaseStatus.Cancelled] = []
    };

    public OnboardingCase CreateCase(CreateCaseRequest request)
    {
        var onboardingCase = new OnboardingCase
        {
            ExternalReference = request.ExternalReference,
            JourneyType = request.JourneyType,
            CurrentChannel = request.CurrentChannel,
            CustomerReference = request.CustomerReference,
            Status = CaseStatus.Started
        };

        _store.Cases[onboardingCase.CaseId] = onboardingCase;
        AppendAudit(onboardingCase.CaseId, "system", "orchestrator", "case.created", onboardingCase.ExternalReference);
        return onboardingCase;
    }

    public OnboardingCase? GetCase(string caseId) => _store.Cases.TryGetValue(caseId, out var onboardingCase) ? onboardingCase : null;

    public IReadOnlyCollection<OnboardingCase> SearchCases(string? customerReference)
    {
        return _store.Cases.Values
            .Where(c => string.IsNullOrWhiteSpace(customerReference) || c.CustomerReference.Equals(customerReference, StringComparison.OrdinalIgnoreCase))
            .ToArray();
    }

    public void CancelCase(string caseId, string actorId)
    {
        var onboardingCase = GetRequiredCase(caseId);
        Transition(onboardingCase, CaseStatus.Cancelled, "staff", actorId, "case.cancelled");
    }

    public ChannelTransition TransitionChannel(string caseId, ChannelTransitionRequest request)
    {
        var onboardingCase = GetRequiredCase(caseId);
        var transition = new ChannelTransition
        {
            CaseId = caseId,
            FromChannel = onboardingCase.CurrentChannel,
            ToChannel = request.ToChannel,
            TransitionReason = request.Reason,
            PerformedBy = request.PerformedBy
        };

        onboardingCase.PreviousChannel = onboardingCase.CurrentChannel;
        onboardingCase.CurrentChannel = request.ToChannel;
        onboardingCase.UpdatedAt = DateTimeOffset.UtcNow;
        _store.AppendTransition(transition);
        AppendAudit(caseId, "staff", request.PerformedBy, "channel.transitioned", request.Reason);
        return transition;
    }

    public PersonProfile SubmitPersonProfile(PersonProfile profile)
    {
        _ = GetRequiredCase(profile.CaseId);
        _store.ProfilesByCase[profile.CaseId] = profile;
        Transition(GetRequiredCase(profile.CaseId), CaseStatus.IdentityCaptured, "customer", profile.PersonId, "person.submitted");
        return profile;
    }

    public IdentityDocument UploadIdentityDocument(IdentityDocument identityDocument)
    {
        _ = GetRequiredCase(identityDocument.CaseId);
        _store.DocumentsByCase[identityDocument.CaseId] = identityDocument;
        AppendAudit(identityDocument.CaseId, "customer", "document", "document.uploaded", identityDocument.DocumentType);
        return identityDocument;
    }

    public IdentityDocument TriggerDocumentVerification(string caseId)
    {
        var onboardingCase = GetRequiredCase(caseId);
        var document = _store.DocumentsByCase.GetValueOrDefault(caseId) ?? throw new InvalidOperationException("Document is required before verification.");
        document.ProviderReference = _identityProviderAdapter.StartVerification(document);
        Transition(onboardingCase, CaseStatus.DocumentVerificationPending, "system", "orchestrator", "document.verification.requested");
        return document;
    }

    public IdentityDocument? GetDocumentResult(string caseId) => _store.DocumentsByCase.GetValueOrDefault(caseId);

    public BiometricSession UploadSelfie(BiometricSession biometricSession)
    {
        _ = GetRequiredCase(biometricSession.CaseId);
        _store.BiometricsByCase[biometricSession.CaseId] = biometricSession;
        AppendAudit(biometricSession.CaseId, "customer", "selfie", "biometric.uploaded", biometricSession.SelfieFileReference);
        return biometricSession;
    }

    public BiometricSession TriggerBiometricVerification(string caseId)
    {
        var onboardingCase = GetRequiredCase(caseId);
        var biometric = _store.BiometricsByCase.GetValueOrDefault(caseId) ?? throw new InvalidOperationException("Selfie is required before biometric verification.");
        biometric.ProviderReference = _biometricProviderAdapter.StartVerification(biometric);
        Transition(onboardingCase, CaseStatus.BiometricPending, "system", "orchestrator", "biometric.verification.requested");
        return biometric;
    }

    public BiometricSession? GetBiometricResult(string caseId) => _store.BiometricsByCase.GetValueOrDefault(caseId);

    public RiskResult RunRiskScreening(string caseId)
    {
        var onboardingCase = GetRequiredCase(caseId);
        Transition(onboardingCase, CaseStatus.RiskPending, "system", "orchestrator", "risk.requested");
        var (fraudScore, sanctions, pep, adverseMedia, screeningReference) = _riskProviderAdapter.Evaluate(caseId);
        var result = new RiskResult
        {
            CaseId = caseId,
            FraudScore = fraudScore,
            FinalRiskScore = fraudScore,
            SanctionsResult = sanctions,
            PepResult = pep,
            AdverseMediaResult = adverseMedia,
            ScreeningReference = screeningReference
        };

        if (fraudScore >= _rules.RejectThreshold)
        {
            result.RecommendedAction = "reject";
            Transition(onboardingCase, CaseStatus.RiskRejected, "system", "rules", "risk.rejected");
            Transition(onboardingCase, CaseStatus.Rejected, "system", "rules", "case.rejected");
        }
        else if (fraudScore >= _rules.ReviewThreshold)
        {
            result.RecommendedAction = "review";
            Transition(onboardingCase, CaseStatus.RiskReview, "system", "rules", "risk.review");
        }
        else
        {
            result.RecommendedAction = "approve";
            Transition(onboardingCase, CaseStatus.ApprovedForSigning, "system", "rules", "risk.approved");
        }

        _store.RisksByCase[caseId] = result;
        return result;
    }

    public RiskResult? GetRiskResult(string caseId) => _store.RisksByCase.GetValueOrDefault(caseId);

    public void SubmitManualRiskDecision(string caseId, ManualRiskDecisionRequest request)
    {
        var onboardingCase = GetRequiredCase(caseId);
        var decision = request.Decision.ToLowerInvariant();
        if (decision == "approve")
        {
            Transition(onboardingCase, CaseStatus.ApprovedForSigning, "staff", request.PerformedBy, "risk.override.approve");
            return;
        }

        if (decision == "reject")
        {
            Transition(onboardingCase, CaseStatus.RiskRejected, "staff", request.PerformedBy, "risk.override.reject");
            Transition(onboardingCase, CaseStatus.Rejected, "staff", request.PerformedBy, "case.rejected");
            return;
        }

        throw new InvalidOperationException("Decision must be approve or reject.");
    }

    public ReviewCase AssignReviewQueue(string caseId, string queueName, string reason)
    {
        _ = GetRequiredCase(caseId);
        var reviewCase = new ReviewCase
        {
            CaseId = caseId,
            QueueName = queueName,
            Reason = reason
        };
        _store.ReviewsByCase[caseId] = reviewCase;
        AppendAudit(caseId, "system", "routing", "review.assigned", queueName);
        return reviewCase;
    }

    public ReviewCase AddReviewNote(string caseId, string note, string actor)
    {
        var review = _store.ReviewsByCase.GetValueOrDefault(caseId) ?? throw new InvalidOperationException("Review case not found.");
        review.Notes.Add($"{DateTimeOffset.UtcNow:O}|{actor}|{note}");
        AppendAudit(caseId, "staff", actor, "review.noted", note);
        return review;
    }

    public void OverrideReviewDecision(string caseId, string overrideDecision, string actor)
    {
        var review = _store.ReviewsByCase.GetValueOrDefault(caseId) ?? throw new InvalidOperationException("Review case not found.");
        review.OverrideDecision = overrideDecision;
        review.ResolvedAt = DateTimeOffset.UtcNow;
        AppendAudit(caseId, "staff", actor, "review.overridden", overrideDecision);
    }

    public SignaturePackage CreateSignaturePackage(string caseId, string documentReference, string level, string authMethod)
    {
        var onboardingCase = GetRequiredCase(caseId);
        if (onboardingCase.Status != CaseStatus.ApprovedForSigning)
        {
            throw new InvalidOperationException("Case must be approved for signing before package creation.");
        }

        var signaturePackage = new SignaturePackage
        {
            CaseId = caseId,
            DocumentReference = documentReference,
            SignatureLevel = level,
            SignerAuthMethod = authMethod,
            Status = "created"
        };
        _store.SignaturesByCase[caseId] = signaturePackage;
        AppendAudit(caseId, "system", "signature", "signature.package.created", documentReference);
        return signaturePackage;
    }

    public SignaturePackage SendForSignature(string caseId)
    {
        var onboardingCase = GetRequiredCase(caseId);
        var signaturePackage = _store.SignaturesByCase.GetValueOrDefault(caseId) ?? throw new InvalidOperationException("Signature package not found.");
        signaturePackage.ProviderReference = _signatureProviderAdapter.StartSigning(signaturePackage);
        signaturePackage.Status = "pending";
        Transition(onboardingCase, CaseStatus.SignaturePending, "system", "signature", "signature.requested");
        return signaturePackage;
    }

    public SignaturePackage? GetSignatureStatus(string caseId) => _store.SignaturesByCase.GetValueOrDefault(caseId);

    public string? GetSignatureEvidence(string caseId) => _store.SignaturesByCase.GetValueOrDefault(caseId)?.EvidenceReference;

    public bool ProcessKycCallback(CallbackEvent callbackEvent)
    {
        if (!_store.ProcessedCallbacks.TryAdd($"kyc:{callbackEvent.EventId}", true))
        {
            return false;
        }

        var onboardingCase = GetRequiredCase(callbackEvent.CaseId);
        if (callbackEvent.Success)
        {
            Transition(onboardingCase, CaseStatus.DocumentVerified, "provider", "kyc", "document.verified");
        }
        else
        {
            Transition(onboardingCase, CaseStatus.DocumentRejected, "provider", "kyc", "document.rejected");
            Transition(onboardingCase, CaseStatus.Rejected, "provider", "kyc", "case.rejected");
        }

        AppendAudit(callbackEvent.CaseId, "provider", "kyc", "callback.kyc", callbackEvent.CorrelationId);
        return true;
    }

    public bool ProcessSignatureCallback(CallbackEvent callbackEvent)
    {
        if (!_store.ProcessedCallbacks.TryAdd($"sig:{callbackEvent.EventId}", true))
        {
            return false;
        }

        var onboardingCase = GetRequiredCase(callbackEvent.CaseId);
        var signature = _store.SignaturesByCase.GetValueOrDefault(callbackEvent.CaseId) ?? throw new InvalidOperationException("Signature package not found.");

        if (callbackEvent.Success)
        {
            signature.Status = "signed";
            signature.SignedAt = DateTimeOffset.UtcNow;
            signature.EvidenceReference = callbackEvent.PayloadReference;
            Transition(onboardingCase, CaseStatus.Signed, "provider", "signature", "signature.completed");
            Transition(onboardingCase, CaseStatus.Completed, "provider", "signature", "case.completed");
        }
        else
        {
            signature.Status = "rejected";
            Transition(onboardingCase, CaseStatus.Rejected, "provider", "signature", "signature.failed");
        }

        AppendAudit(callbackEvent.CaseId, "provider", "signature", "callback.signature", callbackEvent.CorrelationId);
        return true;
    }

    public IReadOnlyCollection<AuditEvent> GetAuditEvents(string? caseId)
    {
        if (string.IsNullOrWhiteSpace(caseId))
        {
            return _store.AuditEventsByCase.Values.SelectMany(x => x).OrderBy(x => x.EventTime).ToArray();
        }

        return _store.AuditEventsByCase.GetValueOrDefault(caseId)?.OrderBy(x => x.EventTime).ToArray() ?? [];
    }

    private OnboardingCase GetRequiredCase(string caseId)
        => _store.Cases.GetValueOrDefault(caseId) ?? throw new KeyNotFoundException($"Case {caseId} was not found.");

    private void Transition(OnboardingCase onboardingCase, CaseStatus target, string actorType, string actorId, string eventType)
    {
        if (!AllowedTransitions.TryGetValue(onboardingCase.Status, out var allowed) || !allowed.Contains(target))
        {
            throw new InvalidOperationException($"Invalid transition from {onboardingCase.Status} to {target}.");
        }

        onboardingCase.Status = target;
        onboardingCase.UpdatedAt = DateTimeOffset.UtcNow;
        AppendAudit(onboardingCase.CaseId, actorType, actorId, eventType, target.ToString());
    }

    private void AppendAudit(string caseId, string actorType, string actorId, string eventType, string payload)
    {
        _store.AppendAuditEvent(new AuditEvent
        {
            CaseId = caseId,
            ActorType = actorType,
            ActorId = actorId,
            EventType = eventType,
            CorrelationId = Guid.NewGuid().ToString("N"),
            PayloadReference = payload
        });
    }
}
