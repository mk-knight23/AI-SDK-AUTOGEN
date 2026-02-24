using AutoGenBackend.Models;

namespace AutoGenBackend.Repositories;

/// <summary>
/// In-memory agent repository implementation
/// </summary>
public class InMemoryAgentRepository : InMemoryRepository<Agent>, IAgentRepository
{
    public Task<Agent?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var agent = _storage.Values.FirstOrDefault(a => a.Name == name);
        return Task.FromResult(agent);
    }

    public Task<IEnumerable<Agent>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        var active = _storage.Values.Where(a => a.Status == AgentStatus.Active);
        return Task.FromResult(active);
    }

    public Task<IEnumerable<Agent>> GetByTypeAsync(AgentType type, CancellationToken cancellationToken = default)
    {
        var agents = _storage.Values.Where(a => a.Type == type);
        return Task.FromResult(agents);
    }

    public Task<IEnumerable<Agent>> GetByStatusAsync(AgentStatus status, CancellationToken cancellationToken = default)
    {
        var agents = _storage.Values.Where(a => a.Status == status);
        return Task.FromResult(agents);
    }

    public async Task<Agent> UpdateStatusAsync(Guid id, AgentStatus status, CancellationToken cancellationToken = default)
    {
        var agent = await GetByIdAsync(id, cancellationToken);
        if (agent == null)
            throw new InvalidOperationException($"Agent with ID {id} not found");

        agent.Status = status;
        agent.UpdatedAt = DateTime.UtcNow;
        return await UpdateAsync(agent, cancellationToken);
    }

    public async Task<Agent> ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var agent = await GetByIdAsync(id, cancellationToken);
        if (agent == null)
            throw new InvalidOperationException($"Agent with ID {id} not found");

        agent.Status = AgentStatus.Active;
        agent.UpdatedAt = DateTime.UtcNow;
        agent.LastActivatedAt = DateTime.UtcNow;
        return await UpdateAsync(agent, cancellationToken);
    }

    public async Task<Agent> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await UpdateStatusAsync(id, AgentStatus.Inactive, cancellationToken);
    }
}
