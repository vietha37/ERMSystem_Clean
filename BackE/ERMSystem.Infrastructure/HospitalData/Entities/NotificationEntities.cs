using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERMSystem.Infrastructure.HospitalData.Entities;

[Table("OutboxMessages", Schema = "notification")]
public class HospitalOutboxMessageEntity
{
    [Key]
    public Guid Id { get; set; }

    [MaxLength(100)]
    public string AggregateType { get; set; } = string.Empty;

    public Guid AggregateId { get; set; }

    [MaxLength(100)]
    public string EventType { get; set; } = string.Empty;

    public string PayloadJson { get; set; } = string.Empty;

    [MaxLength(30)]
    public string Status { get; set; } = string.Empty;

    public DateTime AvailableAtUtc { get; set; }
    public DateTime? PublishedAtUtc { get; set; }
}

[Table("NotificationTemplates", Schema = "notification")]
public class HospitalNotificationTemplateEntity
{
    [Key]
    public Guid Id { get; set; }

    [MaxLength(100)]
    public string TemplateCode { get; set; } = string.Empty;

    [MaxLength(30)]
    public string ChannelCode { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? SubjectTemplate { get; set; }

    public string BodyTemplate { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

[Table("NotificationDeliveries", Schema = "notification")]
public class HospitalNotificationDeliveryEntity
{
    [Key]
    public Guid Id { get; set; }

    public Guid OutboxMessageId { get; set; }

    [MaxLength(30)]
    public string ChannelCode { get; set; } = string.Empty;

    [MaxLength(255)]
    public string Recipient { get; set; } = string.Empty;

    [MaxLength(30)]
    public string DeliveryStatus { get; set; } = string.Empty;

    [MaxLength(150)]
    public string? ProviderMessageId { get; set; }

    public int AttemptCount { get; set; }
    public DateTime? LastAttemptAtUtc { get; set; }
    public DateTime? DeliveredAtUtc { get; set; }

    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }

    public HospitalOutboxMessageEntity OutboxMessage { get; set; } = null!;
}
