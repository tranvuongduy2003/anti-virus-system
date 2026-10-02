using MiniAV.Contracts.Control;

namespace MiniAV.Core.Coordinator;

public interface ICoordinator
{
    bool IsRunning { get; }

    Task<SystemStatus> GetStatusAsync(CancellationToken cancellationToken);

    Task<ScanResponse> ScanAsync(ScanRequest request, CancellationToken cancellationToken);
}
