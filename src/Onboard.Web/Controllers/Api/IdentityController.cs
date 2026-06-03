using Microsoft.AspNetCore.Mvc;
using Onboard.Web.Application;
using Onboard.Web.Domain;

namespace Onboard.Web.Controllers.Api;

[ApiController]
[Route("api/identity")]
public sealed class IdentityController(IOnboardingOrchestrator orchestrator) : ControllerBase
{
    [HttpPost("personal-data")]
    public IActionResult SubmitPersonalData([FromBody] PersonProfile profile) => Ok(orchestrator.SubmitPersonProfile(profile));

    [HttpPost("documents")]
    public IActionResult UploadDocuments([FromBody] IdentityDocument document) => Ok(orchestrator.UploadIdentityDocument(document));

    [HttpPost("{caseId}/verify")]
    public IActionResult TriggerVerification(string caseId) => Ok(orchestrator.TriggerDocumentVerification(caseId));

    [HttpGet("{caseId}/result")]
    public IActionResult FetchResult(string caseId)
    {
        var result = orchestrator.GetDocumentResult(caseId);
        return result is null ? NotFound() : Ok(result);
    }
}
