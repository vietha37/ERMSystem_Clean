using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ERMSystem.Application.DTOs;
using ERMSystem.Application.DTOs.Common;

namespace ERMSystem.Application.Interfaces
{
    public interface IPrescriptionService
    {
        Task<PaginatedResult<PrescriptionDto>> GetAllPrescriptionsAsync(PaginationRequest request, CancellationToken ct = default);
        Task<PrescriptionDto?> GetPrescriptionByIdAsync(Guid id, CancellationToken ct = default);
        Task<PrescriptionDto?> GetPrescriptionByMedicalRecordIdAsync(Guid medicalRecordId, CancellationToken ct = default);
        Task<PrescriptionDto> CreatePrescriptionAsync(CreatePrescriptionDto createPrescriptionDto, CancellationToken ct = default);
        Task DeletePrescriptionAsync(Guid id, CancellationToken ct = default);
    }
}
