using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MiniAV.Contracts.Control;
using MiniAV.Core.Coordinator;

namespace MiniAV.Tests.Unit;

public sealed class CoordinatorServiceTests
{
    [Fact]
    public async Task Status_exposes_the_three_configured_slots_without_fake_generations()
    {
        var service = CreateService();
        await service.StartAsync(CancellationToken.None);

        try
        {
            var status = await service.GetStatusAsync(CancellationToken.None);

            Assert.Equal("Running", status.State);
            Assert.Collection(
                status.Workers,
                worker => Assert.Equal("stdio-scanner", worker.ScannerId),
                worker => Assert.Equal("tcp-scanner", worker.ScannerId),
                worker => Assert.Equal("grpc-scanner", worker.ScannerId));
            Assert.All(status.Workers, worker => Assert.Null(worker.GenerationId));
            Assert.All(status.Workers, worker => Assert.Null(worker.Lifecycle));
        }
        finally
        {
            await service.StopAsync(CancellationToken.None);
        }
    }

    [Fact]
    public async Task Scan_is_unavailable_until_a_worker_generation_is_active()
    {
        var service = CreateService();
        await service.StartAsync(CancellationToken.None);

        try
        {
            var result = await service.ScanAsync(
                new ScanRequest("metadata-only.txt"),
                CancellationToken.None);

            Assert.Equal(DispatchOutcome.Unavailable, result.Outcome);
            Assert.Empty(result.Workers);
            Assert.NotEqual(Guid.Empty, result.CorrelationId);
        }
        finally
        {
            await service.StopAsync(CancellationToken.None);
        }
    }

    private static CoordinatorService CreateService() => new(
        Options.Create(new CoordinatorOptions()),
        NullLogger<CoordinatorService>.Instance);
}
