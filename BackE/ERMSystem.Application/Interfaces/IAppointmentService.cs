using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ERMSystem.Application.DTOs;
using ERMSystem.Application.DTOs.Common;

namespace ERMSystem.Application.Interfaces
{
    public interface IAppointmentService
    {
        Task<PaginatedResult<AppointmentDto>> GetAllAppointmentsAsync(PaginationRequest request, CancellationToken ct = default);
        Task<AppointmentDto?> GetAppointmentByIdAsync(Guid id, CancellationToken ct = default);
        Task<AppointmentDto> CreateAppointmentAsync(CreateAppointmentDto createAppointmentDto, CancellationToken ct = default);
        Task UpdateAppointmentAsync(Guid id, UpdateAppointmentDto updateAppointmentDto, CancellationToken ct = default);
        Task DeleteAppointmentAsync(Guid id, CancellationToken ct = default);
    }
}
