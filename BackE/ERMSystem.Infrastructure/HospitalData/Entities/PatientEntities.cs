using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERMSystem.Infrastructure.HospitalData.Entities;

[Table("Patients", Schema = "patient")]
public class HospitalPatientEntity
{
    [Key]
    public Guid Id { get; set; }

    [MaxLength(50)]
    public string MedicalRecordNumber { get; set; } = string.Empty;

    [MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    public DateOnly DateOfBirth { get; set; }

    [MaxLength(20)]
    public string Gender { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? Phone { get; set; }

    [MaxLength(255)]
    public string? Email { get; set; }

    [MaxLength(255)]
    public string? AddressLine1 { get; set; }

    [MaxLength(255)]
    public string? AddressLine2 { get; set; }

    [MaxLength(150)]
    public string? Ward { get; set; }

    [MaxLength(150)]
    public string? District { get; set; }

    [MaxLength(150)]
    public string? Province { get; set; }

    [MaxLength(100)]
    public string? Nationality { get; set; }

    [MaxLength(100)]
    public string? IdentityNumber { get; set; }

    [MaxLength(150)]
    public string? Occupation { get; set; }

    [MaxLength(50)]
    public string? MaritalStatus { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public HospitalPatientAccountEntity? PatientAccount { get; set; }
    public ICollection<HospitalPatientIdentifierEntity> Identifiers { get; set; } = new List<HospitalPatientIdentifierEntity>();
    public ICollection<HospitalPatientContactEntity> Contacts { get; set; } = new List<HospitalPatientContactEntity>();
    public ICollection<HospitalPatientEmergencyContactEntity> EmergencyContacts { get; set; } = new List<HospitalPatientEmergencyContactEntity>();
    public ICollection<HospitalPatientInsurancePolicyEntity> InsurancePolicies { get; set; } = new List<HospitalPatientInsurancePolicyEntity>();
    public ICollection<HospitalPatientConsentEntity> Consents { get; set; } = new List<HospitalPatientConsentEntity>();
    public ICollection<HospitalAppointmentEntity> Appointments { get; set; } = new List<HospitalAppointmentEntity>();
}

[Table("PatientAccounts", Schema = "patient")]
public class HospitalPatientAccountEntity
{
    [Key]
    public Guid PatientId { get; set; }

    public Guid UserId { get; set; }
    public DateTime ActivatedAtUtc { get; set; }

    [MaxLength(30)]
    public string PortalStatus { get; set; } = string.Empty;

    public HospitalPatientEntity Patient { get; set; } = null!;
    public HospitalUserEntity User { get; set; } = null!;
}

[Table("PatientIdentifiers", Schema = "patient")]
public class HospitalPatientIdentifierEntity
{
    [Key]
    public Guid Id { get; set; }

    public Guid PatientId { get; set; }

    [MaxLength(50)]
    public string IdentifierType { get; set; } = string.Empty;

    [MaxLength(150)]
    public string IdentifierValue { get; set; } = string.Empty;

    public bool IsPrimary { get; set; }
    public DateTime? IssuedAtUtc { get; set; }

    public HospitalPatientEntity Patient { get; set; } = null!;
}

[Table("PatientContacts", Schema = "patient")]
public class HospitalPatientContactEntity
{
    [Key]
    public Guid Id { get; set; }

    public Guid PatientId { get; set; }

    [MaxLength(50)]
    public string ContactType { get; set; } = string.Empty;

    [MaxLength(255)]
    public string ContactValue { get; set; } = string.Empty;

    public bool IsPrimary { get; set; }

    public HospitalPatientEntity Patient { get; set; } = null!;
}

[Table("PatientEmergencyContacts", Schema = "patient")]
public class HospitalPatientEmergencyContactEntity
{
    [Key]
    public Guid Id { get; set; }

    public Guid PatientId { get; set; }

    [MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Relationship { get; set; } = string.Empty;

    [MaxLength(30)]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Address { get; set; }

    public HospitalPatientEntity Patient { get; set; } = null!;
}

[Table("PatientInsurancePolicies", Schema = "patient")]
public class HospitalPatientInsurancePolicyEntity
{
    [Key]
    public Guid Id { get; set; }

    public Guid PatientId { get; set; }

    [MaxLength(200)]
    public string ProviderName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string PolicyNumber { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? CardNumber { get; set; }

    public DateOnly? EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public decimal? CoveragePercent { get; set; }
    public bool IsActive { get; set; }

    public HospitalPatientEntity Patient { get; set; } = null!;
}

[Table("PatientConsents", Schema = "patient")]
public class HospitalPatientConsentEntity
{
    [Key]
    public Guid Id { get; set; }

    public Guid PatientId { get; set; }

    [MaxLength(100)]
    public string ConsentType { get; set; } = string.Empty;

    public DateTime GrantedAtUtc { get; set; }
    public DateTime? RevokedAtUtc { get; set; }

    [MaxLength(1000)]
    public string? EvidenceUri { get; set; }

    public HospitalPatientEntity Patient { get; set; } = null!;
}
