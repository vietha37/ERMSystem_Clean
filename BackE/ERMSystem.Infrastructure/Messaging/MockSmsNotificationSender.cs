using Microsoft.Extensions.Logging;

namespace ERMSystem.Infrastructure.Messaging;

public class MockSmsNotificationSender : INotificationChannelSender
{
    private readonly ILogger<MockSmsNotificationSender> _logger;

    public MockSmsNotificationSender(ILogger<MockSmsNotificationSender> logger)
    {
        _logger = logger;
    }

    public string ChannelCode => "SMS";

    public Task<NotificationSendResult> SendAsync(NotificationSendRequest request, CancellationToken ct = default)
    {
        var providerMessageId = $"mock-sms-{DateTime.UtcNow:yyyyMMddHHmmssfff}";
        _logger.LogInformation(
            "Mock SMS sender da xu ly delivery {DeliveryId} toi {Recipient} voi providerMessageId {ProviderMessageId}.",
            request.DeliveryId,
            request.Recipient,
            providerMessageId);

        return Task.FromResult(new NotificationSendResult
        {
            Success = true,
            ProviderMessageId = providerMessageId
        });
    }
}
