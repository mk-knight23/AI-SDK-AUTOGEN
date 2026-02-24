using Microsoft.AspNetCore.Mvc;
using AutoGenBackend.Services;

namespace AutoGenBackend.Controllers;

/// <summary>
/// Controller for AutoGen orchestration
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AutoGenController : ControllerBase
{
    private readonly AgentOrchestrationService _orchestrationService;

    public AutoGenController(AgentOrchestrationService orchestrationService)
    {
        _orchestrationService = orchestrationService;
    }

    /// <summary>
    /// Register an agent for orchestration
    /// </summary>
    [HttpPost("agents")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult RegisterAgent([FromBody] AgentDefinitionRequest request)
    {
        try
        {
            var definition = new AgentDefinition
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Type = request.Type,
                SystemMessage = request.SystemMessage,
                ModelConfiguration = request.ModelConfiguration ?? new ModelConfiguration(),
                Capabilities = request.Capabilities ?? new Dictionary<string, string>()
            };

            _orchestrationService.RegisterAgent(definition);
            return CreatedAtAction(nameof(GetAgent), new { name = definition.Name }, definition);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Get registered agent
    /// </summary>
    [HttpGet("agents/{name}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetAgent(string name)
    {
        var agent = _orchestrationService.GetAgent(name);
        if (agent == null)
            return NotFound(new { Error = $"Agent '{name}' not found" });

        return Ok(agent);
    }

    /// <summary>
    /// Get all registered agents
    /// </summary>
    [HttpGet("agents")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetAgents()
    {
        var agents = _orchestrationService.GetRegisteredAgents();
        return Ok(agents);
    }

    /// <summary>
    /// Unregister an agent
    /// </summary>
    [HttpDelete("agents/{name}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult UnregisterAgent(string name)
    {
        _orchestrationService.UnregisterAgent(name);
        return NoContent();
    }

    /// <summary>
    /// Create a team
    /// </summary>
    [HttpPost("teams")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult CreateTeam([FromBody] CreateTeamRequest request)
    {
        try
        {
            var team = _orchestrationService.CreateTeam(
                request.Name,
                request.Pattern,
                request.ParticipantNames.ToArray());

            return CreatedAtAction(nameof(GetTeam), new { name = team.Name }, team);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Get a team (placeholder for future implementation)
    /// </summary>
    [HttpGet("teams/{name}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetTeam(string name)
    {
        // Team storage would be implemented in a full version
        return Ok(new { Name = name, Message = "Team retrieval not yet implemented" });
    }

    /// <summary>
    /// Run a team workflow
    /// </summary>
    [HttpPost("teams/{name}/run")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RunTeam(
        string name,
        [FromBody] RunTeamRequest request,
        CancellationToken cancellationToken)
    {
        // In a full implementation, we would retrieve the team configuration
        // For now, create a simple team from the request
        try
        {
            var team = _orchestrationService.CreateTeam(
                name,
                request.Pattern,
                request.ParticipantNames?.ToArray() ?? Array.Empty<string>());

            var result = await _orchestrationService.RunTeamAsync(
                team,
                request.Task,
                cancellationToken);

            return Accepted(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Get team status (placeholder)
    /// </summary>
    [HttpGet("teams/{name}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetTeamStatus(string name)
    {
        return Ok(new
        {
            TeamName = name,
            Status = "Running",
            Message = "Team status tracking not yet fully implemented"
        });
    }
}

/// <summary>
/// Request to register an agent
/// </summary>
public class AgentDefinitionRequest
{
    public string Name { get; set; } = string.Empty;
    public AgentType Type { get; set; } = AgentType.Assistant;
    public string SystemMessage { get; set; } = string.Empty;
    public ModelConfiguration? ModelConfiguration { get; set; }
    public Dictionary<string, string>? Capabilities { get; set; }
}

/// <summary>
/// Request to create a team
/// </summary>
public class CreateTeamRequest
{
    public string Name { get; set; } = string.Empty;
    public TeamPattern Pattern { get; set; } = TeamPattern.RoundRobin;
    public List<string> ParticipantNames { get; set; } = new();
}

/// <summary>
/// Request to run a team
/// </summary>
public class RunTeamRequest
{
    public string Task { get; set; } = string.Empty;
    public TeamPattern Pattern { get; set; } = TeamPattern.RoundRobin;
    public List<string>? ParticipantNames { get; set; }
}
