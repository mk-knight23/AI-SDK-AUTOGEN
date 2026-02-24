using AutoGenBackend.Models;
using AutoGenBackend.Repositories;

namespace AutoGenBackend.Services;

/// <summary>
/// Service for managing group chats
/// </summary>
public class GroupChatService
{
    private readonly IGroupChatRepository _groupChatRepository;
    private readonly IAgentRepository _agentRepository;

    public GroupChatService(
        IGroupChatRepository groupChatRepository,
        IAgentRepository agentRepository)
    {
        _groupChatRepository = groupChatRepository;
        _agentRepository = agentRepository;
    }

    /// <summary>
    /// Get all group chats
    /// </summary>
    public async Task<IEnumerable<GroupChat>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _groupChatRepository.GetAllAsync(cancellationToken);
    }

    /// <summary>
    /// Get group chat by ID
    /// </summary>
    public async Task<GroupChat?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _groupChatRepository.GetByIdAsync(id, cancellationToken);
    }

    /// <summary>
    /// Create new group chat
    /// </summary>
    public async Task<GroupChat> CreateAsync(GroupChat groupChat, CancellationToken cancellationToken = default)
    {
        // Validate participants exist
        foreach (var participantId in groupChat.ParticipantIds)
        {
            var agent = await _agentRepository.GetByIdAsync(participantId, cancellationToken);
            if (agent == null)
                throw new InvalidOperationException($"Agent with ID {participantId} not found");
        }

        if (groupChat.ParticipantIds.Count < 2)
            throw new ArgumentException("Group chats require at least 2 participants");

        return await _groupChatRepository.AddAsync(groupChat, cancellationToken);
    }

    /// <summary>
    /// Update group chat
    /// </summary>
    public async Task<GroupChat> UpdateAsync(Guid id, GroupChat groupChat, CancellationToken cancellationToken = default)
    {
        var existing = await _groupChatRepository.GetByIdAsync(id, cancellationToken);
        if (existing == null)
            throw new InvalidOperationException($"Group chat with ID {id} not found");

        groupChat.Id = id;
        groupChat.CreatedAt = existing.CreatedAt;
        groupChat.UpdatedAt = DateTime.UtcNow;

        return await _groupChatRepository.UpdateAsync(groupChat, cancellationToken);
    }

    /// <summary>
    /// Delete group chat
    /// </summary>
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var exists = await _groupChatRepository.ExistsAsync(id, cancellationToken);
        if (!exists)
            throw new InvalidOperationException($"Group chat with ID {id} not found");

        await _groupChatRepository.DeleteAsync(id, cancellationToken);
    }

    /// <summary>
    /// Get messages for group chat
    /// </summary>
    public async Task<IEnumerable<GroupChatMessage>> GetMessagesAsync(
        Guid groupChatId,
        int skip = 0,
        int take = 100,
        CancellationToken cancellationToken = default)
    {
        return await _groupChatRepository.GetMessagesAsync(groupChatId, skip, take, cancellationToken);
    }

    /// <summary>
    /// Send message to group chat with routing based on pattern
    /// </summary>
    public async Task<GroupChatMessage> SendMessageAsync(
        Guid groupChatId,
        Guid sourceAgentId,
        string content,
        CancellationToken cancellationToken = default)
    {
        var groupChat = await _groupChatRepository.GetByIdAsync(groupChatId, cancellationToken);
        if (groupChat == null)
            throw new InvalidOperationException($"Group chat with ID {groupChatId} not found");

        // Validate source agent is participant
        if (!groupChat.ParticipantIds.Contains(sourceAgentId))
            throw new InvalidOperationException("Source agent is not a participant in this group chat");

        // Determine target agents based on pattern
        var targetAgentIds = DetermineTargets(groupChat, sourceAgentId);
        if (targetAgentIds.Count == 0)
            throw new InvalidOperationException("No target agents determined for this message");

        var message = new GroupChatMessage
        {
            Id = Guid.NewGuid(),
            GroupChatId = groupChatId,
            SourceAgentId = sourceAgentId,
            TargetAgentIds = targetAgentIds,
            Content = content,
            Timestamp = DateTime.UtcNow
        };

        groupChat.UpdatedAt = DateTime.UtcNow;
        await _groupChatRepository.UpdateAsync(groupChat, cancellationToken);

        return await _groupChatRepository.AddMessageAsync(message, cancellationToken);
    }

    /// <summary>
    /// Determine target agents based on group chat pattern
    /// </summary>
    private List<Guid> DetermineTargets(GroupChat groupChat, Guid sourceAgentId)
    {
        return groupChat.Pattern switch
        {
            GroupChatPattern.RoundRobin => GetNextRoundRobinTarget(groupChat, sourceAgentId),
            GroupChatPattern.Broadcast => groupChat.ParticipantIds.Where(id => id != sourceAgentId).ToList(),
            GroupChatPattern.SpeakerSelection => GetSpeakerSelectionTargets(groupChat, sourceAgentId),
            GroupChatPattern.FreeForAll => groupChat.ParticipantIds.Where(id => id != sourceAgentId).ToList(),
            _ => groupChat.ParticipantIds.Where(id => id != sourceAgentId).ToList()
        };
    }

    private List<Guid> GetNextRoundRobinTarget(GroupChat groupChat, Guid sourceAgentId)
    {
        // Simple round-robin: find next agent after current in list
        var currentIndex = groupChat.ParticipantIds.IndexOf(sourceAgentId);
        if (currentIndex < 0)
            return new List<Guid>();

        var nextIndex = (currentIndex + 1) % groupChat.ParticipantIds.Count;
        return new List<Guid> { groupChat.ParticipantIds[nextIndex] };
    }

    private List<Guid> GetSpeakerSelectionTargets(GroupChat groupChat, Guid sourceAgentId)
    {
        // For speaker selection, the content would specify who to speak to
        // For now, default to round-robin behavior
        return GetNextRoundRobinTarget(groupChat, sourceAgentId);
    }

    /// <summary>
    /// Add participant to group chat
    /// </summary>
    public async Task<GroupChat> AddParticipantAsync(Guid groupChatId, Guid agentId, CancellationToken cancellationToken = default)
    {
        return await _groupChatRepository.AddParticipantAsync(groupChatId, agentId, cancellationToken);
    }

    /// <summary>
    /// Remove participant from group chat
    /// </summary>
    public async Task<GroupChat> RemoveParticipantAsync(Guid groupChatId, Guid agentId, CancellationToken cancellationToken = default)
    {
        return await _groupChatRepository.RemoveParticipantAsync(groupChatId, agentId, cancellationToken);
    }

    /// <summary>
    /// Get group chats by participant
    /// </summary>
    public async Task<IEnumerable<GroupChat>> GetByParticipantAsync(Guid agentId, CancellationToken cancellationToken = default)
    {
        return await _groupChatRepository.GetByParticipantAsync(agentId, cancellationToken);
    }
}
