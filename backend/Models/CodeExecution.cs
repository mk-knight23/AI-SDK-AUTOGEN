namespace AutoGenBackend.Models;

/// <summary>
/// Represents a code execution request and result
/// </summary>
public class CodeExecution
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? ConversationId { get; set; }
    public Guid? RequestingAgentId { get; set; }
    public CodeLanguage Language { get; set; }
    public string Code { get; set; } = string.Empty;
    public ExecutionStatus Status { get; set; } = ExecutionStatus.Pending;
    public ExecutionResult? Result { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int TimeoutSeconds { get; set; } = 30;
    public ResourceLimits ResourceLimits { get; set; } = new();
}

/// <summary>
/// Supported programming languages for code execution
/// </summary>
public enum CodeLanguage
{
    Python,
    JavaScript,
    TypeScript,
    CSharp,
    Bash,
    PowerShell
}

/// <summary>
/// Execution status
/// </summary>
public enum ExecutionStatus
{
    Pending,
    Running,
    Completed,
    Failed,
    Timeout,
    Cancelled
}

/// <summary>
/// Result of code execution
/// </summary>
public class ExecutionResult
{
    public string Stdout { get; set; } = string.Empty;
    public string Stderr { get; set; } = string.Empty;
    public int ExitCode { get; set; }
    public TimeSpan Duration { get; set; }
    public ResourceUsage ResourceUsage { get; set; } = new();
    public string? Error { get; set; }
}

/// <summary>
/// Resource usage statistics
/// </summary>
public class ResourceUsage
{
    public long MemoryUsedMB { get; set; }
    public double CpuTimeSeconds { get; set; }
    public int ProcessId { get; set; }
}

/// <summary>
/// Resource limits for code execution
/// </summary>
public class ResourceLimits
{
    public int MaxMemoryMB { get; set; } = 512;
    public double MaxCpuSeconds { get; set; } = 10.0;
    public int MaxOutputLength { get; set; } = 100000;
}
