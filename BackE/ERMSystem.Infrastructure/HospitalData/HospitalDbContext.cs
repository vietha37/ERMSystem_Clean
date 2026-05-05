using ERMSystem.Infrastructure.HospitalData.Entities;
using Microsoft.EntityFrameworkCore;

namespace ERMSystem.Infrastructure.HospitalData;

public class HospitalDbContext : DbContext
{
    public HospitalDbContext(DbContextOptions<HospitalDbContext> options) : base(options)
    {
    }

    public DbSet<HospitalUserEntity> Users => Set<HospitalUserEntity>();
    public DbSet<HospitalRoleEntity> Roles => Set<HospitalRoleEntity>();
    public DbSet<HospitalUserRoleEntity> UserRoles => Set<HospitalUserRoleEntity>();
    public DbSet<HospitalRefreshTokenEntity> RefreshTokens => Set<HospitalRefreshTokenEntity>();
    public DbSet<HospitalUserSessionEntity> UserSessions => Set<HospitalUserSessionEntity>();
    public DbSet<HospitalSecurityEventEntity> SecurityEvents => Set<HospitalSecurityEventEntity>();

    public DbSet<HospitalDepartmentEntity> Departments => Set<HospitalDepartmentEntity>();
    public DbSet<HospitalSpecialtyEntity> Specialties => Set<HospitalSpecialtyEntity>();
    public DbSet<HospitalClinicEntity> Clinics => Set<HospitalClinicEntity>();
    public DbSet<HospitalStaffProfileEntity> StaffProfiles => Set<HospitalStaffProfileEntity>();
    public DbSet<HospitalDoctorProfileEntity> DoctorProfiles => Set<HospitalDoctorProfileEntity>();
    public DbSet<HospitalDoctorScheduleEntity> DoctorSchedules => Set<HospitalDoctorScheduleEntity>();

    public DbSet<HospitalPatientEntity> Patients => Set<HospitalPatientEntity>();
    public DbSet<HospitalPatientAccountEntity> PatientAccounts => Set<HospitalPatientAccountEntity>();
    public DbSet<HospitalPatientIdentifierEntity> PatientIdentifiers => Set<HospitalPatientIdentifierEntity>();
    public DbSet<HospitalPatientContactEntity> PatientContacts => Set<HospitalPatientContactEntity>();
    public DbSet<HospitalPatientEmergencyContactEntity> PatientEmergencyContacts => Set<HospitalPatientEmergencyContactEntity>();
    public DbSet<HospitalPatientInsurancePolicyEntity> PatientInsurancePolicies => Set<HospitalPatientInsurancePolicyEntity>();
    public DbSet<HospitalPatientConsentEntity> PatientConsents => Set<HospitalPatientConsentEntity>();

    public DbSet<HospitalAppointmentSlotEntity> AppointmentSlots => Set<HospitalAppointmentSlotEntity>();
    public DbSet<HospitalAppointmentEntity> Appointments => Set<HospitalAppointmentEntity>();
    public DbSet<HospitalCheckInEntity> CheckIns => Set<HospitalCheckInEntity>();
    public DbSet<HospitalQueueTicketEntity> QueueTickets => Set<HospitalQueueTicketEntity>();

    public DbSet<HospitalServiceCatalogEntity> ServiceCatalog => Set<HospitalServiceCatalogEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<HospitalUserEntity>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<HospitalPatientEntity>().Property(x => x.RowVersion).IsRowVersion();

        modelBuilder.Entity<HospitalUserRoleEntity>().HasKey(x => new { x.UserId, x.RoleCode });

