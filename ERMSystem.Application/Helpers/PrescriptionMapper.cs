using System.Linq;
using ERMSystem.Application.DTOs;
using ERMSystem.Domain.Entities;

namespace ERMSystem.Application.Helpers
{
    /// <summary>
    /// Shared mapper for Prescription → PrescriptionDto conversions.
    /// Used by both PrescriptionService and PrescriptionItemService.
    /// </summary>
    public static class PrescriptionMapper
    {
        public static PrescriptionDto ToDto(Prescription prescription) => new PrescriptionDto
        {
            Id = prescription.Id,
            MedicalRecordId = prescription.MedicalRecordId,
            CreatedAt = prescription.CreatedAt,
            Items = prescription.PrescriptionItems.Select(i => new PrescriptionItemDto
            {
                Id = i.Id,
                PrescriptionId = i.PrescriptionId,
                MedicineId = i.MedicineId,
                Dosage = i.Dosage,
                Duration = i.Duration
            }).ToList()
        };
    }
}
