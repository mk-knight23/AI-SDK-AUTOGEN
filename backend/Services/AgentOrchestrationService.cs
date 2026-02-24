using AutoGenBackend.Models;

namespace AutoGenBackend.Services;

/// <summary>
/// Service for orchestrating multi-agent workflows using AutoGen
/// </summary>
public class AgentOrchestrationService
{
    private readonly Dictionary<string, AgentDefinition> _registeredAgents = new();

    /// <summary>
    /// Register an agent for orchestration
    /// </summary>
    public void RegisterAgent(AgentDefinition definition)
    {
        if (string.IsNullOrWhiteSpace(definition.Name))
            throw new ArgumentException("Agent name is required", nameof(definition));

        _registeredAgents[definition.Name] = definition;
    }

    /// <summary>
    /// Unregister an agent
    /// </summary>
    public void UnregisterAgent(string name)
    {
        _registeredAgents.Remove(name);
    }

    /// <summary>
    /// Get all registered agents
    /// </summary>
    public IEnumerable<AgentDefinition> GetRegisteredAgents()
    {
        return _registeredAgents.Values;
    }

    /// <summary>
    /// Get registered agent by name
    /// </summary>
    public AgentDefinition? GetAgent(string name)
    {
        _registeredAgents.TryGetValue(name, out var agent);
        return agent;
    }

