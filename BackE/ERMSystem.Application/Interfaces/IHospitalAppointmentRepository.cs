using System;
using System.Threading;
using System.Threading.Tasks;

namespace ERMSystem.Application.Interfaces
{
    public interface IHospitalAppointmentRepository
    {
        Task<HospitalBookingPatientSnapshot?> FindMatchingPatientAsync(
            string fullName,
            DateOnly dateOfBirth,
            string phone,
            string? email,
            CancellationToken ct = default);

        Task<bool> HasDoctorConflictAsync(
            Guid doctorProfileId,
            DateTime appointmentStartUtc,
            DateTime appointmentEndUtc,
            CancellationToken ct = default);

        Task AddPatientAsync(HospitalBookingPatientCreateCommand patient, CancellationToken ct = default);
        Task AddAppointmentAsync(HospitalBookingAppointmentCreateCommand appointment, CancellationToken ct = default);
        Task AddOutboxMessageAsync(HospitalOutboxMessageCreateCommand message, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }

    public class HospitalBookingPatientSnapshot
    {
        public Guid PatientId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public DateOnly DateOfBirth { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
    }

    public class HospitalBookingPatientCreateCommand
    {
        public Guid PatientId { get; set; }
        public string MedicalRecordNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateOnly DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }

    public class HospitalBookingAppointmentCreateCommand
    {
        public Guid AppointmentId { get; set; }
        public string AppointmentNumber { get; set; } = string.Empty;
        public Guid PatientId { get; set; }
        public Guid DoctorProfileId { get; set; }
        public Guid ClinicId { get; set; }
        public string AppointmentType { get; set; } = string.Empty;
        public string BookingChannel { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime AppointmentStartUtc { get; set; }
        public DateTime AppointmentEndUtc { get; set; }
        public string? ChiefComplaint { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }

    public class HospitalOutboxMessageCreateCommand
    {
        public Guid OutboxMessageId { get; set; }
        public string AggregateType { get; set; } = string.Empty;
        public Guid AggregateId { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string PayloadJson { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime AvailableAtUtc { get; set; }
    }
}
