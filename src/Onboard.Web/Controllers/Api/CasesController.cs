using Microsoft.AspNetCore.Mvc;
using Onboard.Web.Application;

namespace Onboard.Web.Controllers.Api;

/// <summary>Manages the onboarding case lifecycle and cross-channel continuity.</summary>
[ApiController]
[Route("api/cases")]
public sealed class CasesController(IOnboardingOrchestrator orchestrator) : ControllerBase
{
    /// <summary>Creates a new onboarding case.</summary>
    [HttpPost]
    public IActionResult CreateCase([FromBody] CreateCaseRequest request) => Ok(orchestrator.CreateCase(request));

    /// <summary>Gets a specific onboarding case by ID.</summary>
    [HttpGet("{caseId}")]
    public IActionResult GetCase(string caseId)
    {
        var onboardingCase = orchestrator.GetCase(caseId);
        return onboardingCase is null ? NotFound() : Ok(onboardingCase);
    }

    /// <summary>Searches cases by optional customer reference.</summary>
    [HttpGet]
    public IActionResult Search([FromQuery] string? customerReference) => Ok(orchestrator.SearchCases(customerReference));

    /// <summary>Resumes a case by transitioning it to a new channel.</summary>
    [HttpPost("{caseId}/resume")]
    public IActionResult ResumeCase(string caseId, [FromBody] ChannelTransitionRequest request) => Ok(orchestrator.TransitionChannel(caseId, request));

    /// <summary>Cancels an in-progress onboarding case.</summary>
    [HttpPost("{caseId}/cancel")]
    public IActionResult CancelCase(string caseId, [FromQuery] string actorId)
    {
        orchestrator.CancelCase(caseId, actorId);
        return NoContent();
    }

    /// <summary>Records a channel transition (e.g. digital → store).</summary>
    [HttpPost("{caseId}/channel-transition")]
    public IActionResult ChannelTransition(string caseId, [FromBody] ChannelTransitionRequest request) => Ok(orchestrator.TransitionChannel(caseId, request));
}

