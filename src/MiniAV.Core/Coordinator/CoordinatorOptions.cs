namespace MiniAV.Core.Coordinator;

public sealed class CoordinatorOptions
{
    public const string SectionName = "Coordinator";

    public int QueueCapacity { get; init; } = 256;

    public TimeSpan CommandTimeout { get; init; } = TimeSpan.FromSeconds(5);
}
