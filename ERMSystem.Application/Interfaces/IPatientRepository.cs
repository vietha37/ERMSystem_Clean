using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ERMSystem.Domain.Entities;

namespace ERMSystem.Application.Interfaces
{
    public interface IPatientRepository
    {
        Task<List<Patient>> GetAllAsync();
        Task<Patient?> GetByIdAsync(Guid id);
        Task AddAsync(Patient patient);
        void Update(Patient patient);
        void Delete(Patient patient);
    }
}
