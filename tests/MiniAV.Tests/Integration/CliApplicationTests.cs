using System.Text.Json;
using MiniAV.Contracts.Control;
using MiniAV.Core.Cli;

namespace MiniAV.Tests.Integration;

public sealed class CliApplicationTests
{
    [Fact]
    public async Task Status_writes_the_configured_topology_as_json()
    {
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = await CliApplication.RunAsync(["status"], output, error);
        var status = JsonSerializer.Deserialize<SystemStatus>(
            output.ToString(),
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.Equal(0, exitCode);
        Assert.Equal(string.Empty, error.ToString());
        Assert.NotNull(status);
        Assert.Equal(3, status.Workers.Count);
    }

    [Fact]
    public async Task Scan_rejects_an_empty_file_path()
    {
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = await CliApplication.RunAsync(["scan", " "], output, error);

        Assert.Equal(2, exitCode);
        Assert.Equal(string.Empty, output.ToString());
        Assert.Contains("Invalid command or arguments", error.ToString());
    }
}
