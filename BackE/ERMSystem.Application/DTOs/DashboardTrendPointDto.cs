namespace ERMSystem.Application.DTOs
{
    public class DashboardTrendPointDto
    {
        public string Label { get; set; } = string.Empty;
        public int PatientsCount { get; set; }
        public int PrescriptionsCount { get; set; }
    }
}
