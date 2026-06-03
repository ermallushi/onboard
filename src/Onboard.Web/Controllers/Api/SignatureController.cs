using Microsoft.AspNetCore.Mvc;
using Onboard.Web.Application;

namespace Onboard.Web.Controllers.Api;

public sealed record CreateSignatureRequest(string DocumentReference, string SignatureLevel, string SignerAuthMethod);

[ApiController]
[Route("api/signature")]
public sealed class SignatureController(IOnboardingOrchestrator orchestrator) : ControllerBase
{
    [HttpPost("{caseId}/package")]
    public IActionResult CreatePackage(string caseId, [FromBody] CreateSignatureRequest request)
        => Ok(orchestrator.CreateSignaturePackage(caseId, request.DocumentReference, request.SignatureLevel, request.SignerAuthMethod));

    [HttpPost("{caseId}/send")]
    public IActionResult SendForSignature(string caseId) => Ok(orchestrator.SendForSignature(caseId));

    [HttpGet("{caseId}/status")]
    public IActionResult GetStatus(string caseId)
    {
        var result = orchestrator.GetSignatureStatus(caseId);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("{caseId}/evidence")]
    public IActionResult GetEvidence(string caseId)
    {
        var evidence = orchestrator.GetSignatureEvidence(caseId);
        return evidence is null ? NotFound() : Ok(new { evidenceReference = evidence });
    }
}
