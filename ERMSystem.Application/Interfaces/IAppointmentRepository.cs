using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ERMSystem.Domain.Entities;

namespace ERMSystem.Application.Interfaces
{
    public interface IAppointmentRepository
    {
        Task<List<Appointment>> GetAllAsync();
        Task<Appointment?> GetByIdAsync(Guid id);
        Task AddAsync(Appointment appointment);
        void Update(Appointment appointment);
        void Delete(Appointment appointment);
        Task<bool> PatientExistsAsync(Guid patientId);
        Task<bool> DoctorExistsAsync(Guid doctorId);
    }
}
