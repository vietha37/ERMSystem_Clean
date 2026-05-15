using System.Collections.Generic;

namespace ERMSystem.Application.DTOs
{
    public class DashboardStatsDto
    {
        public int TotalPatients { get; set; }
        public int AppointmentsToday { get; set; }
        public int PendingAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int CancelledAppointments { get; set; }
        public decimal CompletionRatePercent { get; set; }
        public decimal CancellationRatePercent { get; set; }
        public Dictionary<string, int> TopDiagnoses { get; set; } = new Dictionary<string, int>();
    }
}
