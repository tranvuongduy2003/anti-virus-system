using MiniAV.Contracts.Protocol;

namespace MiniAV.Core.Transports;

public interface IScannerTransport : IAsyncDisposable
{
    Task ConnectAsync(CancellationToken cancellationToken);

    ValueTask SendAsync(ScannerCommand command, CancellationToken cancellationToken);

    IAsyncEnumerable<ScannerResponse> ReadAllAsync(CancellationToken cancellationToken);
}
