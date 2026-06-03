namespace Onboard.Web.Domain;

public enum CaseStatus
{
    Started,
    IdentityCaptured,
    DocumentVerificationPending,
    DocumentVerified,
    DocumentRejected,
    BiometricPending,
    BiometricVerified,
    BiometricFailed,
    RiskPending,
    RiskReview,
    RiskRejected,
    ApprovedForSigning,
    SignaturePending,
    Signed,
    Completed,
    Rejected,
    Expired,
    Cancelled
}
