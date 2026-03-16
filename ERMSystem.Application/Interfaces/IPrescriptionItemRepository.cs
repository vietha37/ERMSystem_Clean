using System;
using System.Threading.Tasks;
using ERMSystem.Domain.Entities;

namespace ERMSystem.Application.Interfaces
{
    public interface IPrescriptionItemRepository
    {
        Task<PrescriptionItem?> GetByIdAsync(Guid id);
        Task AddAsync(PrescriptionItem item);
        void Delete(PrescriptionItem item);
        Task<bool> MedicineExistsAsync(Guid medicineId);
        Task<bool> PrescriptionExistsAsync(Guid prescriptionId);
    }
}
