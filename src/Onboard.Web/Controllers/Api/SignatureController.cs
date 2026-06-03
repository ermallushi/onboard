using Microsoft.AspNetCore.Mvc;
using Onboard.Web.Application;

namespace Onboard.Web.Controllers.Api;

public sealed record CreateSignatureRequest(string DocumentReference, string SignatureLevel, string SignerAuthMethod);

/// <summary>Controls digital signature workflows, status tracking, and evidence retrieval.</summary>
[ApiController]
[Route("api/signature")]
public sealed class SignatureController(IOnboardingOrchestrator orchestrator) : ControllerBase
{
    /// <summary>Creates a signable document package for an approved case. Case must be in ApprovedForSigning status.</summary>
    [HttpPost("{caseId}/package")]
    public IActionResult CreatePackage(string caseId, [FromBody] CreateSignatureRequest request)
        => Ok(orchestrator.CreateSignaturePackage(caseId, request.DocumentReference, request.SignatureLevel, request.SignerAuthMethod));

    /// <summary>Sends the signature package to the provider and transitions the case to SignaturePending.</summary>
    [HttpPost("{caseId}/send")]
    public IActionResult SendForSignature(string caseId) => Ok(orchestrator.SendForSignature(caseId));

    /// <summary>Returns the current signature status for the case.</summary>
    [HttpGet("{caseId}/status")]
    public IActionResult GetStatus(string caseId)
    {
        var result = orchestrator.GetSignatureStatus(caseId);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Returns the signed evidence package reference after signature completion.</summary>
    [HttpGet("{caseId}/evidence")]
    public IActionResult GetEvidence(string caseId)
    {
        var evidence = orchestrator.GetSignatureEvidence(caseId);
        return evidence is null ? NotFound() : Ok(new { evidenceReference = evidence });
    }
}

