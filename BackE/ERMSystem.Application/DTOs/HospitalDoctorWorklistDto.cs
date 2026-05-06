using System;
using System.Collections.Generic;

namespace ERMSystem.Application.DTOs;

public class HospitalDoctorWorklistRequestDto
{
    public DateOnly? WorkDate { get; set; }
    public Guid? DoctorProfileId { get; set; }
}

public class HospitalDoctorWorklistItemDto
{
    public Guid AppointmentId { get; set; }
    public string AppointmentNumber { get; set; } = string.Empty;
    public string AppointmentStatus { get; set; } = string.Empty;
    public DateTime AppointmentStartLocal { get; set; }
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string MedicalRecordNumber { get; set; } = string.Empty;
    public Guid DoctorProfileId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string SpecialtyName { get; set; } = string.Empty;
    public string ClinicName { get; set; } = string.Empty;
    public Guid? EncounterId { get; set; }
    public string? EncounterNumber { get; set; }
    public string? EncounterStatus { get; set; }
    public string? PrimaryDiagnosisName { get; set; }
    public Guid? PrescriptionId { get; set; }
    public string? PrescriptionNumber { get; set; }
    public string WorkflowStage { get; set; } = string.Empty;
}

public class HospitalDoctorWorklistResponseDto
{
    public DateOnly WorkDate { get; set; }
    public Guid? DoctorProfileId { get; set; }
    public string? DoctorName { get; set; }
    public string? SpecialtyName { get; set; }
    public bool IsDoctorResolved { get; set; }
    public string? ResolutionMessage { get; set; }
    public int TotalAppointments { get; set; }
    public int CheckedInAppointments { get; set; }
    public int InProgressEncounters { get; set; }
    public int FinalizedEncounters { get; set; }
    public int IssuedPrescriptions { get; set; }
    public List<HospitalDoctorWorklistItemDto> Items { get; set; } = new();
}
