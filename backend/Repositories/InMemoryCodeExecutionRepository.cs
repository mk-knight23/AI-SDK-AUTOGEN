using AutoGenBackend.Models;

namespace AutoGenBackend.Repositories;

/// <summary>
/// In-memory code execution repository
/// </summary>
public class InMemoryCodeExecutionRepository : InMemoryRepository<CodeExecution>, ICodeExecutionRepository
{
    public Task<IEnumerable<CodeExecution>> GetByConversationAsync(Guid conversationId, CancellationToken cancellationToken = default)
    {
        var executions = _storage.Values
            .Where(e => e.ConversationId == conversationId);
        return Task.FromResult(executions);
    }

    public Task<IEnumerable<CodeExecution>> GetByAgentAsync(Guid agentId, CancellationToken cancellationToken = default)
    {
        var executions = _storage.Values
            .Where(e => e.RequestingAgentId == agentId);
        return Task.FromResult(executions);
    }

    public Task<IEnumerable<CodeExecution>> GetByStatusAsync(ExecutionStatus status, CancellationToken cancellationToken = default)
    {
        var executions = _storage.Values.Where(e => e.Status == status);
        return Task.FromResult(executions);
    }

    public async Task<CodeExecution> UpdateStatusAsync(Guid id, ExecutionStatus status, CancellationToken cancellationToken = default)
    {
        var execution = await GetByIdAsync(id, cancellationToken);
        if (execution == null)
            throw new InvalidOperationException($"Code execution with ID {id} not found");

        execution.Status = status;
        if (status == ExecutionStatus.Running)
            execution.StartedAt = DateTime.UtcNow;
        if (status == ExecutionStatus.Completed || status == ExecutionStatus.Failed || status == ExecutionStatus.Timeout)
            execution.CompletedAt = DateTime.UtcNow;

        return await UpdateAsync(execution, cancellationToken);
    }

    public async Task<CodeExecution> SetResultAsync(Guid id, ExecutionResult result, CancellationToken cancellationToken = default)
    {
        var execution = await GetByIdAsync(id, cancellationToken);
        if (execution == null)
            throw new InvalidOperationException($"Code execution with ID {id} not found");

        execution.Result = result;
        execution.CompletedAt = DateTime.UtcNow;
        execution.Status = result.ExitCode == 0 ? ExecutionStatus.Completed : ExecutionStatus.Failed;
        return await UpdateAsync(execution, cancellationToken);
    }

    public Task<IEnumerable<CodeExecution>> GetPendingAsync(CancellationToken cancellationToken = default)
    {
        return GetByStatusAsync(ExecutionStatus.Pending, cancellationToken);
    }

    public Task<IEnumerable<CodeExecution>> GetRecentAsync(int count = 50, CancellationToken cancellationToken = default)
    {
        var recent = _storage.Values
            .OrderByDescending(e => e.CreatedAt)
            .Take(count);
        return Task.FromResult(recent);
    }
}
