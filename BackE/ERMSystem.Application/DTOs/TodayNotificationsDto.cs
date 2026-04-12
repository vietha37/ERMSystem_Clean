using System.Collections.Generic;

namespace ERMSystem.Application.DTOs
{
    public class TodayNotificationsDto
    {
        public int UnreadCount { get; set; }
        public List<AppointmentNotificationDto> Notifications { get; set; } = new();
    }
}
