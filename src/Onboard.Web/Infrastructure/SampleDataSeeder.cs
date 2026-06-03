using Onboard.Web.Application;
using Onboard.Web.Domain;

namespace Onboard.Web.Infrastructure;

/// <summary>
/// Seeds three demo onboarding cases at different lifecycle stages when the application starts.
/// </summary>
public static class SampleDataSeeder
{
    public static void Seed(IOnboardingOrchestrator orchestrator)
    {
        SeedStartedCase(orchestrator);
        SeedDocumentPendingCase(orchestrator);
        SeedDocumentVerifiedCase(orchestrator);
    }

    // Case A: customer profile collected, waiting for document capture
    private static void SeedStartedCase(IOnboardingOrchestrator orchestrator)
    {
        var onboardingCase = orchestrator.CreateCase(new CreateCaseRequest(
            ExternalReference: "DEMO-DIGITAL-001",
            JourneyType: "digital",
            CurrentChannel: "digital",
            CustomerReference: "CUST-ALICE-001"));

        orchestrator.SubmitPersonProfile(new PersonProfile
        {
            CaseId = onboardingCase.CaseId,
            FirstName = "Alice",
            LastName = "Johnson",
            DateOfBirth = new DateOnly(1988, 4, 12),
            Nationality = "GBR",
            Email = "alice.johnson@example.com",
            Phone = "+44 7700 900111",
            Address = "12 Baker Street, London, W1U 6TN",
            ConsentFlags = new Dictionary<string, bool>
            {
                ["marketing"] = false,
                ["dataProcessing"] = true,
                ["thirdPartySharing"] = false
            }
        });
    }

    // Case B: document uploaded and verification triggered, awaiting async KYC callback
    private static void SeedDocumentPendingCase(IOnboardingOrchestrator orchestrator)
    {
        var onboardingCase = orchestrator.CreateCase(new CreateCaseRequest(
            ExternalReference: "DEMO-DIGITAL-002",
            JourneyType: "digital",
            CurrentChannel: "digital",
            CustomerReference: "CUST-BOB-002"));

        orchestrator.SubmitPersonProfile(new PersonProfile
        {
            CaseId = onboardingCase.CaseId,
            FirstName = "Bob",
            LastName = "Smith",
            DateOfBirth = new DateOnly(1975, 9, 30),
            Nationality = "IRL",
            Email = "bob.smith@example.com",
            Phone = "+353 87 111 2222",
            Address = "5 O'Connell Street, Dublin 1",
            ConsentFlags = new Dictionary<string, bool>
            {
                ["marketing"] = true,
                ["dataProcessing"] = true,
                ["thirdPartySharing"] = false
            }
        });

        orchestrator.UploadIdentityDocument(new IdentityDocument
        {
            CaseId = onboardingCase.CaseId,
            DocumentType = "passport",
            IssuingCountry = "IRL",
            DocumentNumber = "PA1234567",
            ExpiryDate = new DateOnly(2030, 6, 15),
            FrontFileReference = "file://uploads/bob-passport-front.jpg",
            BackFileReference = "file://uploads/bob-passport-back.jpg"
        });

        orchestrator.TriggerDocumentVerification(onboardingCase.CaseId);
    }

    // Case C: KYC callback received and processed, document verified — ready for biometrics
    private static void SeedDocumentVerifiedCase(IOnboardingOrchestrator orchestrator)
    {
        var onboardingCase = orchestrator.CreateCase(new CreateCaseRequest(
            ExternalReference: "DEMO-STORE-003",
            JourneyType: "assisted",
            CurrentChannel: "store",
            CustomerReference: "CUST-CAROL-003"));

        orchestrator.SubmitPersonProfile(new PersonProfile
        {
            CaseId = onboardingCase.CaseId,
            FirstName = "Carol",
            LastName = "Martinez",
            DateOfBirth = new DateOnly(1992, 1, 22),
            Nationality = "ESP",
            Email = "carol.martinez@example.com",
            Phone = "+34 612 345 678",
            Address = "Calle Gran Via 28, 28013 Madrid",
            ConsentFlags = new Dictionary<string, bool>
            {
                ["marketing"] = false,
                ["dataProcessing"] = true,
                ["thirdPartySharing"] = false
            }
        });

        orchestrator.UploadIdentityDocument(new IdentityDocument
        {
            CaseId = onboardingCase.CaseId,
            DocumentType = "national_id",
            IssuingCountry = "ESP",
            DocumentNumber = "12345678Z",
            ExpiryDate = new DateOnly(2028, 3, 10),
            FrontFileReference = "file://uploads/carol-id-front.jpg",
            BackFileReference = "file://uploads/carol-id-back.jpg"
        });

        orchestrator.TriggerDocumentVerification(onboardingCase.CaseId);

        orchestrator.ProcessKycCallback(new CallbackEvent(
            EventId: "callback-kyc-carol-001",
            CaseId: onboardingCase.CaseId,
            Success: true,
            CorrelationId: "corr-carol-001",
            PayloadReference: "kyc-evidence://provider/carol-001"));
    }
}
