using Microsoft.Extensions.Logging;

namespace ERMSystem.Infrastructure.Messaging;

public class MockEmailNotificationSender : INotificationChannelSender
{
    private readonly ILogger<MockEmailNotificationSender> _logger;

    public MockEmailNotificationSender(ILogger<MockEmailNotificationSender> logger)
    {
        _logger = logger;
    }

    public string ChannelCode => "Email";

    public Task<NotificationSendResult> SendAsync(NotificationSendRequest request, CancellationToken ct = default)
    {
        var providerMessageId = $"mock-email-{DateTime.UtcNow:yyyyMMddHHmmssfff}";
        _logger.LogInformation(
            "Mock Email sender da xu ly delivery {DeliveryId} toi {Recipient} voi providerMessageId {ProviderMessageId}.",
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
