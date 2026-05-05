using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERMSystem.Infrastructure.HospitalData.Entities;

[Table("Users", Schema = "identity")]
public class HospitalUserEntity
{
    [Key]
    public Guid Id { get; set; }

    [MaxLength(150)]
    public string Username { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Email { get; set; }

    [MaxLength(500)]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(50)]
    public string PrimaryRoleCode { get; set; } = string.Empty;

    public bool IsActive { get; set; }
    public DateTime? EmailVerifiedAtUtc { get; set; }
    public DateTime? LastLoginAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public ICollection<HospitalUserRoleEntity> UserRoles { get; set; } = new List<HospitalUserRoleEntity>();
    public ICollection<HospitalRefreshTokenEntity> RefreshTokens { get; set; } = new List<HospitalRefreshTokenEntity>();
    public ICollection<HospitalUserSessionEntity> UserSessions { get; set; } = new List<HospitalUserSessionEntity>();
    public ICollection<HospitalSecurityEventEntity> SecurityEvents { get; set; } = new List<HospitalSecurityEventEntity>();
}

[Table("Roles", Schema = "identity")]
public class HospitalRoleEntity
{
    [Key]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public bool IsSystemRole { get; set; }

    public ICollection<HospitalUserRoleEntity> UserRoles { get; set; } = new List<HospitalUserRoleEntity>();
}

[Table("UserRoles", Schema = "identity")]
public class HospitalUserRoleEntity
{
    public Guid UserId { get; set; }

    [MaxLength(50)]
    public string RoleCode { get; set; } = string.Empty;

    public DateTime GrantedAtUtc { get; set; }
    public Guid? GrantedByUserId { get; set; }

    public HospitalUserEntity User { get; set; } = null!;
    public HospitalRoleEntity Role { get; set; } = null!;
}

[Table("RefreshTokens", Schema = "identity")]
public class HospitalRefreshTokenEntity
{
    [Key]
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    [MaxLength(500)]
    public string TokenHash { get; set; } = string.Empty;

    [MaxLength(150)]
    public string? DeviceName { get; set; }

    [MaxLength(64)]
    public string? DeviceIp { get; set; }

    [MaxLength(1000)]
    public string? UserAgent { get; set; }

    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? RotatedAtUtc { get; set; }
    public DateTime? RevokedAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public HospitalUserEntity User { get; set; } = null!;
}

[Table("UserSessions", Schema = "identity")]
public class HospitalUserSessionEntity
{
    [Key]
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    [MaxLength(100)]
    public string SessionCode { get; set; } = string.Empty;

    [MaxLength(150)]
    public string? DeviceName { get; set; }

    [MaxLength(64)]
    public string? DeviceIp { get; set; }

    [MaxLength(1000)]
    public string? UserAgent { get; set; }

    public DateTime StartedAtUtc { get; set; }
    public DateTime? LastSeenAtUtc { get; set; }
    public DateTime? EndedAtUtc { get; set; }

    public HospitalUserEntity User { get; set; } = null!;
}

[Table("SecurityEvents", Schema = "identity")]
public class HospitalSecurityEventEntity
{
    [Key]
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    [MaxLength(100)]
    public string EventType { get; set; } = string.Empty;

    [MaxLength(30)]
    public string Severity { get; set; } = string.Empty;

    public string? Detail { get; set; }

    [MaxLength(64)]
    public string? IpAddress { get; set; }

    [MaxLength(1000)]
    public string? UserAgent { get; set; }

    public DateTime OccurredAtUtc { get; set; }

    public HospitalUserEntity? User { get; set; }
}
