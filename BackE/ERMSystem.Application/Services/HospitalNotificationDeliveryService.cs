using System;
using System.Threading;
using System.Threading.Tasks;
using ERMSystem.Application.DTOs;
using ERMSystem.Application.Interfaces;

namespace ERMSystem.Application.Services
{
    public class HospitalNotificationDeliveryService : IHospitalNotificationDeliveryService
    {
        private readonly IHospitalNotificationDeliveryRepository _repository;

        public HospitalNotificationDeliveryService(IHospitalNotificationDeliveryRepository repository)
        {
            _repository = repository;
        }

        public Task<NotificationDeliveryListDto> GetDeliveriesAsync(string? status, int pageNumber, int pageSize, CancellationToken ct = default)
            => _repository.GetDeliveriesAsync(status, pageNumber, pageSize, ct);

        public Task<bool> RetryDeliveryAsync(Guid deliveryId, CancellationToken ct = default)
            => _repository.RetryDeliveryAsync(deliveryId, ct);
    }
}
