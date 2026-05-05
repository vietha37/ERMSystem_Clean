using System;
using System.Collections.Generic;

namespace ERMSystem.Application.DTOs
{
    public class HospitalPatientPortalOverviewDto
    {
        public HospitalPatientPortalProfileDto Profile { get; set; } = new();
        public IReadOnlyList<HospitalPatientPortalAppointmentDto> UpcomingAppointments { get; set; } = Array.Empty<HospitalPatientPortalAppointmentDto>();
        public IReadOnlyList<HospitalPatientPortalAppointmentDto> RecentAppointments { get; set; } = Array.Empty<HospitalPatientPortalAppointmentDto>();
    }

    public class HospitalPatientPortalProfileDto
    {
        public Guid PatientId { get; set; }
        public string MedicalRecordNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateOnly DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string PortalStatus { get; set; } = string.Empty;
        public DateTime ActivatedAtUtc { get; set; }
    }

    public class HospitalPatientPortalAppointmentDto
    {
        public Guid AppointmentId { get; set; }
        public string AppointmentNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string AppointmentType { get; set; } = string.Empty;
        public string BookingChannel { get; set; } = string.Empty;
        public DateTime AppointmentStartLocal { get; set; }
        public DateTime? AppointmentEndLocal { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string SpecialtyName { get; set; } = string.Empty;
        public string ClinicName { get; set; } = string.Empty;
        public string? ChiefComplaint { get; set; }
    }
}
