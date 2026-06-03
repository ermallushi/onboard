using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Onboard.Web.Application;
using Onboard.Web.Infrastructure;

namespace Onboard.Web.Controllers.Api;

[ApiController]
[Route("api/webhooks")]
public sealed class WebhooksController(IOnboardingOrchestrator orchestrator, ICallbackSecurityService callbackSecurityService) : ControllerBase
{
    [HttpPost("kyc")]
    public IActionResult ReceiveKycCallback([FromBody] CallbackEvent callbackEvent, [FromHeader(Name = "X-Callback-Signature")] string signature)
    {
        var payload = JsonSerializer.Serialize(callbackEvent);
        if (!callbackSecurityService.IsValidSignature(payload, signature))
        {
            return Unauthorized();
        }

        var processed = orchestrator.ProcessKycCallback(callbackEvent);
        return Ok(new { processed });
    }

    [HttpPost("signature")]
    public IActionResult ReceiveSignatureCallback([FromBody] CallbackEvent callbackEvent, [FromHeader(Name = "X-Callback-Signature")] string signature)
    {
        var payload = JsonSerializer.Serialize(callbackEvent);
        if (!callbackSecurityService.IsValidSignature(payload, signature))
        {
            return Unauthorized();
        }

        var processed = orchestrator.ProcessSignatureCallback(callbackEvent);
        return Ok(new { processed });
    }
}
