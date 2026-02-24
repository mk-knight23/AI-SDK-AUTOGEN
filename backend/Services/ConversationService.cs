using AutoGenBackend.Models;
using AutoGenBackend.Repositories;

namespace AutoGenBackend.Services;

/// <summary>
/// Service for managing conversations
/// </summary>
public class ConversationService
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IAgentRepository _agentRepository;

    public ConversationService(
        IConversationRepository conversationRepository,
        IAgentRepository agentRepository)
    {
        _conversationRepository = conversationRepository;
        _agentRepository = agentRepository;
    }

    /// <summary>
    /// Get all conversations
    /// </summary>
    public async Task<IEnumerable<Conversation>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _conversationRepository.GetAllAsync(cancellationToken);
    }

    /// <summary>
    /// Get conversation by ID
    /// </summary>
    public async Task<Conversation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _conversationRepository.GetByIdAsync(id, cancellationToken);
    }

    /// <summary>
    /// Create new conversation
    /// </summary>
    public async Task<Conversation> CreateAsync(Conversation conversation, CancellationToken cancellationToken = default)
    {
        // Validate participants exist
        foreach (var participantId in conversation.ParticipantIds)
        {
            var agent = await _agentRepository.GetByIdAsync(participantId, cancellationToken);
            if (agent == null)
                throw new InvalidOperationException($"Agent with ID {participantId} not found");
        }

        if (conversation.ParticipantIds.Count < 2 && conversation.Type == ConversationType.OneToOne)
            throw new ArgumentException("One-to-one conversations require at least 2 participants");

        return await _conversationRepository.AddAsync(conversation, cancellationToken);
    }

    /// <summary>
    /// Update conversation
    /// </summary>
    public async Task<Conversation> UpdateAsync(Guid id, Conversation conversation, CancellationToken cancellationToken = default)
    {
        var existing = await _conversationRepository.GetByIdAsync(id, cancellationToken);
        if (existing == null)
            throw new InvalidOperationException($"Conversation with ID {id} not found");

        conversation.Id = id;
        conversation.CreatedAt = existing.CreatedAt;
        conversation.UpdatedAt = DateTime.UtcNow;

        return await _conversationRepository.UpdateAsync(conversation, cancellationToken);
    }

    /// <summary>
    /// Delete conversation
    /// </summary>
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var exists = await _conversationRepository.ExistsAsync(id, cancellationToken);
        if (!exists)
            throw new InvalidOperationException($"Conversation with ID {id} not found");

        await _conversationRepository.DeleteAsync(id, cancellationToken);
    }

    /// <summary>
    /// Get messages for conversation
    /// </summary>
    public async Task<IEnumerable<Message>> GetMessagesAsync(
        Guid conversationId,
        int skip = 0,
        int take = 100,
        CancellationToken cancellationToken = default)
    {
        return await _conversationRepository.GetMessagesAsync(conversationId, skip, take, cancellationToken);
    }

    /// <summary>
    /// Send message in conversation
    /// </summary>
    public async Task<Message> SendMessageAsync(
        Guid conversationId,
        Guid sourceAgentId,
        string content,
        Guid? targetAgentId = null,
        CancellationToken cancellationToken = default)
    {
        // Validate conversation exists
        var conversation = await _conversationRepository.GetByIdAsync(conversationId, cancellationToken);
        if (conversation == null)
            throw new InvalidOperationException($"Conversation with ID {conversationId} not found");

        // Validate source agent is participant
        if (!conversation.ParticipantIds.Contains(sourceAgentId))
            throw new InvalidOperationException("Source agent is not a participant in this conversation");

        // Validate target agent if specified
        if (targetAgentId.HasValue && !conversation.ParticipantIds.Contains(targetAgentId.Value))
            throw new InvalidOperationException("Target agent is not a participant in this conversation");

        var message = new Message
        {
            ConversationId = conversationId,
            SourceAgentId = sourceAgentId,
            TargetAgentId = targetAgentId,
            Content = content,
            Type = MessageType.Text,
            Timestamp = DateTime.UtcNow
        };

        conversation.UpdatedAt = DateTime.UtcNow;
        await _conversationRepository.UpdateAsync(conversation, cancellationToken);

        return await _conversationRepository.AddMessageAsync(message, cancellationToken);
    }

    /// <summary>
    /// Get conversations by participant
    /// </summary>
    public async Task<IEnumerable<Conversation>> GetByParticipantAsync(Guid agentId, CancellationToken cancellationToken = default)
    {
        return await _conversationRepository.GetByParticipantAsync(agentId, cancellationToken);
    }
}
