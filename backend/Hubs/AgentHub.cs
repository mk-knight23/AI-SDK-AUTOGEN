using Microsoft.AspNetCore.SignalR;

namespace AutoGenBackend.Hubs;

/// <summary>
/// SignalR hub for real-time agent communication
/// </summary>
public class AgentHub : Hub
{
    private readonly ILogger<AgentHub> _logger;
    private readonly ConnectionMapping _connections;

    public AgentHub(ILogger<AgentHub> logger, ConnectionMapping connections)
    {
        _logger = logger;
        _connections = connections;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _connections.Remove(Context.ConnectionId);
        _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Join a conversation
    /// </summary>
    public async Task JoinConversation(string conversationId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"conversation:{conversationId}");
        _connections.Add(Context.ConnectionId, $"conversation:{conversationId}");
        _logger.LogInformation("Client {ConnectionId} joined conversation {ConversationId}",
            Context.ConnectionId, conversationId);
    }

    /// <summary>
    /// Leave a conversation
    /// </summary>
    public async Task LeaveConversation(string conversationId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conversation:{conversationId}");
        _connections.Remove(Context.ConnectionId, $"conversation:{conversationId}");
        _logger.LogInformation("Client {ConnectionId} left conversation {ConversationId}",
            Context.ConnectionId, conversationId);
    }

    /// <summary>
    /// Join a group chat
    /// </summary>
    public async Task JoinGroupChat(string groupChatId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"groupchat:{groupChatId}");
        _connections.Add(Context.ConnectionId, $"groupchat:{groupChatId}");
        _logger.LogInformation("Client {ConnectionId} joined group chat {GroupChatId}",
            Context.ConnectionId, groupChatId);
    }

    /// <summary>
    /// Leave a group chat
    /// </summary>
    public async Task LeaveGroupChat(string groupChatId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"groupchat:{groupChatId}");
        _connections.Remove(Context.ConnectionId, $"groupchat:{groupChatId}");
        _logger.LogInformation("Client {ConnectionId} left group chat {GroupChatId}",
            Context.ConnectionId, groupChatId);
    }

    /// <summary>
    /// Send message to conversation
    /// </summary>
    public async Task SendMessageToConversation(string conversationId, string agentId, string content)
    {
        await Clients.Group($"conversation:{conversationId}")
            .SendAsync("MessageReceived", new
            {
                ConversationId = conversationId,
                AgentId = agentId,
                Content = content,
                Timestamp = DateTime.UtcNow
            });
    }

    /// <summary>
    /// Send message to group chat
    /// </summary>
    public async Task SendMessageToGroupChat(string groupChatId, string agentId, string content)
    {
        await Clients.Group($"groupchat:{groupChatId}")
            .SendAsync("GroupMessageReceived", new
            {
                GroupChatId = groupChatId,
                AgentId = agentId,
                Content = content,
                Timestamp = DateTime.UtcNow
            });
    }

    /// <summary>
    /// Notify agent status change
    /// </summary>
    public async Task NotifyAgentStatusChanged(string agentId, string status)
    {
        await Clients.All.SendAsync("AgentStatusChanged", new
        {
            AgentId = agentId,
            Status = status,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Notify code execution completed
    /// </summary>
    public async Task NotifyExecutionCompleted(string executionId, object result)
    {
        await Clients.All.SendAsync("ExecutionCompleted", new
        {
            ExecutionId = executionId,
            Result = result,
            Timestamp = DateTime.UtcNow
        });
    }
}

/// <summary>
/// Tracks connection to group mappings
/// </summary>
public class ConnectionMapping
{
    private readonly Dictionary<string, HashSet<string>> _connections = new();

    public int Count => _connections.Count;

    public void Add(string connectionId, string group)
    {
        if (!_connections.TryGetValue(connectionId, out var groups))
        {
            groups = new HashSet<string>();
            _connections[connectionId] = groups;
        }
        groups.Add(group);
    }

    public void Remove(string connectionId, string group)
    {
        if (_connections.TryGetValue(connectionId, out var groups))
        {
            groups.Remove(group);
            if (groups.Count == 0)
            {
                _connections.Remove(connectionId);
            }
        }
    }

    public void Remove(string connectionId)
    {
        _connections.Remove(connectionId);
    }

    public IEnumerable<string> GetConnections(string group)
    {
        return _connections
            .Where(kvp => kvp.Value.Contains(group))
            .Select(kvp => kvp.Key);
    }
}
