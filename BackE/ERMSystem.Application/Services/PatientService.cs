using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ERMSystem.Application.DTOs;
using ERMSystem.Application.DTOs.Common;
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

        public async Task<PaginatedResult<PatientDto>> GetAllPatientsAsync(PaginationRequest request, CancellationToken ct = default)
        {
            var (items, totalCount) = await _patientRepository.GetPagedAsync(
                request.PageNumber,
                request.PageSize,
                request.TextSearch,
                ct);
            return new PaginatedResult<PatientDto>(items.Select(MapToDto), totalCount, request.PageNumber, request.PageSize);
        }

        public async Task<PatientDto?> GetPatientByIdAsync(Guid id, CancellationToken ct = default)
        {
            var patient = await _patientRepository.GetByIdAsync(id, ct);
            return patient == null ? null : MapToDto(patient);
        }

        public async Task<PatientDto?> GetPatientByAppUserIdAsync(Guid appUserId, CancellationToken ct = default)
        {
            var patient = await _patientRepository.GetByAppUserIdAsync(appUserId, ct);
            return patient == null ? null : MapToDto(patient);
        }

        public async Task<PatientDto> CreatePatientAsync(CreatePatientDto dto, CancellationToken ct = default)
        {
            var patient = new Patient
            {
                Id = Guid.NewGuid(),
                AppUserId = null,
                FullName = dto.FullName,
                DateOfBirth = dto.DateOfBirth,
                Gender = dto.Gender,
                Phone = dto.Phone,
                Address = dto.Address,
                EmergencyContactName = dto.EmergencyContactName,
                EmergencyContactPhone = dto.EmergencyContactPhone,
                EmergencyContactRelationship = dto.EmergencyContactRelationship,
                CreatedAt = DateTime.UtcNow
            };

            await _patientRepository.AddAsync(patient, ct);
            return MapToDto(patient);
        }

        public async Task UpdatePatientAsync(Guid id, UpdatePatientDto dto, CancellationToken ct = default)
        {
            var patient = await _patientRepository.GetByIdAsync(id, ct);
            if (patient == null)
                throw new KeyNotFoundException($"Patient with ID {id} not found.");

            patient.FullName = dto.FullName;
            patient.DateOfBirth = dto.DateOfBirth;
            patient.Gender = dto.Gender;
            patient.Phone = dto.Phone;
            patient.Address = dto.Address;
            patient.EmergencyContactName = dto.EmergencyContactName;
            patient.EmergencyContactPhone = dto.EmergencyContactPhone;
            patient.EmergencyContactRelationship = dto.EmergencyContactRelationship;

            await _patientRepository.UpdateAsync(patient, ct);
        }

        public async Task DeletePatientAsync(Guid id, CancellationToken ct = default)
        {
            var patient = await _patientRepository.GetByIdAsync(id, ct);
            if (patient == null)
                throw new KeyNotFoundException($"Patient with ID {id} not found.");

            await _patientRepository.DeleteAsync(patient, ct);
        }

        private static PatientDto MapToDto(Patient p) => new PatientDto
        {
            Id = p.Id,
            FullName = p.FullName,
            DateOfBirth = p.DateOfBirth,
            Gender = p.Gender,
            Phone = p.Phone,
            Address = p.Address,
            EmergencyContactName = p.EmergencyContactName,
            EmergencyContactPhone = p.EmergencyContactPhone,
            EmergencyContactRelationship = p.EmergencyContactRelationship,
            CreatedAt = p.CreatedAt
        };
    }
}
