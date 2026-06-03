using Microsoft.AspNetCore.Mvc;
using Onboard.Web.Application;

namespace Onboard.Web.Controllers.Api;

[ApiController]
[Route("api/risk")]
public sealed class RiskController(IOnboardingOrchestrator orchestrator) : ControllerBase
{
    [HttpPost("{caseId}/run")]
    public IActionResult RunScreening(string caseId) => Ok(orchestrator.RunRiskScreening(caseId));

    [HttpGet("{caseId}/result")]
    public IActionResult FetchResult(string caseId)
    {
        var result = orchestrator.GetRiskResult(caseId);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("{caseId}/manual-decision")]
    public IActionResult SubmitManualDecision(string caseId, [FromBody] ManualRiskDecisionRequest request)
    {
        orchestrator.SubmitManualRiskDecision(caseId, request);
        return NoContent();
    }
}
