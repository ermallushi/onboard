using Onboard.Web.Domain;

namespace Onboard.Web.Application;

public sealed record CreateCaseRequest(string ExternalReference, string JourneyType, string CurrentChannel, string CustomerReference);
public sealed record ChannelTransitionRequest(string ToChannel, string Reason, string PerformedBy);
public sealed record ManualRiskDecisionRequest(string Decision, string PerformedBy);
public sealed record CallbackEvent(string EventId, string CaseId, bool Success, string CorrelationId, string PayloadReference);

public interface IOnboardingOrchestrator
{
    OnboardingCase CreateCase(CreateCaseRequest request);
    OnboardingCase? GetCase(string caseId);
    IReadOnlyCollection<OnboardingCase> SearchCases(string? customerReference);
    void CancelCase(string caseId, string actorId);
    ChannelTransition TransitionChannel(string caseId, ChannelTransitionRequest request);

    PersonProfile SubmitPersonProfile(PersonProfile profile);
    IdentityDocument UploadIdentityDocument(IdentityDocument identityDocument);
    IdentityDocument TriggerDocumentVerification(string caseId);
    IdentityDocument? GetDocumentResult(string caseId);

    BiometricSession UploadSelfie(BiometricSession biometricSession);
    BiometricSession TriggerBiometricVerification(string caseId);
    BiometricSession? GetBiometricResult(string caseId);

    RiskResult RunRiskScreening(string caseId);
    RiskResult? GetRiskResult(string caseId);
    void SubmitManualRiskDecision(string caseId, ManualRiskDecisionRequest request);

    ReviewCase AssignReviewQueue(string caseId, string queueName, string reason);
    ReviewCase AddReviewNote(string caseId, string note, string actor);
    void OverrideReviewDecision(string caseId, string overrideDecision, string actor);

    SignaturePackage CreateSignaturePackage(string caseId, string documentReference, string level, string authMethod);
    SignaturePackage SendForSignature(string caseId);
    SignaturePackage? GetSignatureStatus(string caseId);
    string? GetSignatureEvidence(string caseId);

    bool ProcessKycCallback(CallbackEvent callbackEvent);
    bool ProcessSignatureCallback(CallbackEvent callbackEvent);

    IReadOnlyCollection<AuditEvent> GetAuditEvents(string? caseId);
}

public interface IIdentityProviderAdapter
{
    string StartVerification(IdentityDocument document);
}

public interface IBiometricProviderAdapter
{
    string StartVerification(BiometricSession biometricSession);
}

public interface IRiskProviderAdapter
{
    (decimal FraudScore, string Sanctions, string Pep, string AdverseMedia, string ScreeningReference) Evaluate(string caseId);
}

public interface ISignatureProviderAdapter
{
    string StartSigning(SignaturePackage signaturePackage);
}

public sealed class DecisionRulesOptions
{
    public decimal ReviewThreshold { get; set; } = 40;
    public decimal RejectThreshold { get; set; } = 75;
}
