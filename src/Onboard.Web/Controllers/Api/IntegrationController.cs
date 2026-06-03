using Microsoft.AspNetCore.Mvc;

namespace Onboard.Web.Controllers.Api;

[ApiController]
[Route("api/integrations")]
public sealed class IntegrationController : ControllerBase
{
    [HttpGet("health")]
    public IActionResult Health() => Ok(new { status = "ok" });

    [HttpPost("webhooks/customer-system")]
    public IActionResult ReceiveEnterpriseWebhook([FromBody] object payload) => Accepted(new { received = true, payload });
}
