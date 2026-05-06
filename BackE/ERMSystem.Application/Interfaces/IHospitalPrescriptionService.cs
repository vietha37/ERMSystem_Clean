using System;
using System.Threading;
using System.Threading.Tasks;
using ERMSystem.Application.DTOs;
using ERMSystem.Application.DTOs.Common;

namespace ERMSystem.Application.Interfaces;

public interface IHospitalPrescriptionService
{
    Task<PaginatedResult<HospitalPrescriptionSummaryDto>> GetWorklistAsync(
        HospitalPrescriptionWorklistRequestDto request,
        CancellationToken ct = default);

    Task<HospitalPrescriptionDetailDto?> GetByIdAsync(Guid prescriptionId, CancellationToken ct = default);
    Task<HospitalPrescriptionEligibleEncounterDto[]> GetEligibleEncountersAsync(CancellationToken ct = default);
    Task<HospitalMedicineCatalogDto[]> GetMedicineCatalogAsync(CancellationToken ct = default);
    Task<HospitalPrescriptionDetailDto> CreateAsync(
        CreateHospitalPrescriptionDto request,
        Guid? actorUserId,
        CancellationToken ct = default);
    Task DeleteAsync(Guid prescriptionId, CancellationToken ct = default);
}
