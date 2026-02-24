using AutoGenBackend.Models;

namespace AutoGenBackend.Repositories;

/// <summary>
/// Repository for Conversation entities
/// </summary>
public interface IConversationRepository : IRepository<Conversation>
{
    /// <summary>
    /// Get conversations by participant
    /// </summary>
    Task<IEnumerable<Conversation>> GetByParticipantAsync(Guid agentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get messages for a conversation
    /// </summary>
    Task<IEnumerable<Message>> GetMessagesAsync(Guid conversationId, int skip = 0, int take = 100, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add message to conversation
    /// </summary>
    Task<Message> AddMessageAsync(Message message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get conversations by type
    /// </summary>
    Task<IEnumerable<Conversation>> GetByTypeAsync(ConversationType type, CancellationToken cancellationToken = default);
}
