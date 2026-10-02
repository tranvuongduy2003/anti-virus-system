using MiniAV.Contracts.Control;

namespace MiniAV.Core.Coordinator;

internal abstract record CoordinatorEvent;

internal sealed record StatusRequested(
    TaskCompletionSource<SystemStatus> Completion) : CoordinatorEvent;

internal sealed record ScanRequested(
    Guid CorrelationId,
    ScanRequest Request,
    TaskCompletionSource<ScanResponse> Completion) : CoordinatorEvent;
