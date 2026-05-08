using ChatAndEvents.Data.EventsData.Models;
using ChatAndEvents.Data.EventsData.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ChatAndEvents.API.Server.Controllers.Events;

// 1. We create a DTO class to wrap the two objects together.
// (You can leave this here, or move it to a dedicated 'DTOs' folder later!)
public class SubmitProofRequest
{
    public Quest Quest { get; set; } = null!;
    public Memory Proof { get; set; } = null!;
}

[ApiController]
[Route("api/[controller]")]
public class QuestApprovalController : ControllerBase
{
    private readonly IQuestApprovalService _questApprovalService;

    public QuestApprovalController(IQuestApprovalService questApprovalService)
    {
        _questApprovalService = questApprovalService;
    }

    // 2. We change the endpoint to accept the single wrapper object
    [HttpPost("submit")]
    public async Task<IActionResult> SubmitProof([FromBody] SubmitProofRequest request)
    {
        // 3. Unpack the wrapper and send it to your service as usual
        await _questApprovalService.SubmitProofAsync(request.Quest, request.Proof);
        return NoContent();
    }

    [HttpGet("{eventId}/status")]
    public async Task<IActionResult> GetQuestsWithStatus(
        int eventId,
        [FromQuery] Guid userId)
    {
        var ev = new Event { EventId = eventId };
        var user = new User { UserId = userId };

        var result = await _questApprovalService
            .GetQuestsWithStatus(ev, user);

        return Ok(result);
    }

    [HttpGet("{questId}/proofs")]
    public async Task<IActionResult> GetProofsForQuest(int questId)
    {
        var quest = new Quest { Id = questId };

        var proofs = await _questApprovalService
            .GetProofsForQuestAsync(quest);

        return Ok(proofs);
    }

    [HttpPut("proof-status")]
    public async Task<IActionResult> ChangeProofStatus(
        [FromBody] QuestMemory proof)
    {
        await _questApprovalService.ChangeProofStatusAsync(proof);
        return NoContent();
    }

    [HttpDelete("submission")]
    public async Task<IActionResult> DeleteSubmission(
        [FromBody] QuestMemory proof,
        [FromQuery] Guid userId)
    {
        var user = new User { UserId = userId };

        await _questApprovalService.DeleteSubmissionAsync(proof, user);

        return NoContent();
    }
}