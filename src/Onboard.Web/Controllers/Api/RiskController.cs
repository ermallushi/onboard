using Microsoft.AspNetCore.Mvc;
using Onboard.Web.Application;

namespace Onboard.Web.Controllers.Api;

/// <summary>Executes compliance screening and fraud scoring.</summary>
[ApiController]
[Route("api/risk")]
public sealed class RiskController(IOnboardingOrchestrator orchestrator) : ControllerBase
{
    /// <summary>Runs sanctions, PEP, adverse-media, and fraud screening and applies configured decision rules.</summary>
    [HttpPost("{caseId}/run")]
    public IActionResult RunScreening(string caseId) => Ok(orchestrator.RunRiskScreening(caseId));

    /// <summary>Returns the latest risk screening result for the case.</summary>
    [HttpGet("{caseId}/result")]
    public IActionResult FetchResult(string caseId)
    {
        var result = orchestrator.GetRiskResult(caseId);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Submits a manual risk override decision (approve or reject) for a case in RiskReview.</summary>
    [HttpPost("{caseId}/manual-decision")]
    public IActionResult SubmitManualDecision(string caseId, [FromBody] ManualRiskDecisionRequest request)
    {
        orchestrator.SubmitManualRiskDecision(caseId, request);
        return NoContent();
    }
}

