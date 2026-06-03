using Onboard.Web.Application;
using Onboard.Web.Domain;

namespace Onboard.Web.Infrastructure;

public sealed class MockIdentityProviderAdapter : IIdentityProviderAdapter
{
    public string StartVerification(IdentityDocument document) => $"idv-{document.DocumentId}";
}

public sealed class MockBiometricProviderAdapter : IBiometricProviderAdapter
{
    public string StartVerification(BiometricSession biometricSession) => $"bio-{biometricSession.BiometricId}";
}

public sealed class MockRiskProviderAdapter : IRiskProviderAdapter
{
    public (decimal FraudScore, string Sanctions, string Pep, string AdverseMedia, string ScreeningReference) Evaluate(string caseId)
    {
        var score = Math.Abs(caseId.GetHashCode()) % 100;
        return (score, "clear", "clear", "clear", $"risk-{caseId}");
    }
}

public sealed class MockSignatureProviderAdapter : ISignatureProviderAdapter
{
    public string StartSigning(SignaturePackage signaturePackage) => $"sig-{signaturePackage.SignatureId}";
}
