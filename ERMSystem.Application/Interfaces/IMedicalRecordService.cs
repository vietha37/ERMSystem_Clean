using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ERMSystem.Application.DTOs;
using ERMSystem.Application.DTOs.Common;

namespace ERMSystem.Application.Interfaces
{
    public interface IMedicalRecordService
    {
        Task<PaginatedResult<MedicalRecordDto>> GetAllMedicalRecordsAsync(PaginationRequest request, CancellationToken ct = default);
        Task<MedicalRecordDto?> GetMedicalRecordByIdAsync(Guid id, CancellationToken ct = default);
        Task<MedicalRecordDto?> GetMedicalRecordByAppointmentIdAsync(Guid appointmentId, CancellationToken ct = default);
        Task<MedicalRecordDto> CreateMedicalRecordAsync(CreateMedicalRecordDto createMedicalRecordDto, CancellationToken ct = default);
        Task UpdateMedicalRecordAsync(Guid id, UpdateMedicalRecordDto updateMedicalRecordDto, CancellationToken ct = default);
        Task DeleteMedicalRecordAsync(Guid id, CancellationToken ct = default);
    }
}
