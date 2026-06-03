using Microsoft.AspNetCore.Mvc;
using Onboard.Web.Application;
using Onboard.Web.Domain;

namespace Onboard.Web.Controllers.Api;

/// <summary>Handles document capture and identity verification.</summary>
[ApiController]
[Route("api/identity")]
public sealed class IdentityController(IOnboardingOrchestrator orchestrator) : ControllerBase
{
    /// <summary>Submits the customer's personal data and advances the case to IdentityCaptured.</summary>
    [HttpPost("personal-data")]
    public IActionResult SubmitPersonalData([FromBody] PersonProfile profile) => Ok(orchestrator.SubmitPersonProfile(profile));

    /// <summary>Uploads an identity document for the case.</summary>
    [HttpPost("documents")]
    public IActionResult UploadDocuments([FromBody] IdentityDocument document) => Ok(orchestrator.UploadIdentityDocument(document));

    /// <summary>Triggers async document verification via the configured KYC provider.</summary>
    [HttpPost("{caseId}/verify")]
    public IActionResult TriggerVerification(string caseId) => Ok(orchestrator.TriggerDocumentVerification(caseId));

    /// <summary>Returns the current document verification result for the case.</summary>
    [HttpGet("{caseId}/result")]
    public IActionResult FetchResult(string caseId)
    {
        var result = orchestrator.GetDocumentResult(caseId);
        return result is null ? NotFound() : Ok(result);
    }
}

