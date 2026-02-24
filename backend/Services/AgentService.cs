using AutoGenBackend.Models;
using AutoGenBackend.Repositories;

namespace AutoGenBackend.Services;

/// <summary>
/// Service for managing agents
/// </summary>
public class AgentService
{
    private readonly IAgentRepository _agentRepository;

    public AgentService(IAgentRepository agentRepository)
    {
        _agentRepository = agentRepository;
    }

    /// <summary>
    /// Get all agents
    /// </summary>
    public async Task<IEnumerable<Agent>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _agentRepository.GetAllAsync(cancellationToken);
    }

    /// <summary>
    /// Get agent by ID
    /// </summary>
    public async Task<Agent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _agentRepository.GetByIdAsync(id, cancellationToken);
    }

    /// <summary>
    /// Get agent by name
    /// </summary>
    public async Task<Agent?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _agentRepository.GetByNameAsync(name, cancellationToken);
    }

    /// <summary>
    /// Create new agent
    /// </summary>
    public async Task<Agent> CreateAsync(Agent agent, CancellationToken cancellationToken = default)
    {
        // Validate
        if (string.IsNullOrWhiteSpace(agent.Name))
            throw new ArgumentException("Agent name is required", nameof(agent));

        // Check for duplicate name
        var existing = await _agentRepository.GetByNameAsync(agent.Name, cancellationToken);
        if (existing != null)
            throw new InvalidOperationException($"Agent with name '{agent.Name}' already exists");

        return await _agentRepository.AddAsync(agent, cancellationToken);
    }

    /// <summary>
    /// Update agent
    /// </summary>
    public async Task<Agent> UpdateAsync(Guid id, Agent agent, CancellationToken cancellationToken = default)
    {
        var existing = await _agentRepository.GetByIdAsync(id, cancellationToken);
        if (existing == null)
            throw new InvalidOperationException($"Agent with ID {id} not found");

        // Preserve immutable fields
        agent.Id = id;
        agent.CreatedAt = existing.CreatedAt;
        agent.UpdatedAt = DateTime.UtcNow;

        // Check name uniqueness if changed
        if (agent.Name != existing.Name)
        {
            var nameConflict = await _agentRepository.GetByNameAsync(agent.Name, cancellationToken);
            if (nameConflict != null && nameConflict.Id != id)
                throw new InvalidOperationException($"Agent with name '{agent.Name}' already exists");
        }

        return await _agentRepository.UpdateAsync(agent, cancellationToken);
    }

    /// <summary>
    /// Delete agent
    /// </summary>
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var exists = await _agentRepository.ExistsAsync(id, cancellationToken);
        if (!exists)
            throw new InvalidOperationException($"Agent with ID {id} not found");

        await _agentRepository.DeleteAsync(id, cancellationToken);
    }

    /// <summary>
    /// Activate agent
    /// </summary>
    public async Task<Agent> ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _agentRepository.ActivateAsync(id, cancellationToken);
    }

    /// <summary>
    /// Deactivate agent
    /// </summary>
    public async Task<Agent> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _agentRepository.DeactivateAsync(id, cancellationToken);
    }

    /// <summary>
    /// Get active agents
    /// </summary>
    public async Task<IEnumerable<Agent>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _agentRepository.GetActiveAsync(cancellationToken);
    }

    /// <summary>
    /// Get agents by type
    /// </summary>
    public async Task<IEnumerable<Agent>> GetByTypeAsync(AgentType type, CancellationToken cancellationToken = default)
    {
        return await _agentRepository.GetByTypeAsync(type, cancellationToken);
    }
}
