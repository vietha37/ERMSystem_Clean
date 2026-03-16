using System;
using System.Linq;
using System.Threading.Tasks;
using ERMSystem.Application.DTOs;
using ERMSystem.Application.Interfaces;
using ERMSystem.Domain.Entities;

namespace ERMSystem.Application.Services
{
    public class PrescriptionItemService : IPrescriptionItemService
    {
        private readonly IPrescriptionItemRepository _itemRepository;
        private readonly IPrescriptionRepository _prescriptionRepository;

        public PrescriptionItemService(
            IPrescriptionItemRepository itemRepository,
            IPrescriptionRepository prescriptionRepository)
        {
            _itemRepository = itemRepository;
            _prescriptionRepository = prescriptionRepository;
        }

        public async Task<PrescriptionDto> AddItemToPrescriptionAsync(Guid prescriptionId, AddPrescriptionItemDto addPrescriptionItemDto)
        {
            var prescriptionExists = await _itemRepository.PrescriptionExistsAsync(prescriptionId);
            if (!prescriptionExists)
                throw new KeyNotFoundException($"Prescription with ID {prescriptionId} not found.");

            var medicineExists = await _itemRepository.MedicineExistsAsync(addPrescriptionItemDto.MedicineId);
            if (!medicineExists)
                throw new KeyNotFoundException($"Medicine with ID {addPrescriptionItemDto.MedicineId} not found.");

            var item = new PrescriptionItem
            {
                Id = Guid.NewGuid(),
                PrescriptionId = prescriptionId,
                MedicineId = addPrescriptionItemDto.MedicineId,
                Dosage = addPrescriptionItemDto.Dosage,
                Duration = addPrescriptionItemDto.Duration
            };

            await _itemRepository.AddAsync(item);

            // Reload the full prescription with updated items
            var prescription = await _prescriptionRepository.GetByIdAsync(prescriptionId);
            return PrescriptionService.MapToDto(prescription!);
        }

        public async Task<PrescriptionDto> RemoveItemFromPrescriptionAsync(Guid prescriptionId, Guid itemId)
        {
            var item = await _itemRepository.GetByIdAsync(itemId);
            if (item == null || item.PrescriptionId != prescriptionId)
                throw new KeyNotFoundException(
                    $"PrescriptionItem {itemId} not found on Prescription {prescriptionId}.");

            _itemRepository.Delete(item);

            // Reload the full prescription with updated items
            var prescription = await _prescriptionRepository.GetByIdAsync(prescriptionId);
            return PrescriptionService.MapToDto(prescription!);
        }
    }
}
