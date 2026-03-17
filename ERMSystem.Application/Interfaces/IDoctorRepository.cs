using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ERMSystem.Domain.Entities;

namespace ERMSystem.Application.Interfaces
{
    public interface IDoctorRepository
    {
        Task<List<Doctor>> GetAllAsync(CancellationToken ct = default);
        Task<(IEnumerable<Doctor> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, CancellationToken ct = default);
        Task<Doctor?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task AddAsync(Doctor doctor, CancellationToken ct = default);
        Task UpdateAsync(Doctor doctor, CancellationToken ct = default);
        Task DeleteAsync(Doctor doctor, CancellationToken ct = default);
    }
}
