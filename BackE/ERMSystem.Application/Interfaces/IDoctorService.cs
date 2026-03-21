using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ERMSystem.Application.DTOs;
using ERMSystem.Application.DTOs.Common;

namespace ERMSystem.Application.Interfaces
{
    public interface IDoctorService
    {
        Task<PaginatedResult<DoctorDto>> GetAllDoctorsAsync(PaginationRequest request, CancellationToken ct = default);
        Task<DoctorDto?> GetDoctorByIdAsync(Guid id, CancellationToken ct = default);
        Task<DoctorDto> CreateDoctorAsync(CreateDoctorDto createDoctorDto, CancellationToken ct = default);
        Task UpdateDoctorAsync(Guid id, UpdateDoctorDto updateDoctorDto, CancellationToken ct = default);
        Task DeleteDoctorAsync(Guid id, CancellationToken ct = default);
    }
}
