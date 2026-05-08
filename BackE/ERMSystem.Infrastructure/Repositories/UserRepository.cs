using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ERMSystem.Application.Interfaces;
using ERMSystem.Domain.Entities;
using ERMSystem.Infrastructure.Data;

namespace ERMSystem.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AppUser?> GetByUsernameAsync(string username)
        {
            return await _context.AppUsers
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await _context.AppUsers.AnyAsync(u => u.Username == username);
        }

        public async Task AddAsync(AppUser user)
        {
            await _context.AppUsers.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task<AppUser?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.AppUsers.FindAsync(new object[] { id }, ct);
        }

        public async Task<bool> UsernameExistsAsync(string username, Guid excludeId, CancellationToken ct = default)
        {
            return await _context.AppUsers.AnyAsync(
                u => u.Username == username && u.Id != excludeId,
                ct);
        }

        public async Task<(IEnumerable<AppUser> Items, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? role = null,
            string? textSearch = null,
            CancellationToken ct = default)
        {
            var query = _context.AppUsers.AsQueryable();

            if (!string.IsNullOrWhiteSpace(role))
            {
                query = query.Where(u => u.Role == role);
            }
            else
            {
                query = query.Where(u => u.Role == AppRole.Doctor || u.Role == AppRole.Receptionist);
            }

            if (!string.IsNullOrWhiteSpace(textSearch))
            {
                var keyword = textSearch.Trim();
                var pattern = $"%{keyword}%";
                query = query.Where(u =>
                    EF.Functions.Like(u.Username, pattern) ||
                    EF.Functions.Like(u.Role, pattern));
            }

            var totalCount = await query.CountAsync(ct);
            var items = await query
                .OrderBy(u => u.Username)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, totalCount);
        }

        public async Task UpdateAsync(AppUser user, CancellationToken ct = default)
        {
            _context.AppUsers.Update(user);
            await _context.SaveChangesAsync(ct);
        }

        public async Task<IReadOnlyList<AppUser>> GetInternalUsersAsync(CancellationToken ct = default)
        {
            return await _context.AppUsers
                .Where(u => u.Role == AppRole.Admin || u.Role == AppRole.Doctor || u.Role == AppRole.Receptionist)
                .OrderBy(u => u.Username)
                .ToListAsync(ct);
        }

        public async Task DeleteAsync(AppUser user, CancellationToken ct = default)
        {
            _context.AppUsers.Remove(user);
            await _context.SaveChangesAsync(ct);
        }
    }
}
