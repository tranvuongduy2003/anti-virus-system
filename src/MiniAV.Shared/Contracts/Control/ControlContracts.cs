namespace MiniAV.Contracts.Control;

public enum WorkerLifecycle
{
    Starting,
    Healthy,
    Active,
    Draining,
    Retired,
    Failed
}

public enum DispatchOutcome
{
    Completed,
    Partial,
    Unavailable
}

public sealed record WorkerStatus(
    string ScannerId,
    string Transport,
    long? GenerationId,
    string? WorkerVersion,
    int? ProcessId,
    WorkerLifecycle? Lifecycle);

public sealed record SystemStatus(
    string State,
    IReadOnlyList<WorkerStatus> Workers);

public sealed record ScanRequest(string FilePath);

public sealed record WorkerDispatchResult(
    string ScannerId,
    long GenerationId,
    bool Acknowledged,
    string? ErrorCode = null);

public sealed record ScanResponse(
    Guid CorrelationId,
    DispatchOutcome Outcome,
    IReadOnlyList<WorkerDispatchResult> Workers);

