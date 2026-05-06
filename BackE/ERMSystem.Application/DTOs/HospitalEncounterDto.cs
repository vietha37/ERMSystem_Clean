using System;
using System.ComponentModel.DataAnnotations;

namespace ERMSystem.Application.DTOs;

public class HospitalEncounterWorklistRequestDto
{
    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 10;

    [MaxLength(30)]
    public string? EncounterStatus { get; set; }

    public DateOnly? AppointmentDate { get; set; }

    [MaxLength(200)]
    public string? TextSearch { get; set; }
}

public class HospitalEncounterSummaryDto
{
    public Guid EncounterId { get; set; }
    public string EncounterNumber { get; set; } = string.Empty;
    public Guid? AppointmentId { get; set; }
    public string? AppointmentNumber { get; set; }
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string MedicalRecordNumber { get; set; } = string.Empty;
    public Guid DoctorProfileId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string SpecialtyName { get; set; } = string.Empty;
    public string ClinicName { get; set; } = string.Empty;
    public DateTime? AppointmentStartLocal { get; set; }
    public string EncounterStatus { get; set; } = string.Empty;
    public string? PrimaryDiagnosisName { get; set; }
    public string? Summary { get; set; }
    public DateTime StartedAtLocal { get; set; }
    public DateTime? EndedAtLocal { get; set; }
    public DateTime UpdatedAtLocal { get; set; }
}

public class HospitalEncounterDetailDto : HospitalEncounterSummaryDto
{
    public string EncounterType { get; set; } = string.Empty;
    public string? DiagnosisCode { get; set; }
    public string? DiagnosisType { get; set; }
    public string? Subjective { get; set; }
    public string? Objective { get; set; }
    public string? Assessment { get; set; }
    public string? CarePlan { get; set; }
    public decimal? HeightCm { get; set; }
    public decimal? WeightKg { get; set; }
    public decimal? TemperatureC { get; set; }
    public int? PulseRate { get; set; }
    public int? RespiratoryRate { get; set; }
    public int? SystolicBp { get; set; }
    public int? DiastolicBp { get; set; }
    public decimal? OxygenSaturation { get; set; }
}

public class HospitalEncounterEligibleAppointmentDto
{
    public Guid AppointmentId { get; set; }
    public string AppointmentNumber { get; set; } = string.Empty;
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string MedicalRecordNumber { get; set; } = string.Empty;
    public Guid DoctorProfileId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string SpecialtyName { get; set; } = string.Empty;
    public string ClinicName { get; set; } = string.Empty;
    public DateTime AppointmentStartLocal { get; set; }
    public string AppointmentStatus { get; set; } = string.Empty;
    public Guid? ExistingEncounterId { get; set; }
    public string? ExistingEncounterNumber { get; set; }
}

public class CreateHospitalEncounterDto
{
    [Required]
    public Guid AppointmentId { get; set; }

    [Required]
    [MaxLength(255)]
    public string DiagnosisName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? DiagnosisCode { get; set; }

    [MaxLength(50)]
    public string DiagnosisType { get; set; } = "Working";

    [MaxLength(30)]
    public string EncounterStatus { get; set; } = "InProgress";

    public string? Summary { get; set; }
    public string? Subjective { get; set; }
    public string? Objective { get; set; }
    public string? Assessment { get; set; }
    public string? CarePlan { get; set; }
    public decimal? HeightCm { get; set; }
    public decimal? WeightKg { get; set; }
    public decimal? TemperatureC { get; set; }
    public int? PulseRate { get; set; }
    public int? RespiratoryRate { get; set; }
    public int? SystolicBp { get; set; }
    public int? DiastolicBp { get; set; }
    public decimal? OxygenSaturation { get; set; }
}

public class UpdateHospitalEncounterDto
{
    [Required]
    [MaxLength(255)]
    public string DiagnosisName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? DiagnosisCode { get; set; }

    [MaxLength(50)]
    public string DiagnosisType { get; set; } = "Working";

    [Required]
    [MaxLength(30)]
    public string EncounterStatus { get; set; } = "InProgress";

    public string? Summary { get; set; }
    public string? Subjective { get; set; }
    public string? Objective { get; set; }
    public string? Assessment { get; set; }
    public string? CarePlan { get; set; }
    public decimal? HeightCm { get; set; }
    public decimal? WeightKg { get; set; }
    public decimal? TemperatureC { get; set; }
    public int? PulseRate { get; set; }
    public int? RespiratoryRate { get; set; }
    public int? SystolicBp { get; set; }
    public int? DiastolicBp { get; set; }
    public decimal? OxygenSaturation { get; set; }
}
