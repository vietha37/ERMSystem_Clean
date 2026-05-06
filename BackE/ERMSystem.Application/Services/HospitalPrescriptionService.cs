using System;
using System.Linq;
using System.Text.Json;
using ERMSystem.Application.DTOs;
using ERMSystem.Application.DTOs.Common;
using ERMSystem.Application.Interfaces;

namespace ERMSystem.Application.Services;

public class HospitalPrescriptionService : IHospitalPrescriptionService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly string[] AllowedStatuses = ["Issued", "Dispensed", "Cancelled"];

    private readonly IHospitalPrescriptionRepository _hospitalPrescriptionRepository;

    public HospitalPrescriptionService(IHospitalPrescriptionRepository hospitalPrescriptionRepository)
    {
        _hospitalPrescriptionRepository = hospitalPrescriptionRepository;
    }

    public Task<PaginatedResult<HospitalPrescriptionSummaryDto>> GetWorklistAsync(
        HospitalPrescriptionWorklistRequestDto request,
        CancellationToken ct = default)
        => _hospitalPrescriptionRepository.GetWorklistAsync(request, ct);

    public async Task<HospitalPrescriptionDetailDto?> GetByIdAsync(Guid prescriptionId, CancellationToken ct = default)
    {
        var prescription = await _hospitalPrescriptionRepository.GetByIdAsync(prescriptionId, ct);
        return prescription == null ? null : MapDetail(prescription);
    }

    public Task<HospitalPrescriptionEligibleEncounterDto[]> GetEligibleEncountersAsync(CancellationToken ct = default)
        => _hospitalPrescriptionRepository.GetEligibleEncountersAsync(ct);

    public Task<HospitalMedicineCatalogDto[]> GetMedicineCatalogAsync(CancellationToken ct = default)
        => _hospitalPrescriptionRepository.GetMedicineCatalogAsync(ct);

    public async Task<HospitalPrescriptionDetailDto> CreateAsync(
        CreateHospitalPrescriptionDto request,
        Guid? actorUserId,
        CancellationToken ct = default)
    {
        actorUserId = await ResolveHospitalActorUserIdAsync(actorUserId, ct);
        if (request.Items == null || request.Items.Count == 0)
        {
            throw new InvalidOperationException("Don thuoc phai co it nhat mot thuoc.");
        }

        var normalizedStatus = NormalizeStatus(request.Status);
        var encounter = await _hospitalPrescriptionRepository.GetEncounterForPrescriptionAsync(request.EncounterId, ct);
        if (encounter == null)
        {
            throw new KeyNotFoundException("Khong tim thay encounter de phat hanh don thuoc.");
        }

        if (encounter.ExistingPrescriptionId.HasValue)
        {
            throw new InvalidOperationException("Encounter nay da co don thuoc.");
        }

        if (encounter.EncounterStatus is not ("InProgress" or "Finalized"))
        {
            throw new InvalidOperationException("Chi duoc phat hanh don thuoc cho encounter dang kham hoac da chot ho so.");
        }

        var medicineIds = request.Items.Select(x => x.MedicineId).Distinct().ToArray();
        var medicines = await _hospitalPrescriptionRepository.GetMedicinesByIdsAsync(medicineIds, ct);
        if (medicines.Length != medicineIds.Length)
        {
            throw new InvalidOperationException("Co thuoc khong ton tai hoac da ngung hoat dong.");
        }

        var medicineLookup = medicines.ToDictionary(x => x.MedicineId, x => x);
        var nowUtc = DateTime.UtcNow;
        var orderHeaderId = Guid.NewGuid();
        var prescriptionId = Guid.NewGuid();

        await _hospitalPrescriptionRepository.AddOrderHeaderAsync(new HospitalPrescriptionOrderHeaderCreateCommand
        {
            OrderHeaderId = orderHeaderId,
            EncounterId = encounter.EncounterId,
            OrderNumber = GenerateOrderNumber(nowUtc),
            OrderCategory = "Pharmacy",
            OrderStatus = "Ordered",
            OrderedByUserId = actorUserId,
            OrderedAtUtc = nowUtc
        }, ct);

        await _hospitalPrescriptionRepository.AddPrescriptionAsync(new HospitalPrescriptionCreateCommand
        {
            PrescriptionId = prescriptionId,
            OrderHeaderId = orderHeaderId,
            PrescriptionNumber = GeneratePrescriptionNumber(nowUtc),
            Status = normalizedStatus,
            Notes = NormalizeText(request.Notes),
            CreatedAtUtc = nowUtc
        }, ct);

        foreach (var item in request.Items)
        {
            var medicine = medicineLookup[item.MedicineId];
            await _hospitalPrescriptionRepository.AddPrescriptionItemAsync(new HospitalPrescriptionItemCreateCommand
            {
                PrescriptionItemId = Guid.NewGuid(),
                PrescriptionId = prescriptionId,
                MedicineId = item.MedicineId,
                DoseInstruction = NormalizeRequiredText(item.DoseInstruction, "Lieu dung"),
                Route = NormalizeText(item.Route),
                Frequency = NormalizeText(item.Frequency),
                DurationDays = item.DurationDays,
                Quantity = item.Quantity,
                UnitPrice = null
            }, ct);
        }

        await _hospitalPrescriptionRepository.AddOutboxMessageAsync(new HospitalPrescriptionOutboxCreateCommand
        {
            OutboxMessageId = Guid.NewGuid(),
            AggregateType = "Prescription",
            AggregateId = prescriptionId,
            EventType = "PrescriptionIssued.v1",
            PayloadJson = JsonSerializer.Serialize(new
            {
                prescriptionId,
                encounter.EncounterId,
                encounter.EncounterNumber,
                encounter.PatientId,
                encounter.PatientName,
                encounter.MedicalRecordNumber,
                encounter.DoctorProfileId,
                encounter.DoctorName,
                encounter.SpecialtyName,
                status = normalizedStatus,
                items = request.Items.Select(item =>
                {
                    var medicine = medicineLookup[item.MedicineId];
                    return new
                    {
                        item.MedicineId,
                        medicine.DrugCode,
                        medicineName = medicine.Name,
                        item.DoseInstruction,
                        item.Route,
                        item.Frequency,
                        item.DurationDays,
                        item.Quantity
                    };
                }),
                issuedAtUtc = nowUtc
            }, JsonOptions),
            Status = "Pending",
            AvailableAtUtc = nowUtc
        }, ct);

        await _hospitalPrescriptionRepository.SaveChangesAsync(ct);

        var created = await _hospitalPrescriptionRepository.GetByIdAsync(prescriptionId, ct)
            ?? throw new InvalidOperationException("Khong the tai lai don thuoc sau khi tao.");

        return MapDetail(created);
    }

    public async Task DeleteAsync(Guid prescriptionId, CancellationToken ct = default)
    {
        await _hospitalPrescriptionRepository.DeletePrescriptionAsync(prescriptionId, ct);
        await _hospitalPrescriptionRepository.SaveChangesAsync(ct);
    }

    private async Task<Guid?> ResolveHospitalActorUserIdAsync(Guid? actorUserId, CancellationToken ct)
    {
        if (!actorUserId.HasValue)
        {
            return null;
        }

        return await _hospitalPrescriptionRepository.HospitalUserExistsAsync(actorUserId.Value, ct)
            ? actorUserId
            : null;
    }

    private static HospitalPrescriptionDetailDto MapDetail(HospitalPrescriptionAggregateSnapshot prescription)
    {
        return new HospitalPrescriptionDetailDto
        {
            PrescriptionId = prescription.PrescriptionId,
            PrescriptionNumber = prescription.PrescriptionNumber,
            Status = prescription.Status,
            EncounterId = prescription.EncounterId,
            EncounterNumber = prescription.EncounterNumber,
            PatientId = prescription.PatientId,
            PatientName = prescription.PatientName,
            MedicalRecordNumber = prescription.MedicalRecordNumber,
            DoctorProfileId = prescription.DoctorProfileId,
            DoctorName = prescription.DoctorName,
            SpecialtyName = prescription.SpecialtyName,
            ClinicName = prescription.ClinicName,
            PrimaryDiagnosisName = prescription.PrimaryDiagnosisName,
            TotalItems = prescription.Items.Length,
            CreatedAtLocal = ConvertUtcToClinicLocal(prescription.CreatedAtUtc),
            Notes = prescription.Notes,
            Items = prescription.Items.Select(item => new HospitalPrescriptionItemDto
            {
                PrescriptionItemId = item.PrescriptionItemId,
                MedicineId = item.MedicineId,
                DrugCode = item.DrugCode,
                MedicineName = item.MedicineName,
                GenericName = item.GenericName,
                Strength = item.Strength,
                DosageForm = item.DosageForm,
                Unit = item.Unit,
                DoseInstruction = item.DoseInstruction,
                Route = item.Route,
                Frequency = item.Frequency,
                DurationDays = item.DurationDays,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            }).ToList()
        };
    }

    private static string NormalizeStatus(string? status)
    {
        var normalized = string.IsNullOrWhiteSpace(status) ? "Issued" : status.Trim();
        if (!AllowedStatuses.Contains(normalized, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Trang thai don thuoc khong hop le.");
        }

        return AllowedStatuses.First(x => string.Equals(x, normalized, StringComparison.OrdinalIgnoreCase));
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

    private static string GenerateOrderNumber(DateTime nowUtc)
        => $"ORD-PHA-{nowUtc:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}";

    private static string GeneratePrescriptionNumber(DateTime nowUtc)
        => $"RX-{nowUtc:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}";

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
