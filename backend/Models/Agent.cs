namespace AutoGenBackend.Models;

/// <summary>
/// Represents an AI agent in the multi-agent system
/// </summary>
public class Agent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public AgentType Type { get; set; } = AgentType.Assistant;
    public string SystemMessage { get; set; } = string.Empty;
    public ModelConfig ModelConfig { get; set; } = new();
    public AgentStatus Status { get; set; } = AgentStatus.Inactive;
    public Dictionary<string, string> Metadata { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastActivatedAt { get; set; }
}

/// <summary>
/// Agent type classification
/// </summary>
public enum AgentType
{
    System,
    User,
    Assistant,
    Tool,
    Supervisor,
    Router
}

/// <summary>
/// Agent operational status
/// </summary>
public enum AgentStatus
{
    Active,
    Inactive,
    Error,
    Busy,
    Terminated
}

/// <summary>
/// LLM model configuration for an agent
/// </summary>
public class ModelConfig
{
    public string Provider { get; set; } = "openai";
    public string Model { get; set; } = "gpt-4o-mini";
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public double Temperature { get; set; } = 0.7;
    public int MaxTokens { get; set; } = 2048;
    public int? Seed { get; set; }
}
