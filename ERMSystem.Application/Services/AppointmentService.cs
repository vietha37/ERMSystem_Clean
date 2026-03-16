using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERMSystem.Application.DTOs;
using ERMSystem.Application.Interfaces;
using ERMSystem.Domain.Entities;

namespace ERMSystem.Application.Services
{
    public class AppointmentService : IAppointmentService
    {
        private static readonly HashSet<string> ValidStatuses =
            new HashSet<string>(StringComparer.Ordinal) { "Pending", "Completed", "Cancelled" };

        private readonly IAppointmentRepository _appointmentRepository;

        public AppointmentService(IAppointmentRepository appointmentRepository)
        {
            _appointmentRepository = appointmentRepository;
        }

        public async Task<IEnumerable<AppointmentDto>> GetAllAppointmentsAsync()
        {
            var appointments = await _appointmentRepository.GetAllAsync();
            return appointments.Select(MapToDto);
        }

        public async Task<AppointmentDto?> GetAppointmentByIdAsync(Guid id)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(id);
            return appointment == null ? null : MapToDto(appointment);
        }

        public async Task<AppointmentDto> CreateAppointmentAsync(CreateAppointmentDto createAppointmentDto)
        {
            if (!ValidStatuses.Contains(createAppointmentDto.Status))
                throw new ArgumentException(
                    $"Invalid status '{createAppointmentDto.Status}'. Must be Pending, Completed, or Cancelled.");

            var patientExists = await _appointmentRepository.PatientExistsAsync(createAppointmentDto.PatientId);
            if (!patientExists)
                throw new KeyNotFoundException(
                    $"Patient with ID {createAppointmentDto.PatientId} not found.");

            var doctorExists = await _appointmentRepository.DoctorExistsAsync(createAppointmentDto.DoctorId);
            if (!doctorExists)
                throw new KeyNotFoundException(
                    $"Doctor with ID {createAppointmentDto.DoctorId} not found.");

            var appointment = new Appointment
            {
                Id = Guid.NewGuid(),
                PatientId = createAppointmentDto.PatientId,
                DoctorId = createAppointmentDto.DoctorId,
                AppointmentDate = createAppointmentDto.AppointmentDate,
                Status = createAppointmentDto.Status
            };

            await _appointmentRepository.AddAsync(appointment);

            return MapToDto(appointment);
        }

        public async Task UpdateAppointmentAsync(UpdateAppointmentDto updateAppointmentDto)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(updateAppointmentDto.Id);
            if (appointment == null)
                throw new KeyNotFoundException(
                    $"Appointment with ID {updateAppointmentDto.Id} not found.");

            if (!ValidStatuses.Contains(updateAppointmentDto.Status))
                throw new ArgumentException(
                    $"Invalid status '{updateAppointmentDto.Status}'. Must be Pending, Completed, or Cancelled.");

            var patientExists = await _appointmentRepository.PatientExistsAsync(updateAppointmentDto.PatientId);
            if (!patientExists)
                throw new KeyNotFoundException(
                    $"Patient with ID {updateAppointmentDto.PatientId} not found.");

            var doctorExists = await _appointmentRepository.DoctorExistsAsync(updateAppointmentDto.DoctorId);
            if (!doctorExists)
                throw new KeyNotFoundException(
                    $"Doctor with ID {updateAppointmentDto.DoctorId} not found.");

            appointment.PatientId = updateAppointmentDto.PatientId;
            appointment.DoctorId = updateAppointmentDto.DoctorId;
            appointment.AppointmentDate = updateAppointmentDto.AppointmentDate;
            appointment.Status = updateAppointmentDto.Status;

            _appointmentRepository.Update(appointment);
        }

        public async Task DeleteAppointmentAsync(Guid id)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(id);
            if (appointment == null)
                throw new KeyNotFoundException($"Appointment with ID {id} not found.");

            _appointmentRepository.Delete(appointment);
        }

        private static AppointmentDto MapToDto(Appointment appointment) => new AppointmentDto
        {
            Id = appointment.Id,
            PatientId = appointment.PatientId,
            DoctorId = appointment.DoctorId,
            AppointmentDate = appointment.AppointmentDate,
            Status = appointment.Status
        };
    }
}
