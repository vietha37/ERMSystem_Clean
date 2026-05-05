using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERMSystem.Infrastructure.HospitalData.Entities;

[Table("InboxMessages", Schema = "integration")]
public class IntegrationInboxMessageEntity
{
    [Key]
    public Guid Id { get; set; }

    [MaxLength(100)]
    public string SourceSystem { get; set; } = string.Empty;

    [MaxLength(150)]
    public string MessageKey { get; set; } = string.Empty;

    [MaxLength(100)]
    public string EventType { get; set; } = string.Empty;

    public string PayloadJson { get; set; } = string.Empty;
    public DateTime? ProcessedAtUtc { get; set; }

    [MaxLength(30)]
    public string Status { get; set; } = string.Empty;
}
