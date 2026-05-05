using System;
using System.Collections.Generic;

namespace ERMSystem.Application.DTOs
{
    public class NotificationDeliveryDto
    {
        public Guid Id { get; set; }
        public Guid OutboxMessageId { get; set; }
        public string ChannelCode { get; set; } = string.Empty;
        public string Recipient { get; set; } = string.Empty;
        public string DeliveryStatus { get; set; } = string.Empty;
        public string? ProviderMessageId { get; set; }
        public int AttemptCount { get; set; }
        public DateTime? LastAttemptAtUtc { get; set; }
        public DateTime? DeliveredAtUtc { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class NotificationDeliveryListDto
    {
        public int TotalCount { get; set; }
        public IReadOnlyList<NotificationDeliveryDto> Items { get; set; } = Array.Empty<NotificationDeliveryDto>();
    }
}
