namespace Onboard.Web.Domain;

public sealed class OnboardingCase
{
    public string CaseId { get; init; } = Guid.NewGuid().ToString("N");
    public string ExternalReference { get; init; } = string.Empty;
    public string JourneyType { get; init; } = "digital";
    public string CurrentChannel { get; set; } = "digital";
    public string? PreviousChannel { get; set; }
    public CaseStatus Status { get; set; } = CaseStatus.Started;
    public string SubStatus { get; set; } = string.Empty;
    public string CustomerReference { get; set; } = string.Empty;
    public string AssignedQueue { get; set; } = "auto";
    public string RiskLevel { get; set; } = "unknown";
    public string? FinalDecision { get; set; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class PersonProfile
{
    public string PersonId { get; init; } = Guid.NewGuid().ToString("N");
    public string CaseId { get; init; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public string Nationality { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public Dictionary<string, bool> ConsentFlags { get; set; } = new();
}

public sealed class IdentityDocument
{
    public string DocumentId { get; init; } = Guid.NewGuid().ToString("N");
    public string CaseId { get; init; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string IssuingCountry { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public DateOnly ExpiryDate { get; set; }
    public string FrontFileReference { get; set; } = string.Empty;
    public string BackFileReference { get; set; } = string.Empty;
    public string? NfcReference { get; set; }
    public string ExtractionPayload { get; set; } = string.Empty;
    public string AuthenticityResult { get; set; } = "pending";
    public string TamperResult { get; set; } = "pending";
    public string ProviderReference { get; set; } = string.Empty;
}

public sealed class BiometricSession
{
    public string BiometricId { get; init; } = Guid.NewGuid().ToString("N");
    public string CaseId { get; init; } = string.Empty;
    public string SelfieFileReference { get; set; } = string.Empty;
    public string LivenessResult { get; set; } = "pending";
    public decimal FaceMatchScore { get; set; }
    public string ProviderReference { get; set; } = string.Empty;
    public string DecisionReason { get; set; } = string.Empty;
}

public sealed class RiskResult
{
    public string RiskId { get; init; } = Guid.NewGuid().ToString("N");
    public string CaseId { get; init; } = string.Empty;
    public string SanctionsResult { get; set; } = "pending";
    public string PepResult { get; set; } = "pending";
    public string AdverseMediaResult { get; set; } = "pending";
    public decimal FraudScore { get; set; }
    public decimal FinalRiskScore { get; set; }
    public string RecommendedAction { get; set; } = "review";
    public string ScreeningReference { get; set; } = string.Empty;
}

public sealed class ReviewCase
{
    public string ReviewId { get; init; } = Guid.NewGuid().ToString("N");
    public string CaseId { get; init; } = string.Empty;
    public string QueueName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string? AssignedUser { get; set; }
    public string? OverrideDecision { get; set; }
    public List<string> Notes { get; set; } = new();
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ResolvedAt { get; set; }
}

public sealed class SignaturePackage
{
    public string SignatureId { get; init; } = Guid.NewGuid().ToString("N");
    public string CaseId { get; init; } = string.Empty;
    public string DocumentReference { get; set; } = string.Empty;
    public string SignatureLevel { get; set; } = "simple";
    public string SignerAuthMethod { get; set; } = "otp";
    public string Status { get; set; } = "created";
    public string ProviderReference { get; set; } = string.Empty;
    public string EvidenceReference { get; set; } = string.Empty;
    public DateTimeOffset? SignedAt { get; set; }
}

public sealed class AuditEvent
{
    public string EventId { get; init; } = Guid.NewGuid().ToString("N");
    public string CaseId { get; init; } = string.Empty;
    public string ActorType { get; init; } = string.Empty;
    public string ActorId { get; init; } = string.Empty;
    public string EventType { get; init; } = string.Empty;
    public DateTimeOffset EventTime { get; init; } = DateTimeOffset.UtcNow;
    public string CorrelationId { get; init; } = string.Empty;
    public string PayloadReference { get; init; } = string.Empty;
}

public sealed class ChannelTransition
{
    public string TransitionId { get; init; } = Guid.NewGuid().ToString("N");
    public string CaseId { get; init; } = string.Empty;
    public string FromChannel { get; init; } = string.Empty;
    public string ToChannel { get; init; } = string.Empty;
    public string TransitionReason { get; init; } = string.Empty;
    public string PerformedBy { get; init; } = string.Empty;
    public DateTimeOffset TransitionTime { get; init; } = DateTimeOffset.UtcNow;
}
