using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ERMSystem.Application.DTOs;
using ERMSystem.Application.Interfaces;
using ERMSystem.Infrastructure.HospitalData;
using Microsoft.EntityFrameworkCore;

namespace ERMSystem.Infrastructure.Repositories
{
    public class HospitalPatientPortalRepository : IHospitalPatientPortalRepository
    {
        private readonly HospitalDbContext _hospitalDbContext;

        public HospitalPatientPortalRepository(HospitalDbContext hospitalDbContext)
        {
            _hospitalDbContext = hospitalDbContext;
        }

        public async Task<HospitalPatientPortalOverviewDto?> GetOverviewByUserIdAsync(Guid userId, CancellationToken ct = default)
        {
            var account = await _hospitalDbContext.PatientAccounts
                .AsNoTracking()
                .Include(x => x.Patient)
                .Where(x => x.UserId == userId)
                .Where(x => x.Patient.DeletedAtUtc == null)
                .Select(x => new
                {
                    x.PatientId,
                    x.PortalStatus,
                    x.ActivatedAtUtc,
                    Patient = x.Patient
                })
                .FirstOrDefaultAsync(ct);

            if (account == null)
            {
                return null;
            }

            var appointments = await _hospitalDbContext.Appointments
                .AsNoTracking()
                .Where(x => x.PatientId == account.PatientId)
                .Include(x => x.DoctorProfile)
                    .ThenInclude(x => x.StaffProfile)
                .Include(x => x.DoctorProfile)
                    .ThenInclude(x => x.Specialty)
                .Include(x => x.Clinic)
                .OrderByDescending(x => x.AppointmentStartUtc)
                .Select(x => new HospitalPatientPortalAppointmentDto
                {
                    AppointmentId = x.Id,
                    AppointmentNumber = x.AppointmentNumber,
                    Status = x.Status,
                    AppointmentType = x.AppointmentType,
                    BookingChannel = x.BookingChannel,
                    AppointmentStartLocal = ConvertUtcToClinicLocal(x.AppointmentStartUtc),
                    AppointmentEndLocal = x.AppointmentEndUtc.HasValue
                        ? ConvertUtcToClinicLocal(x.AppointmentEndUtc.Value)
                        : null,
                    DoctorName = x.DoctorProfile.StaffProfile.FullName,
                    SpecialtyName = x.DoctorProfile.Specialty.Name,
                    ClinicName = x.Clinic.Name,
                    ChiefComplaint = x.ChiefComplaint
                })
                .ToListAsync(ct);

            var nowLocal = ConvertUtcToClinicLocal(DateTime.UtcNow);

            return new HospitalPatientPortalOverviewDto
            {
                Profile = new HospitalPatientPortalProfileDto
                {
                    PatientId = account.Patient.Id,
                    MedicalRecordNumber = account.Patient.MedicalRecordNumber,
                    FullName = account.Patient.FullName,
                    DateOfBirth = account.Patient.DateOfBirth,
                    Gender = account.Patient.Gender,
                    Phone = account.Patient.Phone,
                    Email = account.Patient.Email,
                    Address = BuildAddress(account.Patient.AddressLine1, account.Patient.Ward, account.Patient.District, account.Patient.Province),
                    PortalStatus = account.PortalStatus,
                    ActivatedAtUtc = account.ActivatedAtUtc
                },
                UpcomingAppointments = appointments
                    .Where(x => x.AppointmentStartLocal >= nowLocal)
                    .OrderBy(x => x.AppointmentStartLocal)
                    .Take(5)
                    .ToArray(),
                RecentAppointments = appointments
                    .Where(x => x.AppointmentStartLocal < nowLocal)
                    .OrderByDescending(x => x.AppointmentStartLocal)
                    .Take(5)
                    .ToArray()
            };
        }

        private static string? BuildAddress(string? addressLine1, string? ward, string? district, string? province)
        {
            var merged = string.Join(", ", new[] { addressLine1, ward, district, province }
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x!.Trim()));

            return string.IsNullOrWhiteSpace(merged) ? null : merged;
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

        private static DateTime ConvertUtcToClinicLocal(DateTime utcDateTime)
            => TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc), ResolveClinicTimeZone());
    }
}
