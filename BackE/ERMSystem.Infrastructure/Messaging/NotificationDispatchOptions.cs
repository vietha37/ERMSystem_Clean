namespace ERMSystem.Infrastructure.Messaging;

public class NotificationDispatchOptions
{
    public int PollIntervalSeconds { get; set; } = 5;
    public int BatchSize { get; set; } = 20;
}
