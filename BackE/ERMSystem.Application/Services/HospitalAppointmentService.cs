using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ERMSystem.Application.DTOs;
using ERMSystem.Application.DTOs.Common;
using ERMSystem.Application.Interfaces;

namespace ERMSystem.Application.Services
{
    public class HospitalAppointmentService : IHospitalAppointmentService
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private readonly IHospitalDoctorRepository _hospitalDoctorRepository;
        private readonly IHospitalAppointmentRepository _hospitalAppointmentRepository;

        public HospitalAppointmentService(
            IHospitalDoctorRepository hospitalDoctorRepository,
            IHospitalAppointmentRepository hospitalAppointmentRepository)
        {
            _hospitalDoctorRepository = hospitalDoctorRepository;
            _hospitalAppointmentRepository = hospitalAppointmentRepository;
        }

        public async Task<HospitalAppointmentBookingResultDto> BookPublicAppointmentAsync(
            PublicHospitalAppointmentBookingRequestDto request,
            CancellationToken ct = default)
        {
            var doctor = await _hospitalDoctorRepository.GetDoctorByIdAsync(request.DoctorProfileId, ct);
            if (doctor == null)
            {
                throw new KeyNotFoundException("Khong tim thay bac si duoc chon.");
            }

            if (!doctor.IsBookable)
            {
                throw new InvalidOperationException("Bac si hien khong nhan dat lich online.");
            }

            if (request.SpecialtyId.HasValue && request.SpecialtyId.Value != doctor.SpecialtyId)
            {
                throw new InvalidOperationException("Chuyen khoa va bac si duoc chon khong khop nhau.");
            }

            var matchingSchedule = doctor.Schedules
                .Where(x => x.DayOfWeek == (byte)request.PreferredDate.DayOfWeek)
                .Where(x => x.ValidFrom <= request.PreferredDate && (!x.ValidTo.HasValue || x.ValidTo.Value >= request.PreferredDate))
                .Where(x => x.StartTime <= request.PreferredTime)
                .Where(x => request.PreferredTime.AddMinutes(x.SlotMinutes) <= x.EndTime)
                .OrderBy(x => x.StartTime)
                .FirstOrDefault();

            if (matchingSchedule == null)
            {
                throw new InvalidOperationException("Khung gio duoc chon khong nam trong lich lam viec cua bac si.");
            }

            var appointmentStartLocal = request.PreferredDate.ToDateTime(request.PreferredTime);
            var appointmentEndLocal = appointmentStartLocal.AddMinutes(matchingSchedule.SlotMinutes);
            var appointmentStartUtc = ConvertLocalClinicTimeToUtc(appointmentStartLocal);
            var appointmentEndUtc = ConvertLocalClinicTimeToUtc(appointmentEndLocal);

            var hasConflict = await _hospitalAppointmentRepository.HasDoctorConflictAsync(
                doctor.DoctorProfileId,
                appointmentStartUtc,
                appointmentEndUtc,
                ct);

            if (hasConflict)
            {
                throw new InvalidOperationException("Khung gio nay da co lich hen. Vui long chon gio khac.");
            }

            var existingPatient = await _hospitalAppointmentRepository.FindMatchingPatientAsync(
                request.FullName.Trim(),
                request.DateOfBirth,
                request.Phone.Trim(),
                string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim(),
                ct);

            var patientId = existingPatient?.PatientId ?? Guid.NewGuid();
            var isExistingPatient = existingPatient != null;
            var nowUtc = DateTime.UtcNow;

            if (!isExistingPatient)
            {
                await _hospitalAppointmentRepository.AddPatientAsync(new HospitalBookingPatientCreateCommand
                {
                    PatientId = patientId,
                    MedicalRecordNumber = GenerateMedicalRecordNumber(nowUtc),
                    FullName = request.FullName.Trim(),
                    DateOfBirth = request.DateOfBirth,
                    Gender = request.Gender.Trim(),
                    Phone = request.Phone.Trim(),
                    Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim(),
                    CreatedAtUtc = nowUtc,
                    UpdatedAtUtc = nowUtc
                }, ct);
            }

            var appointmentId = Guid.NewGuid();
            var appointmentNumber = GenerateAppointmentNumber(nowUtc);
            var normalizedNotes = BuildNotes(request.Notes, request.ServiceCode);

            await _hospitalAppointmentRepository.AddAppointmentAsync(new HospitalBookingAppointmentCreateCommand
            {
                AppointmentId = appointmentId,
                AppointmentNumber = appointmentNumber,
                PatientId = patientId,
                DoctorProfileId = doctor.DoctorProfileId,
                ClinicId = matchingSchedule.ClinicId,
                AppointmentType = "Outpatient",
                BookingChannel = "Website",
                Status = "Scheduled",
                AppointmentStartUtc = appointmentStartUtc,
                AppointmentEndUtc = appointmentEndUtc,
                ChiefComplaint = string.IsNullOrWhiteSpace(request.ChiefComplaint) ? null : request.ChiefComplaint.Trim(),
                Notes = normalizedNotes,
                CreatedAtUtc = nowUtc,
                UpdatedAtUtc = nowUtc
            }, ct);

            await _hospitalAppointmentRepository.AddOutboxMessageAsync(new HospitalOutboxMessageCreateCommand
            {
                OutboxMessageId = Guid.NewGuid(),
                AggregateType = "Appointment",
                AggregateId = appointmentId,
                EventType = "AppointmentCreated.v1",
                PayloadJson = JsonSerializer.Serialize(new
                {
                    appointmentId,
                    appointmentNumber,
                    patientId,
                    patientName = request.FullName.Trim(),
                    phone = request.Phone.Trim(),
                    email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim(),
                    doctorProfileId = doctor.DoctorProfileId,
                    doctorName = doctor.FullName,
                    specialtyName = doctor.SpecialtyName,
                    clinicName = matchingSchedule.ClinicName,
                    appointmentStartLocal,
                    appointmentEndLocal,
                    channel = "Website"
                }, JsonOptions),
                Status = "Pending",
                AvailableAtUtc = nowUtc
            }, ct);

            await _hospitalAppointmentRepository.SaveChangesAsync(ct);

            return new HospitalAppointmentBookingResultDto
            {
                AppointmentId = appointmentId,
                AppointmentNumber = appointmentNumber,
                PatientId = patientId,
                DoctorProfileId = doctor.DoctorProfileId,
                DoctorName = doctor.FullName,
                SpecialtyName = doctor.SpecialtyName,
                ClinicName = matchingSchedule.ClinicName,
                AppointmentStartLocal = appointmentStartLocal,
                AppointmentEndLocal = appointmentEndLocal,
                Status = "Scheduled",
                IsExistingPatient = isExistingPatient,
                NotificationQueued = true
            };
        }

