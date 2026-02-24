using AutoGenBackend.Models;

namespace AutoGenBackend.Repositories;

/// <summary>
/// In-memory group chat repository
/// </summary>
public class InMemoryGroupChatRepository : InMemoryRepository<GroupChat>, IGroupChatRepository
{
    private readonly ConcurrentDictionary<Guid, ConcurrentBag<GroupChatMessage>> _messages = new();

    public Task<IEnumerable<GroupChat>> GetByParticipantAsync(Guid agentId, CancellationToken cancellationToken = default)
    {
        var chats = _storage.Values.Where(c => c.ParticipantIds.Contains(agentId));
        return Task.FromResult(chats);
    }

    public Task<IEnumerable<GroupChatMessage>> GetMessagesAsync(Guid groupChatId, int skip = 0, int take = 100, CancellationToken cancellationToken = default)
    {
        if (!_messages.TryGetValue(groupChatId, out var messages))
        {
            return Task.FromResult(Enumerable.Empty<GroupChatMessage>());
        }

        return Task.FromResult(messages
            .OrderBy(m => m.SequenceNumber)
            .Skip(skip)
            .Take(take)
            .AsEnumerable());
    }

    public Task<GroupChatMessage> AddMessageAsync(GroupChatMessage message, CancellationToken cancellationToken = default)
    {
        if (message.Id == Guid.Empty)
        {
            message.Id = Guid.NewGuid();
        }

        var messages = _messages.GetOrAdd(message.GroupChatId, _ => new ConcurrentBag<GroupChatMessage>());

        // Set sequence number
        var sequence = messages.Count + 1;
        message.SequenceNumber = sequence;

        messages.Add(message);
        return Task.FromResult(message);
    }

    public Task<IEnumerable<GroupChat>> GetByPatternAsync(GroupChatPattern pattern, CancellationToken cancellationToken = default)
    {
        var chats = _storage.Values.Where(c => c.Pattern == pattern);
        return Task.FromResult(chats);
    }

    public async Task<GroupChat> AddParticipantAsync(Guid groupChatId, Guid agentId, CancellationToken cancellationToken = default)
    {
        var chat = await GetByIdAsync(groupChatId, cancellationToken);
        if (chat == null)
            throw new InvalidOperationException($"Group chat with ID {groupChatId} not found");

        if (!chat.ParticipantIds.Contains(agentId))
        {
            chat.ParticipantIds.Add(agentId);
            chat.UpdatedAt = DateTime.UtcNow;
            return await UpdateAsync(chat, cancellationToken);
        }

        return chat;
    }

    public async Task<GroupChat> RemoveParticipantAsync(Guid groupChatId, Guid agentId, CancellationToken cancellationToken = default)
    {
        var chat = await GetByIdAsync(groupChatId, cancellationToken);
        if (chat == null)
            throw new InvalidOperationException($"Group chat with ID {groupChatId} not found");

        chat.ParticipantIds.Remove(agentId);
        chat.UpdatedAt = DateTime.UtcNow;
        return await UpdateAsync(chat, cancellationToken);
    }
}
