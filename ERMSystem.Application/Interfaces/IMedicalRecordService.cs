using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ERMSystem.Application.DTOs;

namespace ERMSystem.Application.Interfaces
{
    public interface IMedicalRecordService
    {
        Task<IEnumerable<MedicalRecordDto>> GetAllMedicalRecordsAsync();
        Task<MedicalRecordDto?> GetMedicalRecordByIdAsync(Guid id);
        Task<MedicalRecordDto?> GetMedicalRecordByAppointmentIdAsync(Guid appointmentId);
        Task<MedicalRecordDto> CreateMedicalRecordAsync(CreateMedicalRecordDto createMedicalRecordDto);
        Task UpdateMedicalRecordAsync(UpdateMedicalRecordDto updateMedicalRecordDto);
        Task DeleteMedicalRecordAsync(Guid id);
    }
}
