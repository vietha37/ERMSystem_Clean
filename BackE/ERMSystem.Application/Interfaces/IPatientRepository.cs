using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ERMSystem.Domain.Entities;

namespace ERMSystem.Application.Interfaces
{
    public interface IPatientRepository
    {
        Task<List<Patient>> GetAllAsync(CancellationToken ct = default);
        Task<(IEnumerable<Patient> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, CancellationToken ct = default);
        Task<int> GetTotalCountAsync(CancellationToken ct = default);
        Task<Dictionary<DateTime, int>> GetCreatedCountByDayAsync(DateTime fromUtc, CancellationToken ct = default);
        Task<Patient?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task AddAsync(Patient patient, CancellationToken ct = default);
        Task UpdateAsync(Patient patient, CancellationToken ct = default);
        Task DeleteAsync(Patient patient, CancellationToken ct = default);
    }
}
