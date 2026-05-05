namespace ERMSystem.Infrastructure.Messaging;

public class OutboxPublisherOptions
{
    public int PollIntervalSeconds { get; set; } = 5;
    public int RetryDelaySeconds { get; set; } = 30;
    public int BatchSize { get; set; } = 20;
}
