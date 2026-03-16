using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ERMSystem.Application.DTOs;

namespace ERMSystem.Application.Interfaces
{
    public interface IAppointmentService
    {
        Task<IEnumerable<AppointmentDto>> GetAllAppointmentsAsync();
        Task<AppointmentDto?> GetAppointmentByIdAsync(Guid id);
        Task<AppointmentDto> CreateAppointmentAsync(CreateAppointmentDto createAppointmentDto);
        Task UpdateAppointmentAsync(UpdateAppointmentDto updateAppointmentDto);
        Task DeleteAppointmentAsync(Guid id);
    }
}
