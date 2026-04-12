using System.Collections.Generic;
using System;

namespace ERMSystem.Application.DTOs
{
    public class DashboardTrendsDto
    {
        public string Period { get; set; } = "daily";
        public List<DashboardTrendPointDto> Points { get; set; } = new List<DashboardTrendPointDto>();
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int CurrentPatientsTotal { get; set; }
        public int CurrentPrescriptionsTotal { get; set; }
        public int PreviousPatientsTotal { get; set; }
        public int PreviousPrescriptionsTotal { get; set; }
    }
}
