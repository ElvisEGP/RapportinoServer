using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RapportinoServer.Models;

namespace RapportinoServer.Data.Repositories
{
    public class DashboardRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<DashboardRepository> _logger;

        public DashboardRepository(IConfiguration configuration, ILogger<DashboardRepository> logger)
        {
            _connectionString =
                configuration.GetConnectionString("Default")
                ?? configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'Default' or 'DefaultConnection' not found in configuration.");

            _logger = logger;
        }

        public async Task<DashboardSummary> GetSummaryAsync(CancellationToken ct = default)
        {
            const string aggregatesSql = @"
                SELECT
                    (SELECT COUNT(*) FROM dbo.Client) AS ClientCount,
                    (SELECT COUNT(*) FROM dbo.Machines) AS MachineCount,
                    (SELECT COUNT(*) FROM dbo.Technician) AS TechnicianCount,
                    (SELECT COUNT(*) FROM dbo.TypeService) AS TypeServiceCount,
                    (SELECT COUNT(*) FROM dbo.Report) AS ReportTotal,
                    (
                        SELECT COUNT(*)
                        FROM dbo.Report
                        WHERE [Data] >= DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1)
                          AND [Data] < DATEADD(MONTH, 1, DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1))
                    ) AS ReportsThisMonth,
                    (
                        SELECT COUNT(*)
                        FROM dbo.Report
                        WHERE [Data] >= DATEADD(MONTH, -1, DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1))
                          AND [Data] < DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1)
                    ) AS ReportsPreviousMonth,
                    (
                        SELECT CAST(COALESCE(SUM(DATEDIFF(MINUTE, CAST('00:00:00' AS TIME), wl.WorkedTime)), 0) AS BIGINT)
                        FROM dbo.ReportWorkLog wl
                        INNER JOIN dbo.Report r ON r.Id = wl.ReportId
                        WHERE r.[Data] >= DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1)
                          AND r.[Data] < DATEADD(MONTH, 1, DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1))
                    ) AS WorkedMinutesThisMonth;
            ";

            const string recentSql = @"
                SELECT TOP 8
                    r.Id,
                    c.CompanyName AS ClientName,
                    r.[Data],
                    r.Serial,
                    r.TechnicianName,
                    r.ServiceType
                FROM dbo.Report r
                LEFT JOIN dbo.Client c ON r.ClientId = c.Id
                ORDER BY r.[Data] DESC, r.Id DESC;
            ";

            const string servicesSql = @"
                SELECT TOP 8
                    ISNULL(NULLIF(LTRIM(RTRIM(ServiceType)), N''), N'Non specificato') AS Label,
                    COUNT(*) AS ItemCount
                FROM dbo.Report
                GROUP BY ISNULL(NULLIF(LTRIM(RTRIM(ServiceType)), N''), N'Non specificato')
                ORDER BY COUNT(*) DESC;
            ";

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct).ConfigureAwait(false);

            DashboardSummary summary;
            try
            {
                var row = await conn.QuerySingleAsync<AggregateRow>(
                    new CommandDefinition(aggregatesSql, cancellationToken: ct)
                ).ConfigureAwait(false);

                summary = new DashboardSummary
                {
                    ClientCount = row.ClientCount,
                    MachineCount = row.MachineCount,
                    TechnicianCount = row.TechnicianCount,
                    TypeServiceCount = row.TypeServiceCount,
                    ReportTotal = row.ReportTotal,
                    ReportsThisMonth = row.ReportsThisMonth,
                    ReportsPreviousMonth = row.ReportsPreviousMonth,
                    WorkedMinutesThisMonth = row.WorkedMinutesThisMonth
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dashboard aggregates query failed");
                throw;
            }

            try
            {
                var recent = await conn.QueryAsync<DashboardRecentReport>(
                    new CommandDefinition(recentSql, cancellationToken: ct)
                ).ConfigureAwait(false);
                summary.RecentReports = recent.AsList();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Dashboard recent reports query failed (table may be missing)");
                summary.RecentReports = Array.Empty<DashboardRecentReport>();
            }

            try
            {
                var slices = await conn.QueryAsync<DashboardServiceSlice>(
                    new CommandDefinition(servicesSql, cancellationToken: ct)
                ).ConfigureAwait(false);
                summary.ServiceTypeBreakdown = slices.AsList();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Dashboard service breakdown query failed");
                summary.ServiceTypeBreakdown = Array.Empty<DashboardServiceSlice>();
            }

            return summary;
        }
        public async Task<double> GetWorkedHoursThisMonthForTechnicianAsync(int technicianId)
        {
            const string sql = @"
                SELECT 
                    CAST(COALESCE(SUM(DATEDIFF(MINUTE, CAST('00:00:00' AS TIME), wl.WorkedTime)), 0) AS BIGINT)
                FROM dbo.ReportWorkLog wl
                INNER JOIN dbo.Report r ON r.Id = wl.ReportId
                WHERE r.TechnicianId = @TechId
                AND r.[Data] >= DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1)
                AND r.[Data] < DATEADD(MONTH, 1, DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1));
            ";

            using var conn = new SqlConnection(_connectionString);
            return await conn.ExecuteScalarAsync<long>(sql, new { TechId = technicianId });
        }

        

        private sealed class AggregateRow
        {
            public int ClientCount { get; set; }
            public int MachineCount { get; set; }
            public int TechnicianCount { get; set; }
            public int TypeServiceCount { get; set; }
            public int ReportTotal { get; set; }
            public int ReportsThisMonth { get; set; }
            public int ReportsPreviousMonth { get; set; }
            public long WorkedMinutesThisMonth { get; set; }
        }
    }
}
