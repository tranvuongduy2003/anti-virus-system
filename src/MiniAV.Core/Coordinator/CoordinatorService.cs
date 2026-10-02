using System.Threading.Channels;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MiniAV.Contracts.Control;

namespace MiniAV.Core.Coordinator;

public sealed class CoordinatorService : BackgroundService, ICoordinator
{
    private static readonly ScannerSlot[] Topology =
    [
        new("stdio-scanner", "stdio"),
        new("tcp-scanner", "tcp"),
        new("grpc-scanner", "grpc")
    ];

    private readonly Channel<CoordinatorEvent> _events;
    private readonly ILogger<CoordinatorService> _logger;
    private volatile bool _isRunning;

    public CoordinatorService(
        IOptions<CoordinatorOptions> options,
        ILogger<CoordinatorService> logger)
    {
        _logger = logger;
        _events = Channel.CreateBounded<CoordinatorEvent>(new BoundedChannelOptions(options.Value.QueueCapacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        });
    }

    public bool IsRunning => _isRunning;

    public async Task<SystemStatus> GetStatusAsync(CancellationToken cancellationToken)
    {
        var completion = NewCompletion<SystemStatus>();
        await _events.Writer.WriteAsync(new StatusRequested(completion), cancellationToken);
        return await completion.Task.WaitAsync(cancellationToken);
    }

    public async Task<ScanResponse> ScanAsync(ScanRequest request, CancellationToken cancellationToken)
    {
        var completion = NewCompletion<ScanResponse>();
        var correlationId = Guid.NewGuid();
        await _events.Writer.WriteAsync(
            new ScanRequested(correlationId, request, completion),
            cancellationToken);
        return await completion.Task.WaitAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _isRunning = true;
        _logger.LogInformation("MiniAV coordinator started with {SlotCount} configured scanner slots", Topology.Length);

        try
        {
            await foreach (var coordinatorEvent in _events.Reader.ReadAllAsync(stoppingToken))
            {
                Handle(coordinatorEvent);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Normal host shutdown.
        }
        finally
        {
            _isRunning = false;
            _events.Writer.TryComplete();
        }
    }

    private static void Handle(CoordinatorEvent coordinatorEvent)
    {
        switch (coordinatorEvent)
        {
            case StatusRequested requested:
                requested.Completion.TrySetResult(CreateStatus());
                break;

            case ScanRequested requested:
                // No generation is routable until the process manager and transports are attached.
                requested.Completion.TrySetResult(new ScanResponse(
                    requested.CorrelationId,
                    DispatchOutcome.Unavailable,
                    []));
                break;

            default:
                throw new InvalidOperationException($"Unknown coordinator event: {coordinatorEvent.GetType().Name}");
        }
    }

    private static SystemStatus CreateStatus() => new(
        "Running",
        Topology.Select(slot => new WorkerStatus(
            slot.ScannerId,
            slot.Transport,
            GenerationId: null,
            WorkerVersion: null,
            ProcessId: null,
            Lifecycle: null)).ToArray());

    private static TaskCompletionSource<T> NewCompletion<T>() =>
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    private sealed record ScannerSlot(string ScannerId, string Transport);
}
