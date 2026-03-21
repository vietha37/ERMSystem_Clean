using System;
using System.Threading.Tasks;
using ERMSystem.Domain.Entities;

namespace ERMSystem.Application.Interfaces
{
    public interface IPrescriptionItemRepository
    {
        Task<PrescriptionItem?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task AddAsync(PrescriptionItem item, CancellationToken ct = default);
        Task DeleteAsync(PrescriptionItem item, CancellationToken ct = default);
        Task<bool> MedicineExistsAsync(Guid medicineId, CancellationToken ct = default);
        Task<bool> PrescriptionExistsAsync(Guid prescriptionId, CancellationToken ct = default);
    }
}
