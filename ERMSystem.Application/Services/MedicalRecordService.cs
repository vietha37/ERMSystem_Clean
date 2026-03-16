using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERMSystem.Application.DTOs;
using ERMSystem.Application.Interfaces;
using ERMSystem.Domain.Entities;

namespace ERMSystem.Application.Services
{
    public class MedicalRecordService : IMedicalRecordService
    {
        private readonly IMedicalRecordRepository _medicalRecordRepository;

        public MedicalRecordService(IMedicalRecordRepository medicalRecordRepository)
        {
            _medicalRecordRepository = medicalRecordRepository;
        }

        public async Task<IEnumerable<MedicalRecordDto>> GetAllMedicalRecordsAsync()
        {
            var records = await _medicalRecordRepository.GetAllAsync();
            return records.Select(MapToDto);
        }

        public async Task<MedicalRecordDto?> GetMedicalRecordByIdAsync(Guid id)
        {
            var record = await _medicalRecordRepository.GetByIdAsync(id);
            return record == null ? null : MapToDto(record);
        }

        public async Task<MedicalRecordDto?> GetMedicalRecordByAppointmentIdAsync(Guid appointmentId)
        {
            var record = await _medicalRecordRepository.GetByAppointmentIdAsync(appointmentId);
            return record == null ? null : MapToDto(record);
        }

        public async Task<MedicalRecordDto> CreateMedicalRecordAsync(CreateMedicalRecordDto createMedicalRecordDto)
        {
            var appointmentExists = await _medicalRecordRepository.AppointmentExistsAsync(createMedicalRecordDto.AppointmentId);
            if (!appointmentExists)
                throw new KeyNotFoundException(
                    $"Appointment with ID {createMedicalRecordDto.AppointmentId} not found.");

            var recordAlreadyExists = await _medicalRecordRepository
                .MedicalRecordExistsForAppointmentAsync(createMedicalRecordDto.AppointmentId);
            if (recordAlreadyExists)
                throw new InvalidOperationException(
                    $"A MedicalRecord already exists for Appointment {createMedicalRecordDto.AppointmentId}.");

            var record = new MedicalRecord
            {
                Id = Guid.NewGuid(),
                AppointmentId = createMedicalRecordDto.AppointmentId,
                Symptoms = createMedicalRecordDto.Symptoms,
                Diagnosis = createMedicalRecordDto.Diagnosis,
                Notes = createMedicalRecordDto.Notes
            };

            await _medicalRecordRepository.AddAsync(record);

            return MapToDto(record);
        }

        public async Task UpdateMedicalRecordAsync(UpdateMedicalRecordDto updateMedicalRecordDto)
        {
            var record = await _medicalRecordRepository.GetByIdAsync(updateMedicalRecordDto.Id);
            if (record == null)
                throw new KeyNotFoundException(
                    $"MedicalRecord with ID {updateMedicalRecordDto.Id} not found.");

            record.Symptoms = updateMedicalRecordDto.Symptoms;
            record.Diagnosis = updateMedicalRecordDto.Diagnosis;
            record.Notes = updateMedicalRecordDto.Notes;

            _medicalRecordRepository.Update(record);
        }

        public async Task DeleteMedicalRecordAsync(Guid id)
        {
            var record = await _medicalRecordRepository.GetByIdAsync(id);
            if (record == null)
                throw new KeyNotFoundException($"MedicalRecord with ID {id} not found.");

            _medicalRecordRepository.Delete(record);
        }

        private static MedicalRecordDto MapToDto(MedicalRecord record) => new MedicalRecordDto
        {
            Id = record.Id,
            AppointmentId = record.AppointmentId,
            Symptoms = record.Symptoms,
            Diagnosis = record.Diagnosis,
            Notes = record.Notes
        };
    }
}
