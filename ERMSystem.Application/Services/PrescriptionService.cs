using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERMSystem.Application.DTOs;
using ERMSystem.Application.Interfaces;
using ERMSystem.Domain.Entities;

namespace ERMSystem.Application.Services
{
    public class PrescriptionService : IPrescriptionService
    {
        private readonly IPrescriptionRepository _prescriptionRepository;

        public PrescriptionService(IPrescriptionRepository prescriptionRepository)
        {
            _prescriptionRepository = prescriptionRepository;
        }

        public async Task<IEnumerable<PrescriptionDto>> GetAllPrescriptionsAsync()
        {
            var prescriptions = await _prescriptionRepository.GetAllAsync();
            return prescriptions.Select(MapToDto);
        }

        public async Task<PrescriptionDto?> GetPrescriptionByIdAsync(Guid id)
        {
            var prescription = await _prescriptionRepository.GetByIdAsync(id);
            return prescription == null ? null : MapToDto(prescription);
        }

        public async Task<PrescriptionDto?> GetPrescriptionByMedicalRecordIdAsync(Guid medicalRecordId)
        {
            var prescription = await _prescriptionRepository.GetByMedicalRecordIdAsync(medicalRecordId);
            return prescription == null ? null : MapToDto(prescription);
        }

        public async Task<PrescriptionDto> CreatePrescriptionAsync(CreatePrescriptionDto createPrescriptionDto)
        {
            var medicalRecordExists = await _prescriptionRepository.MedicalRecordExistsAsync(createPrescriptionDto.MedicalRecordId);
            if (!medicalRecordExists)
                throw new KeyNotFoundException(
                    $"MedicalRecord with ID {createPrescriptionDto.MedicalRecordId} not found.");

            var prescriptionAlreadyExists = await _prescriptionRepository
                .PrescriptionExistsForMedicalRecordAsync(createPrescriptionDto.MedicalRecordId);
            if (prescriptionAlreadyExists)
                throw new InvalidOperationException(
                    $"A Prescription already exists for MedicalRecord {createPrescriptionDto.MedicalRecordId}.");

            var prescription = new Prescription
            {
                Id = Guid.NewGuid(),
                MedicalRecordId = createPrescriptionDto.MedicalRecordId,
                CreatedAt = DateTime.UtcNow
            };

            await _prescriptionRepository.AddAsync(prescription);

            return MapToDto(prescription);
        }

        public async Task DeletePrescriptionAsync(Guid id)
        {
            var prescription = await _prescriptionRepository.GetByIdAsync(id);
            if (prescription == null)
                throw new KeyNotFoundException($"Prescription with ID {id} not found.");

            _prescriptionRepository.Delete(prescription);
        }

        public static PrescriptionDto MapToDto(Prescription prescription) => new PrescriptionDto
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
