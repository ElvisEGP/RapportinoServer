using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RapportinoServer.Models;

namespace RapportinoServer.Data.Repositories
{
    public class ReportRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<ReportRepository> _logger;

        public ReportRepository(IConfiguration configuration, ILogger<ReportRepository> logger)
        {
            _connectionString =
                configuration.GetConnectionString("Default")
                ?? configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'Default' or 'DefaultConnection' not found in configuration.");

            _logger = logger;
        }

        private sealed class ReportRow
        {
            public int Id { get; set; }
            public int ClientId { get; set; }
            public string? CompanyName { get; set; }
            public DateTime Data { get; set; }
            public string Serial { get; set; } = string.Empty;
            public string Model { get; set; } = string.Empty;
            public string ReportDescription { get; set; } = string.Empty;
            public string? ChangedMaterial { get; set; }
            public string? OrderedMaterial { get; set; }
            public string Email { get; set; } = string.Empty;
            public string TechnicianName { get; set; } = string.Empty;
            public string ServiceType { get; set; } = string.Empty;
            public bool SafetyDevicesCheck { get; set; }
        }

        private sealed class WorkLogRow
        {
            public int Id { get; set; }
            public int ReportId { get; set; }
            public DateTime WorkedDate { get; set; }
            public TimeSpan WorkedTime { get; set; }
        }

        private static Report Map(ReportRow row)
        {
            var companyName = row.CompanyName;
            if (string.IsNullOrWhiteSpace(companyName))
            {
                companyName = "Cliente non specificato";
            }

            var report = new Report
            {
                Id = row.Id,
                ClientId = row.ClientId,
                Client = new Client 
                { 
                    Id = row.ClientId, 
                    CompanyName = companyName,
                    Address = "", NumberAddress = "", City = "", PostalCode = "", State = "", Country = "", Phone = "", Email = "" 
                },
                Data = DateOnly.FromDateTime(row.Data),
                Serial = row.Serial,
                Model = row.Model,
                ReportDescription = row.ReportDescription,
                ChangedMaterial = row.ChangedMaterial,
                OrderedMaterial = row.OrderedMaterial,
                Email = row.Email,
                TechnicianName = row.TechnicianName,
                ServiceType = row.ServiceType,
                SafetyDevicesCheck = row.SafetyDevicesCheck
            };

            return report;
        }

        private static List<ReportWorkLog> MapWorkLogs(IEnumerable<WorkLogRow> rows)
        {
            var result = new List<ReportWorkLog>();
            foreach (var row in rows)
            {
                result.Add(new ReportWorkLog
                {
                    Id = row.Id,
                    ReportId = row.ReportId,
                    WorkedDate = DateOnly.FromDateTime(row.WorkedDate),
                    WorkedTime = row.WorkedTime
                });
            }

            return result;
        }

        public async Task<IEnumerable<Report>> GetAllAsync(CancellationToken ct = default)
        {
            const string sql = @"
                SELECT
                    r.Id,
                    r.Data,
                    r.Serial,
                    r.Model,
                    r.ReportDescription,
                    r.ChangedMaterial,
                    r.OrderedMaterial,
                    r.Email,
                    r.TechnicianName,
                    r.ServiceType,
                    r.SafetyDevicesCheck,
                    r.ClientId,
                    c.CompanyName
                FROM dbo.Report r
                LEFT JOIN dbo.Client c ON r.ClientId = c.Id
                ORDER BY r.Data DESC, r.Id DESC;
            ";

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct).ConfigureAwait(false);

            var rows = await conn.QueryAsync<ReportRow>(
                new CommandDefinition(sql, cancellationToken: ct)
            ).ConfigureAwait(false);

            var reportList = new List<Report>();
            var reportMap = new Dictionary<int, Report>();
            foreach (var row in rows)
            {
                var report = Map(row);
                reportList.Add(report);
                reportMap[report.Id] = report;
            }

            if (reportList.Count == 0)
                return reportList;

            const string logsSql = @"
                SELECT
                    Id,
                    ReportId,
                    WorkedDate,
                    WorkedTime
                FROM dbo.ReportWorkLog
                WHERE ReportId IN @ReportIds
                ORDER BY WorkedDate, Id;
            ";

            var logs = await conn.QueryAsync<WorkLogRow>(
                new CommandDefinition(logsSql, new { ReportIds = reportMap.Keys }, cancellationToken: ct)
            ).ConfigureAwait(false);

            foreach (var log in logs)
            {
                if (reportMap.TryGetValue(log.ReportId, out var report))
                {
                    report.WorkLogs.Add(new ReportWorkLog
                    {
                        Id = log.Id,
                        ReportId = log.ReportId,
                        WorkedDate = DateOnly.FromDateTime(log.WorkedDate),
                        WorkedTime = log.WorkedTime
                    });
                }
            }

