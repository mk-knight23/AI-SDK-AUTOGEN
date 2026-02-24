using AutoGenBackend.Models;

namespace AutoGenBackend.Repositories;

/// <summary>
/// In-memory conversation repository with message storage
/// </summary>
public class InMemoryConversationRepository : InMemoryRepository<Conversation>, IConversationRepository
{
    private readonly ConcurrentDictionary<Guid, ConcurrentBag<Message>> _messages = new();

    public Task<IEnumerable<Conversation>> GetByParticipantAsync(Guid agentId, CancellationToken cancellationToken = default)
    {
        var conversations = _storage.Values
            .Where(c => c.ParticipantIds.Contains(agentId));
        return Task.FromResult(conversations);
    }

    public Task<IEnumerable<Message>> GetMessagesAsync(Guid conversationId, int skip = 0, int take = 100, CancellationToken cancellationToken = default)
    {
        if (!_messages.TryGetValue(conversationId, out var messages))
        {
            return Task.FromResult(Enumerable.Empty<Message>());
        }

        return Task.FromResult(messages
            .OrderBy(m => m.Timestamp)
            .Skip(skip)
            .Take(take)
            .AsEnumerable());
    }

    public Task<Message> AddMessageAsync(Message message, CancellationToken cancellationToken = default)
    {
        if (message.Id == Guid.Empty)
        {
            message.Id = Guid.NewGuid();
        }

        var messages = _messages.GetOrAdd(message.ConversationId, _ => new ConcurrentBag<Message>());
        messages.Add(message);
        return Task.FromResult(message);
    }

    public Task<IEnumerable<Conversation>> GetByTypeAsync(ConversationType type, CancellationToken cancellationToken = default)
    {
        var conversations = _storage.Values.Where(c => c.Type == type);
        return Task.FromResult(conversations);
    }
}
