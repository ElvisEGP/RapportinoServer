using System;
using System.Collections.Generic;

namespace RapportinoServer.Models
{
    public sealed class DashboardSummary
    {
        public int ClientCount { get; set; }
        public int MachineCount { get; set; }
        public int TechnicianCount { get; set; }
        public int TypeServiceCount { get; set; }
        public int ReportTotal { get; set; }
        public int ReportsThisMonth { get; set; }
        public int ReportsPreviousMonth { get; set; }
        public long WorkedMinutesThisMonth { get; set; }
        public double WorkedHoursThisMonthForUser { get; set; }


        public IReadOnlyList<DashboardRecentReport> RecentReports { get; set; } = Array.Empty<DashboardRecentReport>();
        public IReadOnlyList<DashboardServiceSlice> ServiceTypeBreakdown { get; set; } = Array.Empty<DashboardServiceSlice>();

        public int MonthOverMonthDeltaPercent
        {
            get
            {
                if (ReportsPreviousMonth == 0)
                    return ReportsThisMonth > 0 ? 100 : 0;
                return (int)Math.Round((ReportsThisMonth - ReportsPreviousMonth) * 100.0 / ReportsPreviousMonth);
            }
        }

        public double WorkedHoursThisMonth => Math.Round(WorkedMinutesThisMonth / 60.0, 1);
    }

    public sealed class DashboardRecentReport
    {
        public int Id { get; set; }
        public string? ClientName { get; set; }
        public DateTime Data { get; set; }
        public string Serial { get; set; } = string.Empty;
        public string TechnicianName { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty;
    }

    public sealed class DashboardServiceSlice
    {
        public string Label { get; set; } = string.Empty;
        public int ItemCount { get; set; }
    }
}