            return reportList;
        }

        public async Task<Report?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            const string sql = @"
                SELECT
                    r.Id,
                    r.Data,
                    r.Serial,
                    r.Model,
                    r.ReportDescription,
                    r.ChangedMaterial,
                    r.OrderedMaterial,
                    r.Email,
                    r.TechnicianName,
                    r.ServiceType,
                    r.SafetyDevicesCheck,
                    r.ClientId,
                    c.CompanyName
                FROM dbo.Report r
                LEFT JOIN dbo.Client c ON r.ClientId = c.Id
                WHERE r.Id = @Id;
            ";

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct).ConfigureAwait(false);

            var row = await conn.QuerySingleOrDefaultAsync<ReportRow>(
                new CommandDefinition(sql, new { Id = id }, cancellationToken: ct)
            ).ConfigureAwait(false);

            if (row is null)
                return null;

            var report = Map(row);

            const string logsSql = @"
                SELECT
                    Id,
                    ReportId,
                    WorkedDate,
                    WorkedTime
                FROM dbo.ReportWorkLog
                WHERE ReportId = @Id
                ORDER BY WorkedDate, Id;
            ";

            var logs = await conn.QueryAsync<WorkLogRow>(
                new CommandDefinition(logsSql, new { Id = id }, cancellationToken: ct)
            ).ConfigureAwait(false);

            report.WorkLogs = MapWorkLogs(logs);
            return report;
        }

        public async Task<int> InsertAsync(Report report, CancellationToken ct = default)
        {
            const string sql = @"
                INSERT INTO dbo.Report
                (
                    Data,
                    Serial,
                    Model,
                    ReportDescription,
                    ChangedMaterial,
                    OrderedMaterial,
                    Email,
                    TechnicianName,
                    ServiceType,
                    SafetyDevicesCheck,
                    ClientId
                )
                VALUES
                (
                    @Data,
                    @Serial,
                    @Model,
                    @ReportDescription,
                    @ChangedMaterial,
                    @OrderedMaterial,
                    @Email,
                    @TechnicianName,
                    @ServiceType,
                    @SafetyDevicesCheck,
                    @ClientId
                );

                SELECT CAST(SCOPE_IDENTITY() AS int);
            ";

            var parameters = new
            {
                Data = report.Data.ToDateTime(TimeOnly.MinValue),
                report.Serial,
                report.Model,
                report.ReportDescription,
                report.ChangedMaterial,
                report.OrderedMaterial,
                report.Email,
                report.TechnicianName,
                report.ServiceType,
                report.SafetyDevicesCheck,
                report.ClientId
            };

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct).ConfigureAwait(false);
            using var tx = conn.BeginTransaction();

            try
            {
                var reportId = await conn.ExecuteScalarAsync<int>(
                    new CommandDefinition(sql, parameters, transaction: tx, cancellationToken: ct)
                ).ConfigureAwait(false);

                report.Id = reportId;
                await ReplaceWorkLogsAsync(conn, tx, reportId, report.WorkLogs, ct).ConfigureAwait(false);
                tx.Commit();
                return reportId;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }


        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            const string sql = @"
                DELETE FROM dbo.Report
                WHERE Id = @Id;
            ";

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct).ConfigureAwait(false);
            await conn.ExecuteAsync(new CommandDefinition(sql, new { Id = id }, cancellationToken: ct)).ConfigureAwait(false);
        }

        public Task<IEnumerable<Report>> GetByClientAsync(int clientId, CancellationToken ct = default)
        {
            return GetAllAsync(ct);
        }

        private static async Task ReplaceWorkLogsAsync(
            SqlConnection conn,
            SqlTransaction tx,
            int reportId,
            IEnumerable<ReportWorkLog> workLogs,
            CancellationToken ct)
        {
            const string deleteSql = @"
                DELETE FROM dbo.ReportWorkLog
                WHERE ReportId = @ReportId;
            ";

            await conn.ExecuteAsync(
                new CommandDefinition(deleteSql, new { ReportId = reportId }, transaction: tx, cancellationToken: ct)
            ).ConfigureAwait(false);

            const string insertSql = @"
                INSERT INTO dbo.ReportWorkLog (ReportId, WorkedDate, WorkedTime)
                VALUES (@ReportId, @WorkedDate, @WorkedTime);
            ";

            foreach (var workLog in workLogs)
            {
                await conn.ExecuteAsync(
                    new CommandDefinition(
                        insertSql,
                        new
                        {
                            ReportId = reportId,
                            WorkedDate = workLog.WorkedDate.ToDateTime(TimeOnly.MinValue),
                            workLog.WorkedTime
                        },
                        transaction: tx,
                        cancellationToken: ct
                    )
                ).ConfigureAwait(false);
            }
        }
    }
}