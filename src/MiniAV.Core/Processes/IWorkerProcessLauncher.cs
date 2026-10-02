namespace MiniAV.Core.Processes;

public interface IWorkerProcessLauncher
{
    Task<WorkerProcessHandle> StartAsync(
        WorkerStartRequest request,
        CancellationToken cancellationToken);
}

public sealed record WorkerStartRequest(
    string ScannerId,
    string WorkerVersion,
    string Transport,
    string ExecutablePath,
    IReadOnlyList<string> Arguments);

public sealed record WorkerProcessHandle(
    int ProcessId,
    DateTimeOffset StartedAt,
    Task<int> ExitCode);
