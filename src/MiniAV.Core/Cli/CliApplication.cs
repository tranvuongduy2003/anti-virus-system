using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MiniAV.Contracts.Control;
using MiniAV.Core.Coordinator;

namespace MiniAV.Core.Cli;

public static class CliApplication
{
    private const int InvalidArgumentsExitCode = 2;
    private const int NotImplementedExitCode = 3;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public static async Task<int> RunAsync(
        string[] args,
        TextWriter? output = null,
        TextWriter? error = null,
        CancellationToken cancellationToken = default)
    {
        output ??= Console.Out;
        error ??= Console.Error;

        if (args is ["--help"] or ["-h"] or ["help"])
        {
            await WriteUsageAsync(output);
            return 0;
        }

        if (!TryParse(args, out var command, out var validationError))
        {
            await error.WriteLineAsync(validationError);
            await WriteUsageAsync(error);
            return InvalidArgumentsExitCode;
        }

        if (command is UpdateCommand update)
        {
            await error.WriteLineAsync(
                $"Update manager is not implemented yet (scanner: '{update.ScannerId}', manifest: '{update.ManifestPath}').");
            return NotImplementedExitCode;
        }

        var builder = Host.CreateApplicationBuilder();
        builder.Logging.ClearProviders();
        builder.Services.AddOptions<CoordinatorOptions>()
            .BindConfiguration(CoordinatorOptions.SectionName)
            .Validate(options => options.QueueCapacity > 0, "QueueCapacity must be greater than zero.")
            .ValidateOnStart();
        builder.Services.AddSingleton<CoordinatorService>();
        builder.Services.AddSingleton<ICoordinator>(services => services.GetRequiredService<CoordinatorService>());
        builder.Services.AddHostedService(services => services.GetRequiredService<CoordinatorService>());

        using var host = builder.Build();
        await host.StartAsync(cancellationToken);

        try
        {
            var coordinator = host.Services.GetRequiredService<ICoordinator>();
            object result = command switch
            {
                StatusCommand => await coordinator.GetStatusAsync(cancellationToken),
                ScanCommand scan => await coordinator.ScanAsync(new ScanRequest(scan.FilePath), cancellationToken),
                _ => throw new InvalidOperationException($"Unsupported command: {command.GetType().Name}")
            };

            await output.WriteLineAsync(JsonSerializer.Serialize(result, JsonOptions));
            return 0;
        }
        finally
        {
            await host.StopAsync(CancellationToken.None);
        }
    }

    private static bool TryParse(string[] args, out CliCommand command, out string error)
    {
        switch (args)
        {
            case ["status"]:
                command = new StatusCommand();
                error = string.Empty;
                return true;

            case ["scan", var filePath] when !string.IsNullOrWhiteSpace(filePath):
                command = new ScanCommand(filePath);
                error = string.Empty;
                return true;

            case ["update", var scannerId, var manifestPath]
                when !string.IsNullOrWhiteSpace(scannerId) && !string.IsNullOrWhiteSpace(manifestPath):
                command = new UpdateCommand(scannerId, manifestPath);
                error = string.Empty;
                return true;

            default:
                command = new StatusCommand();
                error = args.Length == 0
                    ? "A command is required."
                    : $"Invalid command or arguments: {string.Join(' ', args)}";
                return false;
        }
    }

    private static Task WriteUsageAsync(TextWriter writer) => writer.WriteLineAsync(
        """
        Usage:
          miniav status
          miniav scan <file-path>
          miniav update <scanner-id> <manifest-path>
        """);

    private abstract record CliCommand;

    private sealed record StatusCommand : CliCommand;

    private sealed record ScanCommand(string FilePath) : CliCommand;

    private sealed record UpdateCommand(string ScannerId, string ManifestPath) : CliCommand;
}
