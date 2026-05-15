namespace ERMSystem.Application.DTOs
{
    public enum NotificationDeliveryRetryResult
    {
        NotFound = 0,
        Requeued = 1,
        InvalidStatus = 2
    }
}
