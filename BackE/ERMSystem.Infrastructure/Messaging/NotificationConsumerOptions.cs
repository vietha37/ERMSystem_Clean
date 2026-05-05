namespace ERMSystem.Infrastructure.Messaging;

public class NotificationConsumerOptions
{
    public int ReconnectDelaySeconds { get; set; } = 30;
    public ushort PrefetchCount { get; set; } = 10;
}
