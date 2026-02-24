using AutoGenBackend.Models;

namespace AutoGenBackend.Repositories;

/// <summary>
/// Repository for CodeExecution entities
/// </summary>
public interface ICodeExecutionRepository : IRepository<CodeExecution>
{
    /// <summary>
    /// Get executions by conversation
    /// </summary>
    Task<IEnumerable<CodeExecution>> GetByConversationAsync(Guid conversationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get executions by agent
    /// </summary>
    Task<IEnumerable<CodeExecution>> GetByAgentAsync(Guid agentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get executions by status
    /// </summary>
    Task<IEnumerable<CodeExecution>> GetByStatusAsync(ExecutionStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update execution status
    /// </summary>
    Task<CodeExecution> UpdateStatusAsync(Guid id, ExecutionStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Set execution result
    /// </summary>
    Task<CodeExecution> SetResultAsync(Guid id, ExecutionResult result, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get pending executions
    /// </summary>
    Task<IEnumerable<CodeExecution>> GetPendingAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get recent executions
    /// </summary>
    Task<IEnumerable<CodeExecution>> GetRecentAsync(int count = 50, CancellationToken cancellationToken = default);
}
