using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ERMSystem.Domain.Entities;

namespace ERMSystem.Application.Interfaces
{
    public interface IMedicineRepository
    {
        Task<List<Medicine>> GetAllAsync(CancellationToken ct = default);
        Task<(IEnumerable<Medicine> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, CancellationToken ct = default);
        Task<Medicine?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task AddAsync(Medicine medicine, CancellationToken ct = default);
        Task UpdateAsync(Medicine medicine, CancellationToken ct = default);
        Task DeleteAsync(Medicine medicine, CancellationToken ct = default);
    }
}
