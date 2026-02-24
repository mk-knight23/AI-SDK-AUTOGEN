using Microsoft.AspNetCore.Mvc;
using AutoGenBackend.Models;
using AutoGenBackend.Services;

namespace AutoGenBackend.Controllers;

/// <summary>
/// Controller for agent management
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AgentsController : ControllerBase
{
    private readonly AgentService _agentService;

    public AgentsController(AgentService agentService)
    {
        _agentService = agentService;
    }

    /// <summary>
    /// Get all agents
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Agent>>> GetAll(CancellationToken cancellationToken)
    {
        var agents = await _agentService.GetAllAsync(cancellationToken);
        return Ok(agents);
    }

    /// <summary>
    /// Get agent by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Agent>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var agent = await _agentService.GetByIdAsync(id, cancellationToken);
        if (agent == null)
            return NotFound(new { Error = $"Agent with ID {id} not found" });

        return Ok(agent);
    }

    /// <summary>
    /// Get active agents
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Agent>>> GetActive(CancellationToken cancellationToken)
    {
        var agents = await _agentService.GetActiveAsync(cancellationToken);
        return Ok(agents);
    }

    /// <summary>
    /// Get agents by type
    /// </summary>
    [HttpGet("by-type/{type}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Agent>>> GetByType(
        AgentType type,
        CancellationToken cancellationToken)
    {
        var agents = await _agentService.GetByTypeAsync(type, cancellationToken);
        return Ok(agents);
    }

    /// <summary>
    /// Create new agent
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Agent>> Create(
        [FromBody] Agent agent,
        CancellationToken cancellationToken)
    {
        try
        {
            var created = await _agentService.CreateAsync(agent, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Update agent
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Agent>> Update(
        Guid id,
        [FromBody] Agent agent,
        CancellationToken cancellationToken)
    {
        try
        {
            var updated = await _agentService.UpdateAsync(id, agent, cancellationToken);
            return Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Delete agent
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _agentService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Activate agent
    /// </summary>
    [HttpPost("{id}/activate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Agent>> Activate(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var agent = await _agentService.ActivateAsync(id, cancellationToken);
            return Ok(agent);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Deactivate agent
    /// </summary>
    [HttpPost("{id}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Agent>> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var agent = await _agentService.DeactivateAsync(id, cancellationToken);
            return Ok(agent);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
    }
}
