using Microsoft.AspNetCore.Mvc;
using Onboard.Web.Application;

namespace Onboard.Web.Controllers.Api;

public sealed record AssignReviewRequest(string QueueName, string Reason);
public sealed record AddReviewNoteRequest(string Note, string Actor);
public sealed record OverrideReviewRequest(string Decision, string Actor);

/// <summary>Supports manual review, exception handling, and compliance override operations.</summary>
[ApiController]
[Route("api/review")]
public sealed class ReviewController(IOnboardingOrchestrator orchestrator) : ControllerBase
{
    /// <summary>Assigns the case to a named review queue.</summary>
    [HttpPost("{caseId}/assign")]
    public IActionResult AssignQueue(string caseId, [FromBody] AssignReviewRequest request)
        => Ok(orchestrator.AssignReviewQueue(caseId, request.QueueName, request.Reason));

    /// <summary>Adds a reviewer note to the case review record.</summary>
    [HttpPost("{caseId}/note")]
    public IActionResult AddNote(string caseId, [FromBody] AddReviewNoteRequest request)
        => Ok(orchestrator.AddReviewNote(caseId, request.Note, request.Actor));

    /// <summary>Applies a compliance override decision to the review (approve or reject).</summary>
    [HttpPost("{caseId}/override")]
    public IActionResult OverrideDecision(string caseId, [FromBody] OverrideReviewRequest request)
    {
        orchestrator.OverrideReviewDecision(caseId, request.Decision, request.Actor);
        return NoContent();
    }
}

