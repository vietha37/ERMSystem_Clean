using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERMSystem.Application.DTOs;
using ERMSystem.Application.Interfaces;
using ERMSystem.Domain.Entities;

namespace ERMSystem.Application.Services
{
    public class PatientService : IPatientService
    {
        private readonly IPatientRepository _patientRepository;

        public PatientService(IPatientRepository patientRepository)
        {
            _patientRepository = patientRepository;
        }

        public async Task<IEnumerable<PatientDto>> GetAllPatientsAsync()
        {
            var patients = await _patientRepository.GetAllAsync();
            return patients.Select(p => new PatientDto
            {
                Id = p.Id,
                FullName = p.FullName,
                DateOfBirth = p.DateOfBirth,
                Gender = p.Gender,
                Phone = p.Phone,
                Address = p.Address,
                CreatedAt = p.CreatedAt
            });
        }

        public async Task<PatientDto?> GetPatientByIdAsync(Guid id)
        {
            var patient = await _patientRepository.GetByIdAsync(id);
            if (patient == null)
                return null;

            return new PatientDto
            {
                Id = patient.Id,
                FullName = patient.FullName,
                DateOfBirth = patient.DateOfBirth,
                Gender = patient.Gender,
                Phone = patient.Phone,
                Address = patient.Address,
                CreatedAt = patient.CreatedAt
            };
        }

        public async Task<PatientDto> CreatePatientAsync(CreatePatientDto createPatientDto)
        {
            var patient = new Patient
            {
                Id = Guid.NewGuid(),
                FullName = createPatientDto.FullName,
                DateOfBirth = createPatientDto.DateOfBirth,
                Gender = createPatientDto.Gender,
                Phone = createPatientDto.Phone,
                Address = createPatientDto.Address,
                CreatedAt = DateTime.UtcNow
            };

            await _patientRepository.AddAsync(patient);

            return new PatientDto
            {
                Id = patient.Id,
                FullName = patient.FullName,
                DateOfBirth = patient.DateOfBirth,
                Gender = patient.Gender,
                Phone = patient.Phone,
                Address = patient.Address,
                CreatedAt = patient.CreatedAt
            };
        }

        public async Task UpdatePatientAsync(UpdatePatientDto updatePatientDto)
        {
            var patient = await _patientRepository.GetByIdAsync(updatePatientDto.Id);
            if (patient == null)
                throw new KeyNotFoundException($"Patient with ID {updatePatientDto.Id} not found.");

            patient.FullName = updatePatientDto.FullName;
            patient.DateOfBirth = updatePatientDto.DateOfBirth;
            patient.Gender = updatePatientDto.Gender;
            patient.Phone = updatePatientDto.Phone;
            patient.Address = updatePatientDto.Address;

            _patientRepository.Update(patient);
        }

        public async Task DeletePatientAsync(Guid id)
        {
            var patient = await _patientRepository.GetByIdAsync(id);
            if (patient == null)
                throw new KeyNotFoundException($"Patient with ID {id} not found.");

            _patientRepository.Delete(patient);
        }
    }
}