        modelBuilder.Entity<HospitalUserRoleEntity>()
            .HasOne(x => x.User)
            .WithMany(x => x.UserRoles)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<HospitalUserRoleEntity>()
            .HasOne(x => x.Role)
            .WithMany(x => x.UserRoles)
            .HasForeignKey(x => x.RoleCode)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<HospitalRefreshTokenEntity>()
            .HasOne(x => x.User)
            .WithMany(x => x.RefreshTokens)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<HospitalUserSessionEntity>()
            .HasOne(x => x.User)
            .WithMany(x => x.UserSessions)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<HospitalSecurityEventEntity>()
            .HasOne(x => x.User)
            .WithMany(x => x.SecurityEvents)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<HospitalSpecialtyEntity>()
            .HasOne(x => x.Department)
            .WithMany(x => x.Specialties)
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<HospitalClinicEntity>()
            .HasOne(x => x.Department)
            .WithMany(x => x.Clinics)
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<HospitalStaffProfileEntity>()
            .HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<HospitalStaffProfileEntity>()
            .HasOne(x => x.Department)
            .WithMany(x => x.StaffProfiles)
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<HospitalDoctorProfileEntity>()
            .HasOne(x => x.StaffProfile)
            .WithOne(x => x.DoctorProfile)
            .HasForeignKey<HospitalDoctorProfileEntity>(x => x.StaffProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<HospitalDoctorProfileEntity>()
            .HasOne(x => x.Specialty)
            .WithMany(x => x.DoctorProfiles)
            .HasForeignKey(x => x.SpecialtyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<HospitalDoctorScheduleEntity>()
            .HasOne(x => x.DoctorProfile)
            .WithMany(x => x.DoctorSchedules)
            .HasForeignKey(x => x.DoctorProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<HospitalDoctorScheduleEntity>()
            .HasOne(x => x.Clinic)
            .WithMany(x => x.DoctorSchedules)
            .HasForeignKey(x => x.ClinicId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<HospitalPatientAccountEntity>()
            .HasOne(x => x.Patient)
            .WithOne(x => x.PatientAccount)
            .HasForeignKey<HospitalPatientAccountEntity>(x => x.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<HospitalPatientAccountEntity>()
            .HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<HospitalPatientIdentifierEntity>()
            .HasOne(x => x.Patient)
            .WithMany(x => x.Identifiers)
            .HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<HospitalPatientContactEntity>()
            .HasOne(x => x.Patient)
            .WithMany(x => x.Contacts)
            .HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<HospitalPatientEmergencyContactEntity>()
            .HasOne(x => x.Patient)
            .WithMany(x => x.EmergencyContacts)
            .HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<HospitalPatientInsurancePolicyEntity>()
            .HasOne(x => x.Patient)
            .WithMany(x => x.InsurancePolicies)
            .HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<HospitalPatientConsentEntity>()
            .HasOne(x => x.Patient)
            .WithMany(x => x.Consents)
            .HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<HospitalAppointmentSlotEntity>()
            .HasOne(x => x.DoctorSchedule)
            .WithMany(x => x.AppointmentSlots)
            .HasForeignKey(x => x.DoctorScheduleId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<HospitalAppointmentEntity>()
            .HasOne(x => x.Patient)
            .WithMany(x => x.Appointments)
            .HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<HospitalAppointmentEntity>()
            .HasOne(x => x.DoctorProfile)
            .WithMany(x => x.Appointments)
            .HasForeignKey(x => x.DoctorProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<HospitalAppointmentEntity>()
            .HasOne(x => x.Clinic)
            .WithMany()
            .HasForeignKey(x => x.ClinicId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<HospitalAppointmentEntity>()
            .HasOne(x => x.AppointmentSlot)
            .WithMany(x => x.Appointments)
            .HasForeignKey(x => x.AppointmentSlotId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<HospitalAppointmentEntity>()
            .HasOne(x => x.CreatedByUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<HospitalCheckInEntity>()
            .HasOne(x => x.Appointment)
            .WithOne(x => x.CheckIn)
            .HasForeignKey<HospitalCheckInEntity>(x => x.AppointmentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<HospitalQueueTicketEntity>()
            .HasOne(x => x.Appointment)
            .WithMany(x => x.QueueTickets)
            .HasForeignKey(x => x.AppointmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
