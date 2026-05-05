using ERMSystem.Application.DTOs;
using ERMSystem.Application.Interfaces;
using ERMSystem.Infrastructure.HospitalData;
using Microsoft.EntityFrameworkCore;

namespace ERMSystem.Infrastructure.Repositories;

public class HospitalNotificationDeliveryRepository : IHospitalNotificationDeliveryRepository
{
    private readonly HospitalDbContext _hospitalDbContext;

    public HospitalNotificationDeliveryRepository(HospitalDbContext hospitalDbContext)
    {
        _hospitalDbContext = hospitalDbContext;
    }

    public async Task<NotificationDeliveryListDto> GetDeliveriesAsync(string? status, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _hospitalDbContext.NotificationDeliveries.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.DeliveryStatus == status.Trim());
        }

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(x => x.LastAttemptAtUtc ?? x.DeliveredAtUtc)
            .ThenByDescending(x => x.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new NotificationDeliveryDto
            {
                Id = x.Id,
                OutboxMessageId = x.OutboxMessageId,
                ChannelCode = x.ChannelCode,
                Recipient = x.Recipient,
                DeliveryStatus = x.DeliveryStatus,
                ProviderMessageId = x.ProviderMessageId,
                AttemptCount = x.AttemptCount,
                LastAttemptAtUtc = x.LastAttemptAtUtc,
                DeliveredAtUtc = x.DeliveredAtUtc,
                ErrorMessage = x.ErrorMessage
            })
            .ToListAsync(ct);

        return new NotificationDeliveryListDto
        {
            TotalCount = totalCount,
            Items = items
        };
    }

    public async Task<bool> RetryDeliveryAsync(Guid deliveryId, CancellationToken ct = default)
    {
        var delivery = await _hospitalDbContext.NotificationDeliveries.FirstOrDefaultAsync(x => x.Id == deliveryId, ct);
        if (delivery == null)
        {
            return false;
        }

        delivery.DeliveryStatus = "Queued";
        delivery.ErrorMessage = null;
        delivery.ProviderMessageId = null;
        delivery.DeliveredAtUtc = null;

        await _hospitalDbContext.SaveChangesAsync(ct);
        return true;
    }
}
