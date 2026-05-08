using ERMSystem.Application.DTOs;
using ERMSystem.Application.Interfaces;
using ERMSystem.Domain.Entities;
using ERMSystem.Infrastructure.HospitalData;
using ERMSystem.Infrastructure.HospitalData.Entities;
using Microsoft.EntityFrameworkCore;

namespace ERMSystem.Infrastructure.Services;

public class HospitalIdentityBridgeService : IHospitalIdentityBridgeService
{
    private readonly HospitalDbContext _hospitalDbContext;

    public HospitalIdentityBridgeService(HospitalDbContext hospitalDbContext)
    {
        _hospitalDbContext = hospitalDbContext;
    }

    public async Task SyncInternalUserAsync(AppUser user, string? previousUsername = null, CancellationToken ct = default)
    {
        var hospitalUser = await FindHospitalUserForProjectionAsync(user.Id, user.Username, previousUsername, ct);
        var nowUtc = DateTime.UtcNow;
        var normalizedUsername = user.Username.Trim();

        if (hospitalUser == null)
        {
            hospitalUser = new HospitalUserEntity
            {
                Id = user.Id,
                Username = normalizedUsername,
                PasswordHash = user.PasswordHash,
                PrimaryRoleCode = user.Role,
                IsActive = true,
                CreatedAtUtc = nowUtc,
                UpdatedAtUtc = nowUtc
            };

            _hospitalDbContext.Users.Add(hospitalUser);
        }
        else
        {
            hospitalUser.Username = normalizedUsername;
            hospitalUser.PasswordHash = user.PasswordHash;
            hospitalUser.PrimaryRoleCode = user.Role;
            hospitalUser.IsActive = true;
            hospitalUser.DeletedAtUtc = null;
            hospitalUser.UpdatedAtUtc = nowUtc;
        }

        var hasUserRole = await _hospitalDbContext.UserRoles.AnyAsync(
            x => x.UserId == hospitalUser.Id && x.RoleCode == user.Role,
            ct);

        if (!hasUserRole)
        {
            _hospitalDbContext.UserRoles.Add(new HospitalUserRoleEntity
            {
                UserId = hospitalUser.Id,
                RoleCode = user.Role,
                GrantedAtUtc = nowUtc,
                GrantedByUserId = null
            });
        }

        await _hospitalDbContext.SaveChangesAsync(ct);
    }

    public async Task DeactivateInternalUserAsync(AppUser user, CancellationToken ct = default)
    {
        var hospitalUser = await FindHospitalUserForProjectionAsync(user.Id, user.Username, null, ct);
        if (hospitalUser == null)
        {
            return;
        }

        var nowUtc = DateTime.UtcNow;
        hospitalUser.IsActive = false;
        hospitalUser.DeletedAtUtc ??= nowUtc;
        hospitalUser.UpdatedAtUtc = nowUtc;

        await _hospitalDbContext.SaveChangesAsync(ct);
    }

    public async Task<Guid?> ResolveHospitalUserIdAsync(Guid? legacyUserId, string? username, CancellationToken ct = default)
    {
        if (legacyUserId.HasValue)
        {
            var byId = await _hospitalDbContext.Users
                .AsNoTracking()
                .Where(x => x.DeletedAtUtc == null)
                .Where(x => x.Id == legacyUserId.Value)
                .Select(x => (Guid?)x.Id)
                .FirstOrDefaultAsync(ct);

            if (byId.HasValue)
            {
                return byId.Value;
            }
        }

        var normalizedUsername = username?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedUsername))
        {
            return null;
        }

        return await _hospitalDbContext.Users
            .AsNoTracking()
            .Where(x => x.DeletedAtUtc == null)
            .Where(x => x.Username == normalizedUsername)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<HospitalInternalUserSyncResultDto> SyncInternalUsersAsync(
        IEnumerable<AppUser> users,
        CancellationToken ct = default)
    {
        var internalUsers = users
            .Where(x => Array.Exists(AppRole.Internal, role => role == x.Role))
            .ToArray();

        foreach (var user in internalUsers)
        {
            await SyncInternalUserAsync(user, null, ct);
        }

        return new HospitalInternalUserSyncResultDto
        {
            TotalUsers = internalUsers.Length,
            SyncedUsers = internalUsers.Length
        };
    }

    private async Task<HospitalUserEntity?> FindHospitalUserForProjectionAsync(
        Guid legacyUserId,
        string username,
        string? previousUsername,
        CancellationToken ct)
    {
        var normalizedUsername = username.Trim();
        var normalizedPreviousUsername = string.IsNullOrWhiteSpace(previousUsername) ? null : previousUsername.Trim();

        var hospitalUser = await _hospitalDbContext.Users
            .FirstOrDefaultAsync(x => x.Id == legacyUserId, ct);

        if (hospitalUser != null)
        {
            return hospitalUser;
        }

        hospitalUser = await _hospitalDbContext.Users
            .FirstOrDefaultAsync(x => x.Username == normalizedUsername, ct);

        if (hospitalUser != null)
        {
            return hospitalUser;
        }

        if (string.IsNullOrWhiteSpace(normalizedPreviousUsername))
        {
            return null;
        }

        return await _hospitalDbContext.Users
            .FirstOrDefaultAsync(x => x.Username == normalizedPreviousUsername, ct);
    }
}
