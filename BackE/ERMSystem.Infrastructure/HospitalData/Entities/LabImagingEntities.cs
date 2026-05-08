using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERMSystem.Infrastructure.HospitalData.Entities;

[Table("LabServices", Schema = "lab")]
public class HospitalLabServiceEntity
{
    [Key]
    public Guid Id { get; set; }

    [MaxLength(50)]
    public string ServiceCode { get; set; } = string.Empty;

    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? SampleType { get; set; }

    public decimal? UnitPrice { get; set; }
    public bool IsActive { get; set; }
}

[Table("LabOrders", Schema = "lab")]
public class HospitalLabOrderEntity
{
    [Key]
    public Guid Id { get; set; }

    public Guid OrderHeaderId { get; set; }
    public Guid LabServiceId { get; set; }

    [MaxLength(30)]
    public string OrderStatus { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? PriorityCode { get; set; }

    public DateTime RequestedAtUtc { get; set; }
    public DateTime? ResultedAtUtc { get; set; }

    public HospitalOrderHeaderEntity OrderHeader { get; set; } = null!;
    public HospitalLabServiceEntity LabService { get; set; } = null!;
    public ICollection<HospitalSpecimenEntity> Specimens { get; set; } = new List<HospitalSpecimenEntity>();
    public ICollection<HospitalLabResultItemEntity> ResultItems { get; set; } = new List<HospitalLabResultItemEntity>();
}

[Table("Specimens", Schema = "lab")]
public class HospitalSpecimenEntity
{
    [Key]
    public Guid Id { get; set; }

    public Guid LabOrderId { get; set; }

    [MaxLength(50)]
    public string SpecimenCode { get; set; } = string.Empty;

    public DateTime? CollectedAtUtc { get; set; }
    public DateTime? ReceivedAtUtc { get; set; }

    [MaxLength(30)]
    public string Status { get; set; } = string.Empty;

    public HospitalLabOrderEntity LabOrder { get; set; } = null!;
}

[Table("LabResultItems", Schema = "lab")]
public class HospitalLabResultItemEntity
{
    [Key]
    public Guid Id { get; set; }

    public Guid LabOrderId { get; set; }

    [MaxLength(50)]
    public string? AnalyteCode { get; set; }

    [MaxLength(255)]
    public string AnalyteName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? ResultValue { get; set; }

    [MaxLength(50)]
    public string? Unit { get; set; }

    [MaxLength(100)]
    public string? ReferenceRange { get; set; }

    [MaxLength(20)]
    public string? AbnormalFlag { get; set; }

    public DateTime? VerifiedAtUtc { get; set; }

    public HospitalLabOrderEntity LabOrder { get; set; } = null!;
}

[Table("ImagingServices", Schema = "imaging")]
public class HospitalImagingServiceEntity
{
    [Key]
    public Guid Id { get; set; }

    [MaxLength(50)]
    public string ServiceCode { get; set; } = string.Empty;

    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Modality { get; set; }

    public decimal? UnitPrice { get; set; }
    public bool IsActive { get; set; }
}

[Table("ImagingOrders", Schema = "imaging")]
public class HospitalImagingOrderEntity
{
    [Key]
    public Guid Id { get; set; }

    public Guid OrderHeaderId { get; set; }
    public Guid ImagingServiceId { get; set; }

    [MaxLength(30)]
    public string OrderStatus { get; set; } = string.Empty;

    public DateTime RequestedAtUtc { get; set; }
    public DateTime? ReportedAtUtc { get; set; }

    public HospitalOrderHeaderEntity OrderHeader { get; set; } = null!;
    public HospitalImagingServiceEntity ImagingService { get; set; } = null!;
    public HospitalImagingReportEntity? ImagingReport { get; set; }
}

[Table("ImagingReports", Schema = "imaging")]
public class HospitalImagingReportEntity
{
    [Key]
    public Guid Id { get; set; }

    public Guid ImagingOrderId { get; set; }
    public string? Findings { get; set; }
    public string? Impression { get; set; }

    [MaxLength(1000)]
    public string? ReportUri { get; set; }

    public Guid? SignedByUserId { get; set; }
    public DateTime? SignedAtUtc { get; set; }

    public HospitalImagingOrderEntity ImagingOrder { get; set; } = null!;
    public HospitalUserEntity? SignedByUser { get; set; }
}
