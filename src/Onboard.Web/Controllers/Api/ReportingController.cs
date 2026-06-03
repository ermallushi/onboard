using Microsoft.AspNetCore.Mvc;
using Onboard.Web.Application;

namespace Onboard.Web.Controllers.Api;

[ApiController]
[Route("api/reporting")]
public sealed class ReportingController(IOnboardingOrchestrator orchestrator) : ControllerBase
{
    [HttpGet("operational-metrics")]
    public IActionResult OperationalMetrics()
    {
        var cases = orchestrator.SearchCases(null);
        return Ok(new
        {
            totalCases = cases.Count,
            byStatus = cases.GroupBy(c => c.Status.ToString()).ToDictionary(g => g.Key, g => g.Count())
        });
    }

    [HttpGet("audit-export")]
    public IActionResult AuditExport([FromQuery] string? caseId) => Ok(orchestrator.GetAuditEvents(caseId));

    [HttpGet("regulator-export")]
    public IActionResult RegulatorExport([FromQuery] string? caseId)
        => Ok(new { generatedAt = DateTimeOffset.UtcNow, events = orchestrator.GetAuditEvents(caseId) });
}
