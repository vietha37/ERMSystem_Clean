using ERMSystem.Application.Interfaces;
using ERMSystem.Infrastructure.HospitalData;
using ERMSystem.Infrastructure.HospitalData.Entities;
using Microsoft.EntityFrameworkCore;

namespace ERMSystem.Infrastructure.Repositories;

public class HospitalAppointmentRepository : IHospitalAppointmentRepository
{
    private readonly HospitalDbContext _hospitalDbContext;

    public HospitalAppointmentRepository(HospitalDbContext hospitalDbContext)
    {
        _hospitalDbContext = hospitalDbContext;
    }

    public async Task<HospitalBookingPatientSnapshot?> FindMatchingPatientAsync(
        string fullName,
        DateOnly dateOfBirth,
        string phone,
        string? email,
        CancellationToken ct = default)
    {
        var normalizedFullName = fullName.Trim();
        var normalizedPhone = phone.Trim();
        var normalizedEmail = string.IsNullOrWhiteSpace(email) ? null : email.Trim();

        var patient = await _hospitalDbContext.Patients
            .AsNoTracking()
            .Where(x => x.DeletedAtUtc == null)
            .Where(x =>
                (x.Phone != null && x.Phone == normalizedPhone) ||
                (normalizedEmail != null && x.Email != null && x.Email == normalizedEmail) ||
                (x.FullName == normalizedFullName && x.DateOfBirth == dateOfBirth))
            .OrderByDescending(x => x.UpdatedAtUtc)
            .FirstOrDefaultAsync(ct);

        if (patient == null)
        {
            return null;
        }

        return new HospitalBookingPatientSnapshot
        {
            PatientId = patient.Id,
            FullName = patient.FullName,
            DateOfBirth = patient.DateOfBirth,
            Phone = patient.Phone,
            Email = patient.Email
        };
    }

    public Task<bool> HasDoctorConflictAsync(
        Guid doctorProfileId,
        DateTime appointmentStartUtc,
        DateTime appointmentEndUtc,
        CancellationToken ct = default)
    {
        return _hospitalDbContext.Appointments
            .AsNoTracking()
            .Where(x => x.DoctorProfileId == doctorProfileId)
            .Where(x => x.Status != "Cancelled")
            .AnyAsync(
                x => x.AppointmentStartUtc < appointmentEndUtc &&
                     (x.AppointmentEndUtc ?? x.AppointmentStartUtc) > appointmentStartUtc,
                ct);
    }

    public Task AddPatientAsync(HospitalBookingPatientCreateCommand patient, CancellationToken ct = default)
    {
        _hospitalDbContext.Patients.Add(new HospitalPatientEntity
        {
            Id = patient.PatientId,
            MedicalRecordNumber = patient.MedicalRecordNumber,
            FullName = patient.FullName,
            DateOfBirth = patient.DateOfBirth,
            Gender = patient.Gender,
            Phone = patient.Phone,
            Email = patient.Email,
            CreatedAtUtc = patient.CreatedAtUtc,
            UpdatedAtUtc = patient.UpdatedAtUtc
        });

        return Task.CompletedTask;
    }

    public Task AddAppointmentAsync(HospitalBookingAppointmentCreateCommand appointment, CancellationToken ct = default)
    {
        _hospitalDbContext.Appointments.Add(new HospitalAppointmentEntity
        {
            Id = appointment.AppointmentId,
            AppointmentNumber = appointment.AppointmentNumber,
            PatientId = appointment.PatientId,
            DoctorProfileId = appointment.DoctorProfileId,
            ClinicId = appointment.ClinicId,
            AppointmentType = appointment.AppointmentType,
            BookingChannel = appointment.BookingChannel,
            Status = appointment.Status,
            AppointmentStartUtc = appointment.AppointmentStartUtc,
            AppointmentEndUtc = appointment.AppointmentEndUtc,
            ChiefComplaint = appointment.ChiefComplaint,
            Notes = appointment.Notes,
            CreatedAtUtc = appointment.CreatedAtUtc,
            UpdatedAtUtc = appointment.UpdatedAtUtc
        });

        return Task.CompletedTask;
    }

    public Task AddOutboxMessageAsync(HospitalOutboxMessageCreateCommand message, CancellationToken ct = default)
    {
        _hospitalDbContext.OutboxMessages.Add(new HospitalOutboxMessageEntity
        {
            Id = message.OutboxMessageId,
            AggregateType = message.AggregateType,
            AggregateId = message.AggregateId,
            EventType = message.EventType,
            PayloadJson = message.PayloadJson,
            Status = message.Status,
            AvailableAtUtc = message.AvailableAtUtc
        });

        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _hospitalDbContext.SaveChangesAsync(ct);
}
