using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERMSystem.Infrastructure.HospitalData.Entities;

[Table("Encounters", Schema = "emr")]
public class HospitalEncounterEntity
{
    [Key]
    public Guid Id { get; set; }

    [MaxLength(50)]
    public string EncounterNumber { get; set; } = string.Empty;

    public Guid PatientId { get; set; }
    public Guid? AppointmentId { get; set; }
    public Guid DoctorProfileId { get; set; }
    public Guid ClinicId { get; set; }

    [MaxLength(50)]
    public string EncounterType { get; set; } = string.Empty;

    [MaxLength(30)]
    public string EncounterStatus { get; set; } = string.Empty;

    public DateTime StartedAtUtc { get; set; }
    public DateTime? EndedAtUtc { get; set; }
    public string? Summary { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    public HospitalPatientEntity Patient { get; set; } = null!;
    public HospitalAppointmentEntity? Appointment { get; set; }
    public HospitalDoctorProfileEntity DoctorProfile { get; set; } = null!;
    public HospitalClinicEntity Clinic { get; set; } = null!;
    public ICollection<HospitalVitalSignEntity> VitalSigns { get; set; } = new List<HospitalVitalSignEntity>();
    public ICollection<HospitalDiagnosisEntity> Diagnoses { get; set; } = new List<HospitalDiagnosisEntity>();
    public ICollection<HospitalClinicalNoteEntity> ClinicalNotes { get; set; } = new List<HospitalClinicalNoteEntity>();
}

[Table("VitalSigns", Schema = "emr")]
public class HospitalVitalSignEntity
{
    [Key]
    public Guid Id { get; set; }

    public Guid EncounterId { get; set; }
    public decimal? HeightCm { get; set; }
    public decimal? WeightKg { get; set; }
    public decimal? TemperatureC { get; set; }
    public int? PulseRate { get; set; }
    public int? RespiratoryRate { get; set; }
    public int? SystolicBp { get; set; }
    public int? DiastolicBp { get; set; }
    public decimal? OxygenSaturation { get; set; }
    public DateTime RecordedAtUtc { get; set; }
    public Guid? RecordedByUserId { get; set; }

    public HospitalEncounterEntity Encounter { get; set; } = null!;
    public HospitalUserEntity? RecordedByUser { get; set; }
}

[Table("Diagnoses", Schema = "emr")]
public class HospitalDiagnosisEntity
{
    [Key]
    public Guid Id { get; set; }

    public Guid EncounterId { get; set; }

    [MaxLength(50)]
    public string DiagnosisType { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? DiagnosisCode { get; set; }

    [MaxLength(255)]
    public string DiagnosisName { get; set; } = string.Empty;

    public bool IsPrimary { get; set; }
    public DateTime NotedAtUtc { get; set; }

    public HospitalEncounterEntity Encounter { get; set; } = null!;
}

[Table("ClinicalNotes", Schema = "emr")]
public class HospitalClinicalNoteEntity
{
    [Key]
    public Guid Id { get; set; }

    public Guid EncounterId { get; set; }

    [MaxLength(50)]
    public string NoteType { get; set; } = string.Empty;

    public string? Subjective { get; set; }
    public string? Objective { get; set; }
    public string? Assessment { get; set; }
    public string? CarePlan { get; set; }
    public Guid? AuthoredByUserId { get; set; }
    public DateTime AuthoredAtUtc { get; set; }
    public DateTime? SignedAtUtc { get; set; }

    public HospitalEncounterEntity Encounter { get; set; } = null!;
    public HospitalUserEntity? AuthoredByUser { get; set; }
}

[Table("OrderHeaders", Schema = "emr")]
public class HospitalOrderHeaderEntity
{
    [Key]
    public Guid Id { get; set; }

    public Guid EncounterId { get; set; }

    [MaxLength(50)]
    public string OrderNumber { get; set; } = string.Empty;

    [MaxLength(50)]
    public string OrderCategory { get; set; } = string.Empty;

    [MaxLength(30)]
    public string OrderStatus { get; set; } = string.Empty;

    public Guid? OrderedByUserId { get; set; }
    public DateTime OrderedAtUtc { get; set; }

    public HospitalEncounterEntity Encounter { get; set; } = null!;
    public HospitalUserEntity? OrderedByUser { get; set; }
}
