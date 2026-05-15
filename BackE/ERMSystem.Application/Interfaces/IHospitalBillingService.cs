using ERMSystem.Application.DTOs;
using ERMSystem.Application.DTOs.Common;

namespace ERMSystem.Application.Interfaces;

public interface IHospitalBillingService
{
    Task<PaginatedResult<HospitalInvoiceSummaryDto>> GetWorklistAsync(
        HospitalInvoiceWorklistRequestDto request,
        CancellationToken ct = default);

    Task<HospitalInvoiceDetailDto?> GetByIdAsync(Guid invoiceId, CancellationToken ct = default);
    Task<HospitalBillingEligibleEncounterDto[]> GetEligibleEncountersAsync(CancellationToken ct = default);
    Task<HospitalInvoiceDetailDto> CreateInvoiceAsync(CreateHospitalInvoiceDto request, CancellationToken ct = default);
    Task<HospitalInvoiceDetailDto?> ReceivePaymentAsync(Guid invoiceId, ReceiveHospitalPaymentDto request, Guid? actorUserId, string? actorUsername, CancellationToken ct = default);
    Task<HospitalInvoiceDetailDto?> RefundPaymentAsync(Guid invoiceId, RefundHospitalPaymentDto request, Guid? actorUserId, string? actorUsername, CancellationToken ct = default);
}
