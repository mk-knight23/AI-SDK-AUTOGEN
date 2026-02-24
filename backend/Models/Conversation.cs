namespace AutoGenBackend.Models;

/// <summary>
/// Represents a conversation between agents
/// </summary>
public class Conversation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public List<Guid> ParticipantIds { get; set; } = new();
    public ConversationType Type { get; set; } = ConversationType.OneToOne;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}

/// <summary>
/// Conversation type classification
/// </summary>
public enum ConversationType
{
    OneToOne,
    GroupChat,
    Broadcast,
    RoundRobin,
    Supervised
}

/// <summary>
/// Represents a message in a conversation
/// </summary>
public class Message
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ConversationId { get; set; }
    public Guid SourceAgentId { get; set; }
    public Guid? TargetAgentId { get; set; }
    public string Content { get; set; } = string.Empty;
    public MessageType Type { get; set; } = MessageType.Text;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Dictionary<string, string> Metadata { get; set; } = new();
}

/// <summary>
/// Message type classification
/// </summary>
public enum MessageType
{
    Text,
    Code,
    Image,
    ToolCall,
    ToolResult,
    System
}
