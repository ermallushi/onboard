using FluentAssertions;
using Microsoft.Extensions.Options;
using Onboard.Web.Application;
using Onboard.Web.Domain;
using Onboard.Web.Infrastructure;

namespace Onboard.Web.Tests;

public sealed class OrchestratorTests
{
    private static IOnboardingOrchestrator CreateSut()
    {
        return new OnboardingOrchestrator(
            new InMemoryOnboardingStore(),
            new MockIdentityProviderAdapter(),
            new MockBiometricProviderAdapter(),
            new MockRiskProviderAdapter(),
            new MockSignatureProviderAdapter(),
            Options.Create(new DecisionRulesOptions { ReviewThreshold = 40, RejectThreshold = 75 }));
    }

    [Fact]
    public void Should_Reject_Invalid_Status_Transition()
    {
        var sut = CreateSut();
        var onboardingCase = sut.CreateCase(new CreateCaseRequest("ext-1", "digital", "digital", "cust-1"));

        var call = () => sut.RunRiskScreening(onboardingCase.CaseId);

        call.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Should_Process_Document_Callback_Idempotently()
    {
        var sut = CreateSut();
        var onboardingCase = sut.CreateCase(new CreateCaseRequest("ext-2", "digital", "digital", "cust-2"));
        sut.SubmitPersonProfile(new PersonProfile
        {
            CaseId = onboardingCase.CaseId,
            FirstName = "Ada",
            LastName = "Lovelace",
            DateOfBirth = new DateOnly(1990, 1, 1),
            Nationality = "UK",
            Email = "ada@example.com",
            Phone = "123",
            Address = "street"
        });
        sut.UploadIdentityDocument(new IdentityDocument { CaseId = onboardingCase.CaseId, DocumentType = "passport", DocumentNumber = "P123" });
        sut.TriggerDocumentVerification(onboardingCase.CaseId);

        var first = sut.ProcessKycCallback(new CallbackEvent("event-1", onboardingCase.CaseId, true, "corr-1", "payload-1"));
        var duplicate = sut.ProcessKycCallback(new CallbackEvent("event-1", onboardingCase.CaseId, true, "corr-1", "payload-1"));

        first.Should().BeTrue();
        duplicate.Should().BeFalse();
        sut.GetCase(onboardingCase.CaseId)!.Status.Should().Be(CaseStatus.DocumentVerified);
    }
}
