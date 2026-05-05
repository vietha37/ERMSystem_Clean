using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ERMSystem.Application.DTOs;
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
    }
}