        public Task<PaginatedResult<HospitalAppointmentWorklistItemDto>> GetWorklistAsync(
            HospitalAppointmentWorklistRequestDto request,
            CancellationToken ct = default)
            => _hospitalAppointmentRepository.GetWorklistAsync(request, ct);

        public async Task<HospitalAppointmentWorklistItemDto?> CheckInAsync(
            Guid appointmentId,
            HospitalAppointmentCheckInRequestDto request,
            CancellationToken ct = default)
        {
            var appointment = await _hospitalAppointmentRepository.GetAppointmentAggregateAsync(appointmentId, ct);
            if (appointment == null)
            {
                return null;
            }

            if (appointment.Status is "Cancelled" or "Completed")
            {
                throw new InvalidOperationException("Lich hen nay khong the check-in o trang thai hien tai.");
            }

            if (appointment.CheckInId == null)
            {
                await _hospitalAppointmentRepository.AddCheckInAsync(new HospitalAppointmentCheckInCommand
                {
                    CheckInId = Guid.NewGuid(),
                    AppointmentId = appointment.AppointmentId,
                    CheckInTimeUtc = DateTime.UtcNow,
                    CounterLabel = string.IsNullOrWhiteSpace(request.CounterLabel) ? null : request.CounterLabel.Trim(),
                    CheckInStatus = "CheckedIn"
                }, ct);
            }

            if (string.IsNullOrWhiteSpace(appointment.QueueNumber))
            {
                var nextSequence = await _hospitalAppointmentRepository.GetNextQueueSequenceAsync(appointment.AppointmentStartUtc, ct);
                await _hospitalAppointmentRepository.AddQueueTicketAsync(new HospitalAppointmentQueueTicketCreateCommand
                {
                    QueueTicketId = Guid.NewGuid(),
                    AppointmentId = appointment.AppointmentId,
                    QueueNumber = $"Q{nextSequence:000}",
                    QueueStatus = "Waiting"
                }, ct);
            }

            await _hospitalAppointmentRepository.UpdateStatusAsync(appointment.AppointmentId, "CheckedIn", ct);
            await _hospitalAppointmentRepository.SaveChangesAsync(ct);

            var refreshed = await _hospitalAppointmentRepository.GetAppointmentAggregateAsync(appointmentId, ct);
            return refreshed == null ? null : MapAggregateToWorklistItem(refreshed);
        }

