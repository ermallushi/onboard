using Microsoft.AspNetCore.Mvc;
using Onboard.Web.Application;

namespace Onboard.Web.Controllers.Api;

[ApiController]
[Route("api/cases")]
public sealed class CasesController(IOnboardingOrchestrator orchestrator) : ControllerBase
{
    [HttpPost]
    public IActionResult CreateCase([FromBody] CreateCaseRequest request) => Ok(orchestrator.CreateCase(request));

    [HttpGet("{caseId}")]
    public IActionResult GetCase(string caseId)
    {
        var onboardingCase = orchestrator.GetCase(caseId);
        return onboardingCase is null ? NotFound() : Ok(onboardingCase);
    }

    [HttpGet]
    public IActionResult Search([FromQuery] string? customerReference) => Ok(orchestrator.SearchCases(customerReference));

    [HttpPost("{caseId}/resume")]
    public IActionResult ResumeCase(string caseId, [FromBody] ChannelTransitionRequest request) => Ok(orchestrator.TransitionChannel(caseId, request));

    [HttpPost("{caseId}/cancel")]
    public IActionResult CancelCase(string caseId, [FromQuery] string actorId)
    {
        orchestrator.CancelCase(caseId, actorId);
        return NoContent();
    }

    [HttpPost("{caseId}/channel-transition")]
    public IActionResult ChannelTransition(string caseId, [FromBody] ChannelTransitionRequest request) => Ok(orchestrator.TransitionChannel(caseId, request));
}
