using ERMSystem.Application.DTOs;
using ERMSystem.Domain.Entities;

namespace ERMSystem.Application.Interfaces;

public interface IHospitalIdentityBridgeService
{
    Task SyncInternalUserAsync(AppUser user, string? previousUsername = null, CancellationToken ct = default);
    Task DeactivateInternalUserAsync(AppUser user, CancellationToken ct = default);
    Task<Guid?> ResolveHospitalUserIdAsync(Guid? legacyUserId, string? username, CancellationToken ct = default);
    Task<HospitalInternalUserSyncResultDto> SyncInternalUsersAsync(IEnumerable<AppUser> users, CancellationToken ct = default);
}