        public async Task<HospitalAppointmentWorklistItemDto?> UpdateStatusAsync(
            Guid appointmentId,
            HospitalAppointmentStatusUpdateRequestDto request,
            CancellationToken ct = default)
        {
            var normalizedStatus = request.Status.Trim();
            var allowedStatuses = new[] { "Scheduled", "CheckedIn", "Completed", "Cancelled" };
            if (!allowedStatuses.Contains(normalizedStatus, StringComparer.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Trang thai lich hen khong hop le.");
            }

            var appointment = await _hospitalAppointmentRepository.GetAppointmentAggregateAsync(appointmentId, ct);
            if (appointment == null)
            {
                return null;
            }

            if (appointment.Status == "Cancelled" && !string.Equals(normalizedStatus, "Cancelled", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Lich hen da huy khong the chuyen sang trang thai khac.");
            }

            var previousStatus = appointment.Status;
            await _hospitalAppointmentRepository.UpdateStatusAsync(appointmentId, normalizedStatus, ct);

            if (!string.Equals(previousStatus, normalizedStatus, StringComparison.OrdinalIgnoreCase))
            {
                var appointmentStartLocal = ConvertUtcToClinicLocal(appointment.AppointmentStartUtc);
                var appointmentEndLocal = appointment.AppointmentEndUtc.HasValue
                    ? ConvertUtcToClinicLocal(appointment.AppointmentEndUtc.Value)
                    : (DateTime?)null;
                var eventType = string.Equals(normalizedStatus, "Cancelled", StringComparison.OrdinalIgnoreCase)
                    ? "AppointmentCancelled.v1"
                    : "AppointmentUpdated.v1";

                await _hospitalAppointmentRepository.AddOutboxMessageAsync(new HospitalOutboxMessageCreateCommand
                {
                    OutboxMessageId = Guid.NewGuid(),
                    AggregateType = "Appointment",
                    AggregateId = appointment.AppointmentId,
                    EventType = eventType,
                    PayloadJson = JsonSerializer.Serialize(new
                    {
                        appointmentId = appointment.AppointmentId,
                        appointmentNumber = appointment.AppointmentNumber,
                        patientId = appointment.PatientId,
                        patientName = appointment.PatientName,
                        phone = appointment.PatientPhone,
                        email = appointment.PatientEmail,
                        doctorProfileId = appointment.DoctorProfileId,
                        doctorName = appointment.DoctorName,
                        specialtyName = appointment.SpecialtyName,
                        clinicName = appointment.ClinicName,
                        appointmentStartLocal,
                        appointmentEndLocal,
                        previousStatus,
                        currentStatus = normalizedStatus,
                        channel = appointment.BookingChannel
                    }, JsonOptions),
                    Status = "Pending",
                    AvailableAtUtc = DateTime.UtcNow
                }, ct);
            }

            await _hospitalAppointmentRepository.SaveChangesAsync(ct);

            var refreshed = await _hospitalAppointmentRepository.GetAppointmentAggregateAsync(appointmentId, ct);
            return refreshed == null ? null : MapAggregateToWorklistItem(refreshed);
        }

        private static TimeZoneInfo ResolveClinicTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            }
            catch
            {
                return TimeZoneInfo.Utc;
            }
        }

        private static DateTime ConvertLocalClinicTimeToUtc(DateTime localDateTime)
        {
            return TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(localDateTime, DateTimeKind.Unspecified), ResolveClinicTimeZone());
        }

        private static DateTime ConvertUtcToClinicLocal(DateTime utcDateTime)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc), ResolveClinicTimeZone());
        }

        private static string GenerateMedicalRecordNumber(DateTime nowUtc)
            => $"MRN-{nowUtc:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}";

        private static string GenerateAppointmentNumber(DateTime nowUtc)
            => $"APT-{nowUtc:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}";

        private static string? BuildNotes(string? notes, string? serviceCode)
        {
            var parts = new[]
            {
                string.IsNullOrWhiteSpace(serviceCode) ? null : $"ServiceCode: {serviceCode.Trim()}",
                string.IsNullOrWhiteSpace(notes) ? null : notes.Trim()
            }.Where(x => !string.IsNullOrWhiteSpace(x));

            var merged = string.Join(" | ", parts);
            return string.IsNullOrWhiteSpace(merged) ? null : merged;
        }

        private static HospitalAppointmentWorklistItemDto MapAggregateToWorklistItem(HospitalAppointmentAggregateSnapshot aggregate)
        {
            return new HospitalAppointmentWorklistItemDto
            {
                AppointmentId = aggregate.AppointmentId,
                AppointmentNumber = aggregate.AppointmentNumber,
                PatientId = aggregate.PatientId,
                PatientName = aggregate.PatientName,
                MedicalRecordNumber = aggregate.MedicalRecordNumber,
                PatientPhone = aggregate.PatientPhone,
                DoctorProfileId = aggregate.DoctorProfileId,
                DoctorName = aggregate.DoctorName,
                SpecialtyName = aggregate.SpecialtyName,
                ClinicName = aggregate.ClinicName,
                FloorLabel = aggregate.FloorLabel,
                RoomLabel = aggregate.RoomLabel,
                AppointmentType = aggregate.AppointmentType,
                BookingChannel = aggregate.BookingChannel,
                Status = aggregate.Status,
                AppointmentStartLocal = ConvertUtcToClinicLocal(aggregate.AppointmentStartUtc),
                AppointmentEndLocal = aggregate.AppointmentEndUtc.HasValue
                    ? ConvertUtcToClinicLocal(aggregate.AppointmentEndUtc.Value)
                    : null,
                ChiefComplaint = aggregate.ChiefComplaint,
                CounterLabel = aggregate.CounterLabel,
                QueueNumber = aggregate.QueueNumber,
                CheckInTimeLocal = aggregate.CheckInTimeUtc.HasValue
                    ? ConvertUtcToClinicLocal(aggregate.CheckInTimeUtc.Value)
                    : null
            };
        }
    }
}
