namespace AutoGenBackend.Models;

/// <summary>
/// Represents a group chat configuration
/// </summary>
public class GroupChat
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<Guid> ParticipantIds { get; set; } = new();
    public GroupChatPattern Pattern { get; set; } = GroupChatPattern.RoundRobin;
    public List<ChatRule> Rules { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}

/// <summary>
/// Group chat routing patterns
/// </summary>
public enum GroupChatPattern
{
    RoundRobin,
    Broadcast,
    Supervised,
    Router,
    FreeForAll,
    SpeakerSelection
}

/// <summary>
/// Routing rules for group chat
/// </summary>
public class ChatRule
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public RuleCondition Condition { get; set; } = new();
    public RuleAction Action { get; set; } = new();
}

/// <summary>
/// Condition for rule evaluation
/// </summary>
public class RuleCondition
{
    public string? SenderAgentId { get; set; }
    public string? TargetAgentId { get; set; }
    public string? Keyword { get; set; }
    public string? MessageType { get; set; }
}

/// <summary>
/// Action to take when rule matches
/// </summary>
public class RuleAction
{
    public ActionType Type { get; set; } = ActionType.RouteToAgent;
    public string? TargetAgentId { get; set; }
    public List<string>? TargetAgentIds { get; set; }
    public bool ShouldBroadcast { get; set; }
}

/// <summary>
/// Action types for rules
/// </summary>
public enum ActionType
{
    RouteToAgent,
    Broadcast,
    Skip,
    Terminate
}

/// <summary>
/// Represents a message in a group chat
/// </summary>
public class GroupChatMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid GroupChatId { get; set; }
    public Guid SourceAgentId { get; set; }
    public List<Guid> TargetAgentIds { get; set; } = new();
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public int SequenceNumber { get; set; }
}
