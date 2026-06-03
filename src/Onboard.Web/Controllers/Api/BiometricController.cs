using Microsoft.AspNetCore.Mvc;
using Onboard.Web.Application;
using Onboard.Web.Domain;

namespace Onboard.Web.Controllers.Api;

/// <summary>Handles liveness detection and face-match biometric checks.</summary>
[ApiController]
[Route("api/biometric")]
public sealed class BiometricController(IOnboardingOrchestrator orchestrator) : ControllerBase
{
    /// <summary>Uploads the customer's selfie for the case.</summary>
    [HttpPost("selfie")]
    public IActionResult UploadSelfie([FromBody] BiometricSession session) => Ok(orchestrator.UploadSelfie(session));

    /// <summary>Triggers async biometric verification via the configured provider.</summary>
    [HttpPost("{caseId}/verify")]
    public IActionResult TriggerVerification(string caseId) => Ok(orchestrator.TriggerBiometricVerification(caseId));

    /// <summary>Returns the current biometric verification result for the case.</summary>
    [HttpGet("{caseId}/result")]
    public IActionResult FetchResult(string caseId)
    {
        var result = orchestrator.GetBiometricResult(caseId);
        return result is null ? NotFound() : Ok(result);
    }
}

