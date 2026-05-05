using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERMSystem.Infrastructure.HospitalData.Entities;

[Table("AppointmentSlots", Schema = "scheduling")]
public class HospitalAppointmentSlotEntity
{
    [Key]
    public Guid Id { get; set; }

    public Guid DoctorScheduleId { get; set; }
    public DateTime SlotStartUtc { get; set; }
    public DateTime SlotEndUtc { get; set; }
    public int Capacity { get; set; }
    public int ReservedCount { get; set; }

    [MaxLength(30)]
    public string SlotStatus { get; set; } = string.Empty;

    public HospitalDoctorScheduleEntity DoctorSchedule { get; set; } = null!;
    public ICollection<HospitalAppointmentEntity> Appointments { get; set; } = new List<HospitalAppointmentEntity>();
}

[Table("Appointments", Schema = "scheduling")]
public class HospitalAppointmentEntity
{
    [Key]
    public Guid Id { get; set; }

    [MaxLength(50)]
    public string AppointmentNumber { get; set; } = string.Empty;

    public Guid PatientId { get; set; }
    public Guid DoctorProfileId { get; set; }
    public Guid ClinicId { get; set; }
    public Guid? AppointmentSlotId { get; set; }

    [MaxLength(50)]
    public string AppointmentType { get; set; } = string.Empty;

    [MaxLength(50)]
    public string BookingChannel { get; set; } = string.Empty;

    [MaxLength(30)]
    public string Status { get; set; } = string.Empty;

    public DateTime AppointmentStartUtc { get; set; }
    public DateTime? AppointmentEndUtc { get; set; }

    [MaxLength(1000)]
    public string? ChiefComplaint { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public Guid? CreatedByUserId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    public HospitalPatientEntity Patient { get; set; } = null!;
    public HospitalDoctorProfileEntity DoctorProfile { get; set; } = null!;
    public HospitalClinicEntity Clinic { get; set; } = null!;
    public HospitalAppointmentSlotEntity? AppointmentSlot { get; set; }
    public HospitalUserEntity? CreatedByUser { get; set; }
    public HospitalCheckInEntity? CheckIn { get; set; }
    public ICollection<HospitalQueueTicketEntity> QueueTickets { get; set; } = new List<HospitalQueueTicketEntity>();
}

[Table("CheckIns", Schema = "scheduling")]
public class HospitalCheckInEntity
{
    [Key]
    public Guid Id { get; set; }

    public Guid AppointmentId { get; set; }
    public DateTime CheckInTimeUtc { get; set; }

    [MaxLength(50)]
    public string? CounterLabel { get; set; }

    [MaxLength(30)]
    public string CheckInStatus { get; set; } = string.Empty;

    public HospitalAppointmentEntity Appointment { get; set; } = null!;
}

[Table("QueueTickets", Schema = "scheduling")]
public class HospitalQueueTicketEntity
{
    [Key]
    public Guid Id { get; set; }

    public Guid AppointmentId { get; set; }

    [MaxLength(30)]
    public string QueueNumber { get; set; } = string.Empty;

    [MaxLength(30)]
    public string QueueStatus { get; set; } = string.Empty;

    public DateTime? CalledAtUtc { get; set; }
    public DateTime? ServedAtUtc { get; set; }

    public HospitalAppointmentEntity Appointment { get; set; } = null!;
}
