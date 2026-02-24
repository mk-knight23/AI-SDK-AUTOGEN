using AutoGenBackend.Models;

namespace AutoGenBackend.Repositories;

/// <summary>
/// Repository for GroupChat entities
/// </summary>
public interface IGroupChatRepository : IRepository<GroupChat>
{
    /// <summary>
    /// Get group chats where agent is a participant
    /// </summary>
    Task<IEnumerable<GroupChat>> GetByParticipantAsync(Guid agentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get messages for a group chat
    /// </summary>
    Task<IEnumerable<GroupChatMessage>> GetMessagesAsync(Guid groupChatId, int skip = 0, int take = 100, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add message to group chat
    /// </summary>
    Task<GroupChatMessage> AddMessageAsync(GroupChatMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get group chats by pattern
    /// </summary>
    Task<IEnumerable<GroupChat>> GetByPatternAsync(GroupChatPattern pattern, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add participant to group chat
    /// </summary>
    Task<GroupChat> AddParticipantAsync(Guid groupChatId, Guid agentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove participant from group chat
    /// </summary>
    Task<GroupChat> RemoveParticipantAsync(Guid groupChatId, Guid agentId, CancellationToken cancellationToken = default);
}
