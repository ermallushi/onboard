using Microsoft.AspNetCore.Mvc;
using Onboard.Web.Application;

namespace Onboard.Web.Controllers.Api;

public sealed record AssistedCaseRequest(string ExternalReference, string CustomerReference, string StaffId);

/// <summary>Supports in-store assisted onboarding, case search, and staff decision workflows.</summary>
[ApiController]
[Route("api/store")]
public sealed class StoreController(IOnboardingOrchestrator orchestrator) : ControllerBase
{
    /// <summary>Searches for cases by customer reference to allow in-store case resumption.</summary>
    [HttpGet("cases")]
    public IActionResult SearchCase([FromQuery] string? customerReference) => Ok(orchestrator.SearchCases(customerReference));

    /// <summary>Starts a new assisted onboarding case from a store branch.</summary>
    [HttpPost("cases/start")]
    public IActionResult StartAssistedCase([FromBody] AssistedCaseRequest request)
        => Ok(orchestrator.CreateCase(new CreateCaseRequest(request.ExternalReference, "assisted", "store", request.CustomerReference)));

    /// <summary>Continues a digital case in-store by recording a channel transition.</summary>
    [HttpPost("cases/{caseId}/continue")]
    public IActionResult ContinueCase(string caseId, [FromBody] ChannelTransitionRequest request)
        => Ok(orchestrator.TransitionChannel(caseId, request));

    /// <summary>Submits a staff override decision for the case (approve or reject).</summary>
    [HttpPost("cases/{caseId}/staff-decision")]
    public IActionResult SubmitStaffDecision(string caseId, [FromBody] ManualRiskDecisionRequest request)
    {
        orchestrator.SubmitManualRiskDecision(caseId, request);
        return NoContent();
    }
}

