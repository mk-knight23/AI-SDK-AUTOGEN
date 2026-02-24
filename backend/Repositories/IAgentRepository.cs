using AutoGenBackend.Models;

namespace AutoGenBackend.Repositories;

/// <summary>
/// Repository for Agent entities
/// </summary>
public interface IAgentRepository : IRepository<Agent>
{
    /// <summary>
    /// Get agent by name
    /// </summary>
    Task<Agent?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all active agents
    /// </summary>
    Task<IEnumerable<Agent>> GetActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get agents by type
    /// </summary>
    Task<IEnumerable<Agent>> GetByTypeAsync(AgentType type, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get agents by status
    /// </summary>
    Task<IEnumerable<Agent>> GetByStatusAsync(AgentStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update agent status
    /// </summary>
    Task<Agent> UpdateStatusAsync(Guid id, AgentStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Activate agent
    /// </summary>
    Task<Agent> ActivateAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deactivate agent
    /// </summary>
    Task<Agent> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
}
