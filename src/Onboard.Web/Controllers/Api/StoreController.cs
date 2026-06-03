using Microsoft.AspNetCore.Mvc;
using Onboard.Web.Application;

namespace Onboard.Web.Controllers.Api;

public sealed record AssistedCaseRequest(string ExternalReference, string CustomerReference, string StaffId);

[ApiController]
[Route("api/store")]
public sealed class StoreController(IOnboardingOrchestrator orchestrator) : ControllerBase
{
    [HttpGet("cases")]
    public IActionResult SearchCase([FromQuery] string? customerReference) => Ok(orchestrator.SearchCases(customerReference));

    [HttpPost("cases/start")]
    public IActionResult StartAssistedCase([FromBody] AssistedCaseRequest request)
        => Ok(orchestrator.CreateCase(new CreateCaseRequest(request.ExternalReference, "assisted", "store", request.CustomerReference)));

    [HttpPost("cases/{caseId}/continue")]
    public IActionResult ContinueCase(string caseId, [FromBody] ChannelTransitionRequest request)
        => Ok(orchestrator.TransitionChannel(caseId, request));

    [HttpPost("cases/{caseId}/staff-decision")]
    public IActionResult SubmitStaffDecision(string caseId, [FromBody] ManualRiskDecisionRequest request)
    {
        orchestrator.SubmitManualRiskDecision(caseId, request);
        return NoContent();
    }
}
