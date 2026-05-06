using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERMSystem.Infrastructure.HospitalData.Entities;

[Table("Medicines", Schema = "pharmacy")]
public class HospitalMedicineEntity
{
    [Key]
    public Guid Id { get; set; }

    [MaxLength(50)]
    public string DrugCode { get; set; } = string.Empty;

    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? GenericName { get; set; }

    [MaxLength(100)]
    public string? Strength { get; set; }

    [MaxLength(100)]
    public string? DosageForm { get; set; }

    [MaxLength(50)]
    public string? Unit { get; set; }

    public bool IsControlled { get; set; }
    public bool IsActive { get; set; }

    public ICollection<HospitalPrescriptionItemEntity> PrescriptionItems { get; set; } = new List<HospitalPrescriptionItemEntity>();
}

[Table("Prescriptions", Schema = "pharmacy")]
public class HospitalPrescriptionEntity
{
    [Key]
    public Guid Id { get; set; }

    public Guid OrderHeaderId { get; set; }

    [MaxLength(50)]
    public string PrescriptionNumber { get; set; } = string.Empty;

    [MaxLength(30)]
    public string Status { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public HospitalOrderHeaderEntity OrderHeader { get; set; } = null!;
    public ICollection<HospitalPrescriptionItemEntity> PrescriptionItems { get; set; } = new List<HospitalPrescriptionItemEntity>();
    public ICollection<HospitalDispensingEntity> Dispensings { get; set; } = new List<HospitalDispensingEntity>();
}

[Table("PrescriptionItems", Schema = "pharmacy")]
public class HospitalPrescriptionItemEntity
{
    [Key]
    public Guid Id { get; set; }

    public Guid PrescriptionId { get; set; }
    public Guid MedicineId { get; set; }

    [MaxLength(255)]
    public string DoseInstruction { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Route { get; set; }

    [MaxLength(100)]
    public string? Frequency { get; set; }

    public int? DurationDays { get; set; }
    public decimal Quantity { get; set; }
    public decimal? UnitPrice { get; set; }

    public HospitalPrescriptionEntity Prescription { get; set; } = null!;
    public HospitalMedicineEntity Medicine { get; set; } = null!;
}

[Table("Dispensings", Schema = "pharmacy")]
public class HospitalDispensingEntity
{
    [Key]
    public Guid Id { get; set; }

    public Guid PrescriptionId { get; set; }

    [MaxLength(30)]
    public string DispensingStatus { get; set; } = string.Empty;

    public DateTime? DispensedAtUtc { get; set; }
    public Guid? DispensedByUserId { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public HospitalPrescriptionEntity Prescription { get; set; } = null!;
    public HospitalUserEntity? DispensedByUser { get; set; }
}
