using System;
using System.Threading;
using System.Threading.Tasks;
using ERMSystem.Application.DTOs;

namespace ERMSystem.Application.Interfaces
{
    public interface IHospitalNotificationDeliveryService
    {
        Task<NotificationDeliveryListDto> GetDeliveriesAsync(string? status, int pageNumber, int pageSize, CancellationToken ct = default);
        Task<bool> RetryDeliveryAsync(Guid deliveryId, CancellationToken ct = default);
    }
}