    /// <summary>
    /// Create a team configuration
    /// </summary>
    public TeamConfiguration CreateTeam(string name, TeamPattern pattern, params string[] agentNames)
    {
        var agents = new List<AgentDefinition>();
        foreach (var agentName in agentNames)
        {
            var agent = GetAgent(agentName);
            if (agent == null)
                throw new InvalidOperationException($"Agent '{agentName}' not found");
            agents.Add(agent);
        }

        return new TeamConfiguration
        {
            Name = name,
            Pattern = pattern,
            Participants = agents,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Run a team workflow (simplified mock)
    /// </summary>
    public async Task<TeamExecutionResult> RunTeamAsync(
        TeamConfiguration team,
        string task,
        CancellationToken cancellationToken = default)
    {
        var result = new TeamExecutionResult
        {
            TeamName = team.Name,
            Task = task,
            StartedAt = DateTime.UtcNow,
            Messages = new List<TeamMessage>()
        };

        // Mock execution based on pattern
        switch (team.Pattern)
        {
            case TeamPattern.RoundRobin:
                result = await RunRoundRobinAsync(team, task, cancellationToken);
                break;
            case TeamPattern.Broadcast:
                result = await RunBroadcastAsync(team, task, cancellationToken);
                break;
            case TeamPattern.Supervised:
                result = await RunSupervisedAsync(team, task, cancellationToken);
                break;
            default:
                result = await RunRoundRobinAsync(team, task, cancellationToken);
                break;
        }

        result.CompletedAt = DateTime.UtcNow;
        return result;
    }

    private async Task<TeamExecutionResult> RunRoundRobinAsync(
        TeamConfiguration team,
        string task,
        CancellationToken cancellationToken)
    {
        var result = new TeamExecutionResult
        {
            TeamName = team.Name,
            Task = task,
            StartedAt = DateTime.UtcNow,
            Messages = new List<TeamMessage>()
        };

        // Simulate round-robin conversation
        for (int i = 0; i < team.Participants.Count; i++)
        {
            var agent = team.Participants[i];
            var message = new TeamMessage
            {
                AgentName = agent.Name,
                Content = $"[{agent.Name}] Processing task: {task}",
                Timestamp = DateTime.UtcNow
            };
            result.Messages.Add(message);

            await Task.Delay(50, cancellationToken);
        }

        result.Status = TeamExecutionStatus.Completed;
        return result;
    }

    private async Task<TeamExecutionResult> RunBroadcastAsync(
        TeamConfiguration team,
        string task,
        CancellationToken cancellationToken)
    {
        var result = new TeamExecutionResult
        {
            TeamName = team.Name,
            Task = task,
            StartedAt = DateTime.UtcNow,
            Messages = new List<TeamMessage>()
        };

        // All agents respond to the task
        foreach (var agent in team.Participants)
        {
            var message = new TeamMessage
            {
                AgentName = agent.Name,
                Content = $"[{agent.Name}] Responding to broadcast: {task}",
                Timestamp = DateTime.UtcNow
            };
            result.Messages.Add(message);

            await Task.Delay(50, cancellationToken);
        }

        result.Status = TeamExecutionStatus.Completed;
        return result;
    }

    private async Task<TeamExecutionResult> RunSupervisedAsync(
        TeamConfiguration team,
        string task,
        CancellationToken cancellationToken)
    {
        var result = new TeamExecutionResult
        {
            TeamName = team.Name,
            Task = task,
            StartedAt = DateTime.UtcNow,
            Messages = new List<TeamMessage>()
        };

        // Find supervisor (first agent with Supervisor type)
        var supervisor = team.Participants.FirstOrDefault(a => a.Type == AgentType.Supervisor)
                        ?? team.Participants.First();

        // Supervisor delegates to workers
        var supervisorMessage = new TeamMessage
        {
            AgentName = supervisor.Name,
            Content = $"[{supervisor.Name}] Delegating task: {task}",
            Timestamp = DateTime.UtcNow
        };
        result.Messages.Add(supervisorMessage);

        var workers = team.Participants.Where(a => a.Id != supervisor.Id);
        foreach (var worker in workers)
        {
            var message = new TeamMessage
            {
                AgentName = worker.Name,
                Content = $"[{worker.Name}] Working on delegated task",
                Timestamp = DateTime.UtcNow
            };
            result.Messages.Add(message);

            await Task.Delay(50, cancellationToken);
        }

        result.Status = TeamExecutionStatus.Completed;
        return result;
    }
}

/// <summary>
/// Agent definition for orchestration
/// </summary>
public class AgentDefinition
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public AgentType Type { get; set; } = AgentType.Assistant;
    public string SystemMessage { get; set; } = string.Empty;
    public ModelConfiguration ModelConfiguration { get; set; } = new();
    public Dictionary<string, string> Capabilities { get; set; } = new();
}

/// <summary>
/// Model configuration for agents
/// </summary>
public class ModelConfiguration
{
    public string Provider { get; set; } = "openai";
    public string Model { get; set; } = "gpt-4o-mini";
    public string ApiKey { get; set; } = string.Empty;
    public double Temperature { get; set; } = 0.7;
    public int MaxTokens { get; set; } = 2048;
}

/// <summary>
/// Team configuration
/// </summary>
public class TeamConfiguration
{
    public string Name { get; set; } = string.Empty;
    public TeamPattern Pattern { get; set; } = TeamPattern.RoundRobin;
    public List<AgentDefinition> Participants { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public Dictionary<string, object> Settings { get; set; } = new();
}

/// <summary>
/// Team execution patterns
/// </summary>
public enum TeamPattern
{
    RoundRobin,
    Broadcast,
    Supervised,
    Router,
    FreeForAll
}

/// <summary>
/// Team execution result
/// </summary>
public class TeamExecutionResult
{
    public string TeamName { get; set; } = string.Empty;
    public string Task { get; set; } = string.Empty;
    public TeamExecutionStatus Status { get; set; }
    public List<TeamMessage> Messages { get; set; } = new();
    public DateTime StartedAt { get; set; }
    public DateTime CompletedAt { get; set; }
    public string? Summary { get; set; }
}

/// <summary>
/// Message in team execution
/// </summary>
public class TeamMessage
{
    public string AgentName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Team execution status
/// </summary>
public enum TeamExecutionStatus
{
    Running,
    Completed,
    Failed,
    Cancelled
}
