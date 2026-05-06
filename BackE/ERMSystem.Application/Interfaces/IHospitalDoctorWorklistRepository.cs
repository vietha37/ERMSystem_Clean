using System;
using System.Threading;
using System.Threading.Tasks;

namespace ERMSystem.Application.Interfaces;

public interface IHospitalDoctorWorklistRepository
{
    Task<HospitalDoctorProfileSnapshot?> GetDoctorProfileAsync(Guid doctorProfileId, CancellationToken ct = default);
    Task<HospitalDoctorProfileSnapshot?> ResolveDoctorByUsernameAsync(string username, CancellationToken ct = default);
    Task<HospitalDoctorWorklistSnapshot[]> GetWorklistAsync(DateOnly workDate, Guid? doctorProfileId, CancellationToken ct = default);
}

public class HospitalDoctorProfileSnapshot
{
    public Guid DoctorProfileId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string SpecialtyName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
}

public class HospitalDoctorWorklistSnapshot
{
    public Guid AppointmentId { get; set; }
    public string AppointmentNumber { get; set; } = string.Empty;
    public string AppointmentStatus { get; set; } = string.Empty;
    public DateTime AppointmentStartUtc { get; set; }
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
}
