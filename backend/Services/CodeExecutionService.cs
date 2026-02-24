using AutoGenBackend.Models;
using AutoGenBackend.Repositories;

namespace AutoGenBackend.Services;

/// <summary>
/// Service for managing code execution
/// </summary>
public class CodeExecutionService
{
    private readonly ICodeExecutionRepository _executionRepository;
    private readonly IDockerSandbox _dockerSandbox;

    public CodeExecutionService(
        ICodeExecutionRepository executionRepository,
        IDockerSandbox dockerSandbox)
    {
        _executionRepository = executionRepository;
        _dockerSandbox = dockerSandbox;
    }

    /// <summary>
    /// Get all executions
    /// </summary>
    public async Task<IEnumerable<CodeExecution>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _executionRepository.GetAllAsync(cancellationToken);
    }

    /// <summary>
    /// Get execution by ID
    /// </summary>
    public async Task<CodeExecution?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _executionRepository.GetByIdAsync(id, cancellationToken);
    }

    /// <summary>
    /// Execute code
    /// </summary>
    public async Task<CodeExecution> ExecuteAsync(
        CodeLanguage language,
        string code,
        Guid? conversationId = null,
        Guid? requestingAgentId = null,
        int timeoutSeconds = 30,
        ResourceLimits? limits = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code is required", nameof(code));

        var execution = new CodeExecution
        {
            Id = Guid.NewGuid(),
            Language = language,
            Code = code,
            ConversationId = conversationId,
            RequestingAgentId = requestingAgentId,
            TimeoutSeconds = timeoutSeconds,
            ResourceLimits = limits ?? new ResourceLimits(),
            Status = ExecutionStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        execution = await _executionRepository.AddAsync(execution, cancellationToken);

        // Execute in background (fire and forget for this API)
        _ = Task.Run(() => ExecuteInternalAsync(execution.Id, cancellationToken), cancellationToken);

        return execution;
    }

    /// <summary>
    /// Internal execution method
    /// </summary>
    private async Task ExecuteInternalAsync(Guid executionId, CancellationToken cancellationToken)
    {
        var execution = await _executionRepository.GetByIdAsync(executionId, cancellationToken);
        if (execution == null)
            return;

        try
        {
            await _executionRepository.UpdateStatusAsync(executionId, ExecutionStatus.Running, cancellationToken);

            var result = await _dockerSandbox.ExecuteAsync(
                execution.Language,
                execution.Code,
                execution.TimeoutSeconds,
                execution.ResourceLimits,
                cancellationToken);

            await _executionRepository.SetResultAsync(executionId, result, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            await _executionRepository.UpdateStatusAsync(executionId, ExecutionStatus.Timeout, cancellationToken);
        }
        catch (Exception ex)
        {
            var errorResult = new ExecutionResult
            {
                ExitCode = -1,
                Stderr = ex.Message,
                Duration = TimeSpan.Zero,
                Error = ex.ToString()
            };
            await _executionRepository.SetResultAsync(executionId, errorResult, cancellationToken);
        }
    }

    /// <summary>
    /// Get recent executions
    /// </summary>
    public async Task<IEnumerable<CodeExecution>> GetRecentAsync(int count = 50, CancellationToken cancellationToken = default)
    {
        return await _executionRepository.GetRecentAsync(count, cancellationToken);
    }

    /// <summary>
    /// Get executions by conversation
    /// </summary>
    public async Task<IEnumerable<CodeExecution>> GetByConversationAsync(Guid conversationId, CancellationToken cancellationToken = default)
    {
        return await _executionRepository.GetByConversationAsync(conversationId, cancellationToken);
    }
}

/// <summary>
/// Docker sandbox interface for code execution
/// </summary>
public interface IDockerSandbox
{
    Task<ExecutionResult> ExecuteAsync(
        CodeLanguage language,
        string code,
        int timeoutSeconds,
        ResourceLimits limits,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// In-memory mock Docker sandbox for development
/// </summary>
public class InMemoryDockerSandbox : IDockerSandbox
{
    public async Task<ExecutionResult> ExecuteAsync(
        CodeLanguage language,
        string code,
        int timeoutSeconds,
        ResourceLimits limits,
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(100, cancellationToken);

        // Simple mock execution based on language
        return language switch
        {
            CodeLanguage.Python => new ExecutionResult
            {
                Stdout = MockPythonExecution(code),
                Stderr = string.Empty,
                ExitCode = 0,
                Duration = TimeSpan.FromMilliseconds(100),
                ResourceUsage = new ResourceUsage { MemoryUsedMB = 50, CpuTimeSeconds = 0.1 }
            },
            CodeLanguage.JavaScript => new ExecutionResult
            {
                Stdout = MockJavaScriptExecution(code),
                Stderr = string.Empty,
                ExitCode = 0,
                Duration = TimeSpan.FromMilliseconds(100),
                ResourceUsage = new ResourceUsage { MemoryUsedMB = 50, CpuTimeSeconds = 0.1 }
            },
            _ => new ExecutionResult
            {
                Stdout = $"Mock execution of {language} code",
                Stderr = string.Empty,
                ExitCode = 0,
                Duration = TimeSpan.FromMilliseconds(100),
                ResourceUsage = new ResourceUsage { MemoryUsedMB = 50, CpuTimeSeconds = 0.1 }
            }
        };
    }

    private string MockPythonExecution(string code)
    {
        if (code.Contains("print("))
        {
            var match = System.Text.RegularExpressions.Regex.Match(code, @"print\(([^)]+)\)");
            if (match.Success)
            {
                var value = match.Groups[1].Value.Trim('\'', '"');
                return value;
            }
        }
        return "Python execution completed";
    }

    private string MockJavaScriptExecution(string code)
    {
        if (code.Contains("console.log("))
        {
            var match = System.Text.RegularExpressions.Regex.Match(code, @"console\.log\(([^)]+)\)");
            if (match.Success)
            {
                var value = match.Groups[1].Value.Trim('\'', '"');
                return value;
            }
        }
        return "JavaScript execution completed";
    }
}
