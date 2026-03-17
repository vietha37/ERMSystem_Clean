using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ERMSystem.Application.DTOs;
using ERMSystem.Application.DTOs.Common;

namespace ERMSystem.Application.Interfaces
{
    public interface IMedicineService
    {
        Task<PaginatedResult<MedicineDto>> GetAllMedicinesAsync(PaginationRequest request, CancellationToken ct = default);
        Task<MedicineDto?> GetMedicineByIdAsync(Guid id, CancellationToken ct = default);
        Task<MedicineDto> CreateMedicineAsync(CreateMedicineDto createMedicineDto, CancellationToken ct = default);
        Task UpdateMedicineAsync(Guid id, UpdateMedicineDto updateMedicineDto, CancellationToken ct = default);
        Task DeleteMedicineAsync(Guid id, CancellationToken ct = default);
    }
}
