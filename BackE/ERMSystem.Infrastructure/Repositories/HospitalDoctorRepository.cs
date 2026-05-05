using ERMSystem.Application.DTOs;
using ERMSystem.Application.Interfaces;
using ERMSystem.Infrastructure.HospitalData;
using Microsoft.EntityFrameworkCore;

namespace ERMSystem.Infrastructure.Repositories;

public class HospitalDoctorRepository : IHospitalDoctorRepository
{
    private readonly HospitalDbContext _hospitalDbContext;

    public HospitalDoctorRepository(HospitalDbContext hospitalDbContext)
    {
        _hospitalDbContext = hospitalDbContext;
    }

    public async Task<IReadOnlyList<HospitalDoctorDto>> GetDoctorsAsync(Guid? specialtyId = null, CancellationToken ct = default)
    {
        var query = _hospitalDbContext.DoctorProfiles
            .AsNoTracking()
            .Include(x => x.StaffProfile)
                .ThenInclude(x => x.Department)
            .Include(x => x.Specialty)
            .Include(x => x.DoctorSchedules)
                .ThenInclude(x => x.Clinic)
            .Where(x => x.IsBookable);

        if (specialtyId.HasValue)
        {
            query = query.Where(x => x.SpecialtyId == specialtyId.Value);
        }

        var doctors = await query
            .OrderBy(x => x.StaffProfile.FullName)
            .ToListAsync(ct);

        return doctors.Select(MapDoctor).ToList();
    }

    public async Task<HospitalDoctorDto?> GetDoctorByIdAsync(Guid doctorProfileId, CancellationToken ct = default)
    {
        var doctor = await _hospitalDbContext.DoctorProfiles
            .AsNoTracking()
            .Include(x => x.StaffProfile)
                .ThenInclude(x => x.Department)
            .Include(x => x.Specialty)
            .Include(x => x.DoctorSchedules)
                .ThenInclude(x => x.Clinic)
            .FirstOrDefaultAsync(x => x.Id == doctorProfileId, ct);

        return doctor == null ? null : MapDoctor(doctor);
    }

    private static HospitalDoctorDto MapDoctor(ERMSystem.Infrastructure.HospitalData.Entities.HospitalDoctorProfileEntity doctor)
    {
        return new HospitalDoctorDto
        {
            DoctorProfileId = doctor.Id,
            StaffProfileId = doctor.StaffProfileId,
            SpecialtyId = doctor.SpecialtyId,
            FullName = doctor.StaffProfile.FullName,
            SpecialtyName = doctor.Specialty.Name,
            DepartmentName = doctor.StaffProfile.Department?.Name ?? string.Empty,
            LicenseNumber = doctor.LicenseNumber,
            Biography = doctor.Biography,
            YearsOfExperience = doctor.YearsOfExperience,
            ConsultationFee = doctor.ConsultationFee,
            IsBookable = doctor.IsBookable,
            Schedules = doctor.DoctorSchedules
                .Where(x => x.IsActive)
                .OrderBy(x => x.DayOfWeek)
                .ThenBy(x => x.StartTime)
                .Select(x => new HospitalDoctorScheduleDto
                {
                    ScheduleId = x.Id,
                    ClinicId = x.ClinicId,
                    DayOfWeek = x.DayOfWeek,
                    StartTime = x.StartTime,
                    EndTime = x.EndTime,
                    SlotMinutes = x.SlotMinutes,
                    ValidFrom = x.ValidFrom,
                    ValidTo = x.ValidTo,
                    ClinicName = x.Clinic.Name,
                    FloorLabel = x.Clinic.FloorLabel,
                    RoomLabel = x.Clinic.RoomLabel
                })
                .ToList()
        };
    }
}
