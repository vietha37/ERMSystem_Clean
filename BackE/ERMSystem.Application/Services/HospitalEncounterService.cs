using System;
using System.Linq;
using System.Text.Json;
using ERMSystem.Application.DTOs;
using ERMSystem.Application.DTOs.Common;
using ERMSystem.Application.Interfaces;

namespace ERMSystem.Application.Services;

public class HospitalEncounterService : IHospitalEncounterService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly string[] AllowedStatuses = ["InProgress", "Finalized"];

    private readonly IHospitalEncounterRepository _hospitalEncounterRepository;
    private readonly IHospitalIdentityBridgeService _hospitalIdentityBridgeService;

    public HospitalEncounterService(
        IHospitalEncounterRepository hospitalEncounterRepository,
        IHospitalIdentityBridgeService hospitalIdentityBridgeService)
    {
        _hospitalEncounterRepository = hospitalEncounterRepository;
        _hospitalIdentityBridgeService = hospitalIdentityBridgeService;
    }

    public Task<PaginatedResult<HospitalEncounterSummaryDto>> GetWorklistAsync(
        HospitalEncounterWorklistRequestDto request,
        CancellationToken ct = default)
        => _hospitalEncounterRepository.GetWorklistAsync(request, ct);

    public async Task<HospitalEncounterDetailDto?> GetByIdAsync(Guid encounterId, CancellationToken ct = default)
    {
        var encounter = await _hospitalEncounterRepository.GetEncounterAggregateAsync(encounterId, ct);
        return encounter == null ? null : MapDetail(encounter);
    }

    public Task<HospitalEncounterEligibleAppointmentDto[]> GetEligibleAppointmentsAsync(CancellationToken ct = default)
        => _hospitalEncounterRepository.GetEligibleAppointmentsAsync(ct);

    public async Task<HospitalEncounterDetailDto> CreateAsync(
        CreateHospitalEncounterDto request,
        Guid? actorUserId,
        string? actorUsername,
        CancellationToken ct = default)
    {
        actorUserId = await ResolveHospitalActorUserIdAsync(actorUserId, actorUsername, ct);
        var normalizedStatus = NormalizeStatus(request.EncounterStatus);
        var appointment = await _hospitalEncounterRepository.GetAppointmentForEncounterAsync(request.AppointmentId, ct);
        if (appointment == null)
        {
            throw new KeyNotFoundException("Khong tim thay lich hen de mo encounter.");
        }

        if (appointment.ExistingEncounterId.HasValue)
        {
            throw new InvalidOperationException("Lich hen nay da co encounter.");
        }

        if (appointment.AppointmentStatus is not ("CheckedIn" or "Completed"))
        {
            throw new InvalidOperationException("Chi duoc mo encounter tu lich hen da check-in hoac da hoan thanh.");
        }

        var nowUtc = DateTime.UtcNow;
        var encounterId = Guid.NewGuid();

        await _hospitalEncounterRepository.AddEncounterAsync(new HospitalEncounterCreateCommand
        {
            EncounterId = encounterId,
            EncounterNumber = GenerateEncounterNumber(nowUtc),
            PatientId = appointment.PatientId,
            AppointmentId = appointment.AppointmentId,
            DoctorProfileId = appointment.DoctorProfileId,
            ClinicId = appointment.ClinicId,
            EncounterType = "Outpatient",
            EncounterStatus = normalizedStatus,
            StartedAtUtc = appointment.AppointmentStartUtc,
            EndedAtUtc = normalizedStatus == "Finalized" ? nowUtc : null,
            Summary = NormalizeText(request.Summary),
            CreatedAtUtc = nowUtc,
            UpdatedAtUtc = nowUtc
        }, ct);

        await _hospitalEncounterRepository.AddVitalSignAsync(new HospitalEncounterVitalSignCreateCommand
        {
            VitalSignId = Guid.NewGuid(),
            EncounterId = encounterId,
            HeightCm = request.HeightCm,
            WeightKg = request.WeightKg,
            TemperatureC = request.TemperatureC,
            PulseRate = request.PulseRate,
            RespiratoryRate = request.RespiratoryRate,
            SystolicBp = request.SystolicBp,
            DiastolicBp = request.DiastolicBp,
            OxygenSaturation = request.OxygenSaturation,
            RecordedAtUtc = nowUtc,
            RecordedByUserId = actorUserId
        }, ct);

        await _hospitalEncounterRepository.AddDiagnosisAsync(new HospitalEncounterDiagnosisCreateCommand
        {
            DiagnosisId = Guid.NewGuid(),
            EncounterId = encounterId,
            DiagnosisType = NormalizeDiagnosisType(request.DiagnosisType),
            DiagnosisCode = NormalizeText(request.DiagnosisCode),
            DiagnosisName = NormalizeRequiredText(request.DiagnosisName, "Chan doan"),
            IsPrimary = true,
            NotedAtUtc = nowUtc
        }, ct);

        await _hospitalEncounterRepository.AddClinicalNoteAsync(new HospitalEncounterClinicalNoteCreateCommand
        {
            ClinicalNoteId = Guid.NewGuid(),
            EncounterId = encounterId,
            NoteType = "Consultation",
            Subjective = NormalizeText(request.Subjective),
            Objective = NormalizeText(request.Objective),
            Assessment = NormalizeText(request.Assessment),
            CarePlan = NormalizeText(request.CarePlan),
            AuthoredByUserId = actorUserId,
            AuthoredAtUtc = nowUtc,
            SignedAtUtc = normalizedStatus == "Finalized" ? nowUtc : null
        }, ct);

        if (normalizedStatus == "Finalized")
        {
            await QueueFinalizedEventAsync(encounterId, appointment, request.DiagnosisName, nowUtc, ct);
        }

        await _hospitalEncounterRepository.SaveChangesAsync(ct);

        var created = await _hospitalEncounterRepository.GetEncounterAggregateAsync(encounterId, ct)
            ?? throw new InvalidOperationException("Khong the tai lai encounter sau khi tao.");

        return MapDetail(created);
    }

    public async Task<HospitalEncounterDetailDto?> UpdateAsync(
        Guid encounterId,
        UpdateHospitalEncounterDto request,
        Guid? actorUserId,
        string? actorUsername,
        CancellationToken ct = default)
    {
        actorUserId = await ResolveHospitalActorUserIdAsync(actorUserId, actorUsername, ct);
        var existing = await _hospitalEncounterRepository.GetEncounterAggregateAsync(encounterId, ct);
        if (existing == null)
        {
            return null;
        }

        var normalizedStatus = NormalizeStatus(request.EncounterStatus);
        var nowUtc = DateTime.UtcNow;
        var finalizedNow = existing.EncounterStatus != "Finalized" && normalizedStatus == "Finalized";

        await _hospitalEncounterRepository.UpdateEncounterAsync(new HospitalEncounterUpdateCommand
        {
            EncounterId = encounterId,
            EncounterStatus = normalizedStatus,
            EndedAtUtc = normalizedStatus == "Finalized"
                ? (existing.EndedAtUtc ?? nowUtc)
                : null,
            Summary = NormalizeText(request.Summary),
            UpdatedAtUtc = nowUtc
        }, ct);

        if (existing.VitalSignId.HasValue)
        {
            await _hospitalEncounterRepository.UpdateVitalSignAsync(new HospitalEncounterVitalSignUpdateCommand
            {
                VitalSignId = existing.VitalSignId.Value,
                HeightCm = request.HeightCm,
                WeightKg = request.WeightKg,
                TemperatureC = request.TemperatureC,
                PulseRate = request.PulseRate,
                RespiratoryRate = request.RespiratoryRate,
                SystolicBp = request.SystolicBp,
                DiastolicBp = request.DiastolicBp,
                OxygenSaturation = request.OxygenSaturation,
                RecordedAtUtc = nowUtc,
                RecordedByUserId = actorUserId
            }, ct);
        }
        else
        {
            await _hospitalEncounterRepository.AddVitalSignAsync(new HospitalEncounterVitalSignCreateCommand
            {
                VitalSignId = Guid.NewGuid(),
                EncounterId = encounterId,
                HeightCm = request.HeightCm,
                WeightKg = request.WeightKg,
                TemperatureC = request.TemperatureC,
                PulseRate = request.PulseRate,
                RespiratoryRate = request.RespiratoryRate,
                SystolicBp = request.SystolicBp,
                DiastolicBp = request.DiastolicBp,
                OxygenSaturation = request.OxygenSaturation,
                RecordedAtUtc = nowUtc,
                RecordedByUserId = actorUserId
            }, ct);
        }

        if (existing.DiagnosisId.HasValue)
        {
            await _hospitalEncounterRepository.UpdateDiagnosisAsync(new HospitalEncounterDiagnosisUpdateCommand
            {
                DiagnosisId = existing.DiagnosisId.Value,
                DiagnosisType = NormalizeDiagnosisType(request.DiagnosisType),
                DiagnosisCode = NormalizeText(request.DiagnosisCode),
                DiagnosisName = NormalizeRequiredText(request.DiagnosisName, "Chan doan"),
                NotedAtUtc = nowUtc
            }, ct);
        }
        else
        {
            await _hospitalEncounterRepository.AddDiagnosisAsync(new HospitalEncounterDiagnosisCreateCommand
            {
                DiagnosisId = Guid.NewGuid(),
                EncounterId = encounterId,
                DiagnosisType = NormalizeDiagnosisType(request.DiagnosisType),
                DiagnosisCode = NormalizeText(request.DiagnosisCode),
                DiagnosisName = NormalizeRequiredText(request.DiagnosisName, "Chan doan"),
                IsPrimary = true,
                NotedAtUtc = nowUtc
            }, ct);
        }

        DateTime? signedAtUtc = normalizedStatus == "Finalized" ? nowUtc : null;

        if (existing.ClinicalNoteId.HasValue)
        {
            await _hospitalEncounterRepository.UpdateClinicalNoteAsync(new HospitalEncounterClinicalNoteUpdateCommand
            {
                ClinicalNoteId = existing.ClinicalNoteId.Value,
                Subjective = NormalizeText(request.Subjective),
                Objective = NormalizeText(request.Objective),
                Assessment = NormalizeText(request.Assessment),
                CarePlan = NormalizeText(request.CarePlan),
                AuthoredByUserId = actorUserId,
                AuthoredAtUtc = nowUtc,
                SignedAtUtc = signedAtUtc
            }, ct);
        }
        else
        {
            await _hospitalEncounterRepository.AddClinicalNoteAsync(new HospitalEncounterClinicalNoteCreateCommand
            {
                ClinicalNoteId = Guid.NewGuid(),
                EncounterId = encounterId,
                NoteType = "Consultation",
                Subjective = NormalizeText(request.Subjective),
                Objective = NormalizeText(request.Objective),
                Assessment = NormalizeText(request.Assessment),
                CarePlan = NormalizeText(request.CarePlan),
                AuthoredByUserId = actorUserId,
                AuthoredAtUtc = nowUtc,
                SignedAtUtc = signedAtUtc
            }, ct);
        }

        if (finalizedNow && existing.AppointmentId.HasValue)
        {
            var appointment = await _hospitalEncounterRepository.GetAppointmentForEncounterAsync(existing.AppointmentId.Value, ct)
                ?? throw new InvalidOperationException("Khong tai lai duoc lich hen cua encounter.");
            await QueueFinalizedEventAsync(encounterId, appointment, request.DiagnosisName, nowUtc, ct);
        }

        await _hospitalEncounterRepository.SaveChangesAsync(ct);

        var updated = await _hospitalEncounterRepository.GetEncounterAggregateAsync(encounterId, ct);
        return updated == null ? null : MapDetail(updated);
    }

    private async Task QueueFinalizedEventAsync(
        Guid encounterId,
        HospitalEncounterAppointmentSnapshot appointment,
        string diagnosisName,
        DateTime nowUtc,
        CancellationToken ct)
    {
        await _hospitalEncounterRepository.AddOutboxMessageAsync(new HospitalEncounterOutboxCreateCommand
        {
            OutboxMessageId = Guid.NewGuid(),
            AggregateType = "Encounter",
            AggregateId = encounterId,
            EventType = "MedicalRecordFinalized.v1",
            PayloadJson = JsonSerializer.Serialize(new
            {
                encounterId,
                appointmentId = appointment.AppointmentId,
                appointment.AppointmentNumber,
                appointment.PatientId,
                appointment.PatientName,
                appointment.MedicalRecordNumber,
                appointment.DoctorProfileId,
                appointment.DoctorName,
                appointment.SpecialtyName,
                appointment.ClinicName,
                diagnosisName = NormalizeRequiredText(diagnosisName, "Chan doan"),
                finalizedAtUtc = nowUtc
            }, JsonOptions),
            Status = "Pending",
            AvailableAtUtc = nowUtc
        }, ct);
    }

    private Task<Guid?> ResolveHospitalActorUserIdAsync(Guid? actorUserId, string? actorUsername, CancellationToken ct)
        => _hospitalIdentityBridgeService.ResolveHospitalUserIdAsync(actorUserId, actorUsername, ct);

    private static string NormalizeStatus(string? status)
    {
        var normalized = string.IsNullOrWhiteSpace(status) ? "InProgress" : status.Trim();
        if (!AllowedStatuses.Contains(normalized, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Trang thai encounter khong hop le.");
        }

        return AllowedStatuses.First(x => string.Equals(x, normalized, StringComparison.OrdinalIgnoreCase));
    }

    private static string NormalizeDiagnosisType(string? diagnosisType)
    {
        return string.IsNullOrWhiteSpace(diagnosisType) ? "Working" : diagnosisType.Trim();
    }

    private static string NormalizeRequiredText(string? value, string fieldName)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new InvalidOperationException($"{fieldName} la truong bat buoc.");
        }

        return normalized;
    }

    private static string? NormalizeText(string? value)
    {
        var normalized = value?.Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private static string GenerateEncounterNumber(DateTime nowUtc)
        => $"ENC-{nowUtc:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}";

    private static HospitalEncounterDetailDto MapDetail(HospitalEncounterAggregateSnapshot encounter)
    {
        return new HospitalEncounterDetailDto
        {
            EncounterId = encounter.EncounterId,
            EncounterNumber = encounter.EncounterNumber,
            AppointmentId = encounter.AppointmentId,
            AppointmentNumber = encounter.AppointmentNumber,
            PatientId = encounter.PatientId,
            PatientName = encounter.PatientName,
            MedicalRecordNumber = encounter.MedicalRecordNumber,
            DoctorProfileId = encounter.DoctorProfileId,
            DoctorName = encounter.DoctorName,
            SpecialtyName = encounter.SpecialtyName,
            ClinicName = encounter.ClinicName,
            AppointmentStartLocal = encounter.AppointmentStartUtc.HasValue
                ? ConvertUtcToClinicLocal(encounter.AppointmentStartUtc.Value)
                : null,
            EncounterStatus = encounter.EncounterStatus,
            PrimaryDiagnosisName = encounter.DiagnosisName,
            Summary = encounter.Summary,
            StartedAtLocal = ConvertUtcToClinicLocal(encounter.StartedAtUtc),
            EndedAtLocal = encounter.EndedAtUtc.HasValue ? ConvertUtcToClinicLocal(encounter.EndedAtUtc.Value) : null,
            UpdatedAtLocal = ConvertUtcToClinicLocal(encounter.UpdatedAtUtc),
            EncounterType = encounter.EncounterType,
            DiagnosisCode = encounter.DiagnosisCode,
            DiagnosisType = encounter.DiagnosisType,
            Subjective = encounter.Subjective,
            Objective = encounter.Objective,
            Assessment = encounter.Assessment,
            CarePlan = encounter.CarePlan,
            HeightCm = encounter.HeightCm,
            WeightKg = encounter.WeightKg,
            TemperatureC = encounter.TemperatureC,
            PulseRate = encounter.PulseRate,
            RespiratoryRate = encounter.RespiratoryRate,
            SystolicBp = encounter.SystolicBp,
            DiastolicBp = encounter.DiastolicBp,
            OxygenSaturation = encounter.OxygenSaturation
        };
    }

    private static TimeZoneInfo ResolveClinicTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        }
        catch
        {
            return TimeZoneInfo.Utc;
        }
    }

    private static DateTime ConvertUtcToClinicLocal(DateTime utcDateTime)
        => TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc), ResolveClinicTimeZone());
}
