using System;
using System.Threading.Tasks;
using ERMSystem.Application.DTOs;

namespace ERMSystem.Application.Interfaces
{
    public interface IPrescriptionItemService
    {
        Task<PrescriptionDto> AddItemToPrescriptionAsync(Guid prescriptionId, AddPrescriptionItemDto addPrescriptionItemDto);
        Task<PrescriptionDto> RemoveItemFromPrescriptionAsync(Guid prescriptionId, Guid itemId);
    }
}
