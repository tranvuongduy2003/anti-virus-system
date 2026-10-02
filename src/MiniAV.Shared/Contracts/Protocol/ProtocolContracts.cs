namespace MiniAV.Contracts.Protocol;

public static class ProtocolVersions
{
    public const string Current = "1.0";
}

public enum ScannerMessageType
{
    Hello,
    HelloAck,
    HealthCheck,
    HealthStatus,
    Scan,
    Accepted,
    Shutdown,
    ShutdownAck,
    Error
}

public sealed record ScannerCommand(
    string ProtocolVersion,
    Guid CorrelationId,
    string ScannerId,
    string WorkerVersion,
    ScannerMessageType Type,
    DateTimeOffset Timestamp,
    IReadOnlyDictionary<string, string>? Metadata = null);

public sealed record ScannerResponse(
    string ProtocolVersion,
    Guid CorrelationId,
    string ScannerId,
    string WorkerVersion,
    ScannerMessageType Type,
    DateTimeOffset Timestamp,
    bool Success,
    string? ErrorCode = null,
    IReadOnlyDictionary<string, string>? Metadata = null);
