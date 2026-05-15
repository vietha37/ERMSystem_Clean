using System;
using System.Threading;
using System.Threading.Tasks;
using ERMSystem.Application.DTOs;
using ERMSystem.Application.DTOs.Common;

namespace ERMSystem.Application.Interfaces;

public interface IHospitalEncounterService
{
    Task<PaginatedResult<HospitalEncounterSummaryDto>> GetWorklistAsync(
        HospitalEncounterWorklistRequestDto request,
        CancellationToken ct = default);

    Task<HospitalEncounterDetailDto?> GetByIdAsync(Guid encounterId, CancellationToken ct = default);

    Task<HospitalEncounterEligibleAppointmentDto[]> GetEligibleAppointmentsAsync(CancellationToken ct = default);

    Task<HospitalEncounterDetailDto> CreateAsync(
        CreateHospitalEncounterDto request,
        Guid? actorUserId,
        string? actorUsername,
        CancellationToken ct = default);

    Task<HospitalEncounterDetailDto?> UpdateAsync(
        Guid encounterId,
        UpdateHospitalEncounterDto request,
        Guid? actorUserId,
        string? actorUsername,
        CancellationToken ct = default);

    Task<HospitalEncounterDetailDto?> AddAttachmentAsync(
        Guid encounterId,
        AddHospitalEncounterAttachmentDto request,
        Guid? actorUserId,
        string? actorUsername,
        CancellationToken ct = default);
}
