using System.Text.Json;

var options = ScannerOptions.Parse(args);
if (options is null)
{
    Console.Error.WriteLine(
        "Usage: MiniAV.Scanner --scanner-id <id> --worker-version <version> --transport <stdio|tcp|grpc>");
    return 2;
}

var startupLog = new
{
    eventName = "scanner.configured",
    scannerId = options.ScannerId,
    workerVersion = options.WorkerVersion,
    transport = options.Transport,
    processId = Environment.ProcessId,
    note = "Transport runtimes are added in the next implementation slice."
};

Console.Error.WriteLine(JsonSerializer.Serialize(startupLog));
return 0;

internal sealed record ScannerOptions(string ScannerId, string WorkerVersion, string Transport)
{
    private static readonly HashSet<string> SupportedTransports =
        new(StringComparer.OrdinalIgnoreCase) { "stdio", "tcp", "grpc" };

    public static ScannerOptions? Parse(string[] args)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        for (var index = 0; index < args.Length - 1; index += 2)
        {
            if (!args[index].StartsWith("--", StringComparison.Ordinal))
            {
                return null;
            }

            values[args[index][2..]] = args[index + 1];
        }

        if (args.Length % 2 != 0
            || !values.TryGetValue("scanner-id", out var scannerId)
            || !values.TryGetValue("worker-version", out var workerVersion)
            || !values.TryGetValue("transport", out var transport)
            || string.IsNullOrWhiteSpace(scannerId)
            || string.IsNullOrWhiteSpace(workerVersion)
            || !SupportedTransports.Contains(transport))
        {
            return null;
        }

        return new ScannerOptions(scannerId, workerVersion, transport.ToLowerInvariant());
    }
}
