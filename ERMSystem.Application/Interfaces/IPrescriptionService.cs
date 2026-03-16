using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ERMSystem.Application.DTOs;

namespace ERMSystem.Application.Interfaces
{
    public interface IPrescriptionService
    {
        Task<IEnumerable<PrescriptionDto>> GetAllPrescriptionsAsync();
        Task<PrescriptionDto?> GetPrescriptionByIdAsync(Guid id);
        Task<PrescriptionDto?> GetPrescriptionByMedicalRecordIdAsync(Guid medicalRecordId);
        Task<PrescriptionDto> CreatePrescriptionAsync(CreatePrescriptionDto createPrescriptionDto);
        Task DeletePrescriptionAsync(Guid id);
    }
}
