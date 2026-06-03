using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Onboard.Web.Application;
using Onboard.Web.Infrastructure;

namespace Onboard.Web.Controllers.Api;

/// <summary>Processes async provider callbacks for KYC and signature events.</summary>
[ApiController]
[Route("api/webhooks")]
public sealed class WebhooksController(IOnboardingOrchestrator orchestrator, ICallbackSecurityService callbackSecurityService) : ControllerBase
{
    /// <summary>
    /// Receives a KYC provider callback. The request must include an X-Callback-Signature header
    /// containing the HMAC-SHA256 hex signature of the JSON body using the shared secret.
    /// Idempotent: duplicate eventId values are silently ignored.
    /// </summary>
    [HttpPost("kyc")]
    public IActionResult ReceiveKycCallback([FromBody] CallbackEvent callbackEvent, [FromHeader(Name = "X-Callback-Signature")] string signature)
    {
        var payload = JsonSerializer.Serialize(callbackEvent);
        if (!callbackSecurityService.IsValidSignature(payload, signature))
            return Unauthorized();

        var processed = orchestrator.ProcessKycCallback(callbackEvent);
        return Ok(new { processed });
    }

    /// <summary>
    /// Receives a digital signature provider callback. Same signature validation and idempotency rules apply.
    /// </summary>
    [HttpPost("signature")]
    public IActionResult ReceiveSignatureCallback([FromBody] CallbackEvent callbackEvent, [FromHeader(Name = "X-Callback-Signature")] string signature)
    {
        var payload = JsonSerializer.Serialize(callbackEvent);
        if (!callbackSecurityService.IsValidSignature(payload, signature))
            return Unauthorized();

        var processed = orchestrator.ProcessSignatureCallback(callbackEvent);
        return Ok(new { processed });
    }
}

