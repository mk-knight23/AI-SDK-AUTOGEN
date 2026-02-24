using Microsoft.AspNetCore.Mvc;
using AutoGenBackend.Models;
using AutoGenBackend.Services;

namespace AutoGenBackend.Controllers;

/// <summary>
/// Controller for group chat management
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class GroupsController : ControllerBase
{
    private readonly GroupChatService _groupChatService;

    public GroupsController(GroupChatService groupChatService)
    {
        _groupChatService = groupChatService;
    }

    /// <summary>
    /// Get all group chats
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<GroupChat>>> GetAll(CancellationToken cancellationToken)
    {
        var groups = await _groupChatService.GetAllAsync(cancellationToken);
        return Ok(groups);
    }

    /// <summary>
    /// Get group chat by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GroupChat>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var group = await _groupChatService.GetByIdAsync(id, cancellationToken);
        if (group == null)
            return NotFound(new { Error = $"Group chat with ID {id} not found" });

        return Ok(group);
    }

    /// <summary>
    /// Get messages for group chat
    /// </summary>
    [HttpGet("{id}/messages")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<GroupChatMessage>>> GetMessages(
        Guid id,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 100,
        CancellationToken cancellationToken = default)
    {
        var group = await _groupChatService.GetByIdAsync(id, cancellationToken);
        if (group == null)
            return NotFound(new { Error = $"Group chat with ID {id} not found" });

        var messages = await _groupChatService.GetMessagesAsync(id, skip, take, cancellationToken);
        return Ok(messages);
    }

    /// <summary>
    /// Create new group chat
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GroupChat>> Create(
        [FromBody] GroupChat groupChat,
        CancellationToken cancellationToken)
    {
        try
        {
            var created = await _groupChatService.CreateAsync(groupChat, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Send message to group chat
    /// </summary>
    [HttpPost("{id}/messages")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GroupChatMessage>> SendMessage(
        Guid id,
        [FromBody] SendGroupMessageRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var message = await _groupChatService.SendMessageAsync(
                id,
                request.SourceAgentId,
                request.Content,
                cancellationToken);

            return CreatedAtAction(nameof(GetMessages), new { id }, message);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Update group chat
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GroupChat>> Update(
        Guid id,
        [FromBody] GroupChat groupChat,
        CancellationToken cancellationToken)
    {
        try
        {
            var updated = await _groupChatService.UpdateAsync(id, groupChat, cancellationToken);
            return Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Delete group chat
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _groupChatService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Add participant to group chat
    /// </summary>
    [HttpPost("{id}/participants/{agentId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GroupChat>> AddParticipant(
        Guid id,
        Guid agentId,
        CancellationToken cancellationToken)
    {
        try
        {
            var group = await _groupChatService.AddParticipantAsync(id, agentId, cancellationToken);
            return Ok(group);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Remove participant from group chat
    /// </summary>
    [HttpDelete("{id}/participants/{agentId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GroupChat>> RemoveParticipant(
        Guid id,
        Guid agentId,
        CancellationToken cancellationToken)
    {
        try
        {
            var group = await _groupChatService.RemoveParticipantAsync(id, agentId, cancellationToken);
            return Ok(group);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Get group chats by participant
    /// </summary>
    [HttpGet("by-participant/{agentId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<GroupChat>>> GetByParticipant(
        Guid agentId,
        CancellationToken cancellationToken)
    {
        var groups = await _groupChatService.GetByParticipantAsync(agentId, cancellationToken);
        return Ok(groups);
    }
}

/// <summary>
/// Request to send a group message
/// </summary>
public class SendGroupMessageRequest
{
    public Guid SourceAgentId { get; set; }
    public string Content { get; set; } = string.Empty;
}
