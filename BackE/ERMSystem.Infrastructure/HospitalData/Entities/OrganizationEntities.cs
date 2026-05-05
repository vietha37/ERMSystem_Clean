using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERMSystem.Infrastructure.HospitalData.Entities;

[Table("Departments", Schema = "org")]
public class HospitalDepartmentEntity
{
    [Key]
    public Guid Id { get; set; }

    [MaxLength(50)]
    public string DepartmentCode { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public ICollection<HospitalSpecialtyEntity> Specialties { get; set; } = new List<HospitalSpecialtyEntity>();
    public ICollection<HospitalClinicEntity> Clinics { get; set; } = new List<HospitalClinicEntity>();
    public ICollection<HospitalStaffProfileEntity> StaffProfiles { get; set; } = new List<HospitalStaffProfileEntity>();
}

[Table("Specialties", Schema = "org")]
public class HospitalSpecialtyEntity
{
    [Key]
    public Guid Id { get; set; }

    [MaxLength(50)]
    public string SpecialtyCode { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public Guid? DepartmentId { get; set; }
    public bool IsActive { get; set; }

    public HospitalDepartmentEntity? Department { get; set; }
    public ICollection<HospitalDoctorProfileEntity> DoctorProfiles { get; set; } = new List<HospitalDoctorProfileEntity>();
}

[Table("Clinics", Schema = "org")]
public class HospitalClinicEntity
{
    [Key]
    public Guid Id { get; set; }

    [MaxLength(50)]
    public string ClinicCode { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public Guid? DepartmentId { get; set; }

    [MaxLength(50)]
    public string? FloorLabel { get; set; }

    [MaxLength(50)]
    public string? RoomLabel { get; set; }

    public bool IsActive { get; set; }

    public HospitalDepartmentEntity? Department { get; set; }
    public ICollection<HospitalDoctorScheduleEntity> DoctorSchedules { get; set; } = new List<HospitalDoctorScheduleEntity>();
}

[Table("StaffProfiles", Schema = "org")]
public class HospitalStaffProfileEntity
{
    [Key]
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    [MaxLength(50)]
    public string StaffCode { get; set; } = string.Empty;

    [MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    public Guid? DepartmentId { get; set; }

    [MaxLength(30)]
    public string? Phone { get; set; }

    [MaxLength(255)]
    public string? Email { get; set; }

    public DateOnly? HireDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public HospitalUserEntity User { get; set; } = null!;
    public HospitalDepartmentEntity? Department { get; set; }
    public HospitalDoctorProfileEntity? DoctorProfile { get; set; }
}

[Table("DoctorProfiles", Schema = "org")]
public class HospitalDoctorProfileEntity
{
    [Key]
    public Guid Id { get; set; }

    public Guid StaffProfileId { get; set; }
    public Guid SpecialtyId { get; set; }

    [MaxLength(100)]
    public string? LicenseNumber { get; set; }

    public string? Biography { get; set; }
    public int? YearsOfExperience { get; set; }
    public decimal? ConsultationFee { get; set; }
    public bool IsBookable { get; set; }

    public HospitalStaffProfileEntity StaffProfile { get; set; } = null!;
    public HospitalSpecialtyEntity Specialty { get; set; } = null!;
    public ICollection<HospitalDoctorScheduleEntity> DoctorSchedules { get; set; } = new List<HospitalDoctorScheduleEntity>();
    public ICollection<HospitalAppointmentEntity> Appointments { get; set; } = new List<HospitalAppointmentEntity>();
}

[Table("DoctorSchedules", Schema = "org")]
public class HospitalDoctorScheduleEntity
{
    [Key]
    public Guid Id { get; set; }

    public Guid DoctorProfileId { get; set; }
    public Guid ClinicId { get; set; }
    public byte DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int SlotMinutes { get; set; }
    public DateOnly ValidFrom { get; set; }
    public DateOnly? ValidTo { get; set; }
    public bool IsActive { get; set; }

    public HospitalDoctorProfileEntity DoctorProfile { get; set; } = null!;
    public HospitalClinicEntity Clinic { get; set; } = null!;
    public ICollection<HospitalAppointmentSlotEntity> AppointmentSlots { get; set; } = new List<HospitalAppointmentSlotEntity>();
}
