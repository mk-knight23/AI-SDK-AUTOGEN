using Microsoft.AspNetCore.Mvc;
using AutoGenBackend.Models;
using AutoGenBackend.Services;

namespace AutoGenBackend.Controllers;

/// <summary>
/// Controller for code execution
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CodeController : ControllerBase
{
    private readonly CodeExecutionService _codeExecutionService;

    public CodeController(CodeExecutionService codeExecutionService)
    {
        _codeExecutionService = codeExecutionService;
    }

    /// <summary>
    /// Execute code
    /// </summary>
    [HttpPost("execute")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CodeExecution>> Execute(
        [FromBody] CodeExecutionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var execution = await _codeExecutionService.ExecuteAsync(
                request.Language,
                request.Code,
                request.ConversationId,
                request.RequestingAgentId,
                request.TimeoutSeconds,
                request.ResourceLimits,
                cancellationToken);

            return AcceptedAtAction(nameof(GetById), new { id = execution.Id }, execution);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Get execution by ID
    /// </summary>
    [HttpGet("executions/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CodeExecution>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var execution = await _codeExecutionService.GetByIdAsync(id, cancellationToken);
        if (execution == null)
            return NotFound(new { Error = $"Execution with ID {id} not found" });

        return Ok(execution);
    }

    /// <summary>
    /// Get all executions
    /// </summary>
    [HttpGet("executions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CodeExecution>>> GetAll(CancellationToken cancellationToken)
    {
        var executions = await _codeExecutionService.GetAllAsync(cancellationToken);
        return Ok(executions);
    }

    /// <summary>
    /// Get recent executions
    /// </summary>
    [HttpGet("executions/recent")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CodeExecution>>> GetRecent(
        [FromQuery] int count = 50,
        CancellationToken cancellationToken = default)
    {
        var executions = await _codeExecutionService.GetRecentAsync(count, cancellationToken);
        return Ok(executions);
    }

    /// <summary>
    /// Get executions by conversation
    /// </summary>
    [HttpGet("conversations/{conversationId}/executions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CodeExecution>>> GetByConversation(
        Guid conversationId,
        CancellationToken cancellationToken)
    {
        var executions = await _codeExecutionService.GetByConversationAsync(conversationId, cancellationToken);
        return Ok(executions);
    }
}

/// <summary>
/// Request to execute code
/// </summary>
public class CodeExecutionRequest
{
    public CodeLanguage Language { get; set; }
    public string Code { get; set; } = string.Empty;
    public Guid? ConversationId { get; set; }
    public Guid? RequestingAgentId { get; set; }
    public int TimeoutSeconds { get; set; } = 30;
    public ResourceLimits? ResourceLimits { get; set; }
}
