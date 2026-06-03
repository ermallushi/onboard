using Microsoft.AspNetCore.Mvc;
using Onboard.Web.Application;
using Onboard.Web.Domain;

namespace Onboard.Web.Controllers.Api;

[ApiController]
[Route("api/biometric")]
public sealed class BiometricController(IOnboardingOrchestrator orchestrator) : ControllerBase
{
    [HttpPost("selfie")]
    public IActionResult UploadSelfie([FromBody] BiometricSession session) => Ok(orchestrator.UploadSelfie(session));

    [HttpPost("{caseId}/verify")]
    public IActionResult TriggerVerification(string caseId) => Ok(orchestrator.TriggerBiometricVerification(caseId));

    [HttpGet("{caseId}/result")]
    public IActionResult FetchResult(string caseId)
    {
        var result = orchestrator.GetBiometricResult(caseId);
        return result is null ? NotFound() : Ok(result);
    }
}
