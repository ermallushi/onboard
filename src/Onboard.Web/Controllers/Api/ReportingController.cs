using Microsoft.AspNetCore.Mvc;
using Onboard.Web.Application;

namespace Onboard.Web.Controllers.Api;

/// <summary>Provides operational metrics, audit exports, and regulator reports.</summary>
[ApiController]
[Route("api/reporting")]
public sealed class ReportingController(IOnboardingOrchestrator orchestrator) : ControllerBase
{
    /// <summary>Returns case counts broken down by status.</summary>
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

    /// <summary>Exports immutable audit events, optionally filtered to a specific case.</summary>
    [HttpGet("audit-export")]
    public IActionResult AuditExport([FromQuery] string? caseId) => Ok(orchestrator.GetAuditEvents(caseId));

    /// <summary>Returns a timestamped regulator-ready audit export.</summary>
    [HttpGet("regulator-export")]
    public IActionResult RegulatorExport([FromQuery] string? caseId)
        => Ok(new { generatedAt = DateTimeOffset.UtcNow, events = orchestrator.GetAuditEvents(caseId) });
}

