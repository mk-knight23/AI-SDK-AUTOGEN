using Microsoft.AspNetCore.Mvc;
using AutoGenBackend.Models;
using AutoGenBackend.Services;

namespace AutoGenBackend.Controllers;

/// <summary>
/// Controller for conversation management
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ConversationsController : ControllerBase
{
    private readonly ConversationService _conversationService;

    public ConversationsController(ConversationService conversationService)
    {
        _conversationService = conversationService;
    }

    /// <summary>
    /// Get all conversations
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Conversation>>> GetAll(CancellationToken cancellationToken)
    {
        var conversations = await _conversationService.GetAllAsync(cancellationToken);
        return Ok(conversations);
    }

    /// <summary>
    /// Get conversation by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Conversation>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var conversation = await _conversationService.GetByIdAsync(id, cancellationToken);
        if (conversation == null)
            return NotFound(new { Error = $"Conversation with ID {id} not found" });

        return Ok(conversation);
    }

    /// <summary>
    /// Get messages for a conversation
    /// </summary>
    [HttpGet("{id}/messages")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<Message>>> GetMessages(
        Guid id,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 100,
        CancellationToken cancellationToken = default)
    {
        var conversation = await _conversationService.GetByIdAsync(id, cancellationToken);
        if (conversation == null)
            return NotFound(new { Error = $"Conversation with ID {id} not found" });

        var messages = await _conversationService.GetMessagesAsync(id, skip, take, cancellationToken);
        return Ok(messages);
    }

    /// <summary>
    /// Create new conversation
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Conversation>> Create(
        [FromBody] Conversation conversation,
        CancellationToken cancellationToken)
    {
        try
        {
            var created = await _conversationService.CreateAsync(conversation, cancellationToken);
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
    /// Send message to conversation
    /// </summary>
    [HttpPost("{id}/messages")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Message>> SendMessage(
        Guid id,
        [FromBody] SendMessageRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var message = await _conversationService.SendMessageAsync(
                id,
                request.SourceAgentId,
                request.Content,
                request.TargetAgentId,
                cancellationToken);

            return CreatedAtAction(
                nameof(GetMessages),
                new { id },
                message);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Update conversation
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Conversation>> Update(
        Guid id,
        [FromBody] Conversation conversation,
        CancellationToken cancellationToken)
    {
        try
        {
            var updated = await _conversationService.UpdateAsync(id, conversation, cancellationToken);
            return Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Delete conversation
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _conversationService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Get conversations by participant
    /// </summary>
    [HttpGet("by-participant/{agentId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Conversation>>> GetByParticipant(
        Guid agentId,
        CancellationToken cancellationToken)
    {
        var conversations = await _conversationService.GetByParticipantAsync(agentId, cancellationToken);
        return Ok(conversations);
    }
}

/// <summary>
/// Request to send a message
/// </summary>
public class SendMessageRequest
{
    public Guid SourceAgentId { get; set; }
    public string Content { get; set; } = string.Empty;
    public Guid? TargetAgentId { get; set; }
}
