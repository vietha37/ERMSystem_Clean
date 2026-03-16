using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ERMSystem.Domain.Entities;

namespace ERMSystem.Application.Interfaces
{
    public interface IMedicalRecordRepository
    {
        Task<List<MedicalRecord>> GetAllAsync();
        Task<MedicalRecord?> GetByIdAsync(Guid id);
        Task<MedicalRecord?> GetByAppointmentIdAsync(Guid appointmentId);
        Task AddAsync(MedicalRecord record);
        void Update(MedicalRecord record);
        void Delete(MedicalRecord record);
        Task<bool> AppointmentExistsAsync(Guid appointmentId);
        Task<bool> MedicalRecordExistsForAppointmentAsync(Guid appointmentId);
    }
}
