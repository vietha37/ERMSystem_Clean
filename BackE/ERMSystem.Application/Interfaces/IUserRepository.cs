using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using ERMSystem.Domain.Entities;

namespace ERMSystem.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<AppUser?> GetByUsernameAsync(string username);
        Task<bool> UsernameExistsAsync(string username);
        Task AddAsync(AppUser user);
        Task<AppUser?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<bool> UsernameExistsAsync(string username, Guid excludeId, CancellationToken ct = default);
        Task<(IEnumerable<AppUser> Items, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? role = null,
            string? textSearch = null,
            CancellationToken ct = default);
        Task UpdateAsync(AppUser user, CancellationToken ct = default);
        Task DeleteAsync(AppUser user, CancellationToken ct = default);
    }
}
