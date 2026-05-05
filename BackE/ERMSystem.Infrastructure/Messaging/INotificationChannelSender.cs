namespace ERMSystem.Infrastructure.Messaging;

public interface INotificationChannelSender
{
    string ChannelCode { get; }
    Task<NotificationSendResult> SendAsync(NotificationSendRequest request, CancellationToken ct = default);
}

public sealed class NotificationSendRequest
{
    public Guid DeliveryId { get; init; }
    public Guid OutboxMessageId { get; init; }
    public string Recipient { get; init; } = string.Empty;
    public string ChannelCode { get; init; } = string.Empty;
}

public sealed class NotificationSendResult
{
    public bool Success { get; init; }
    public string? ProviderMessageId { get; init; }
    public string? ErrorMessage { get; init; }
}
