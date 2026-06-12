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
    public class TypeServiceRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<TypeServiceRepository> _logger;

        public TypeServiceRepository(IConfiguration configuration, ILogger<TypeServiceRepository> logger)
        {
            _connectionString =
                configuration.GetConnectionString("Default")
                ?? configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'Default' or 'DefaultConnection' not found in configuration.");

            _logger = logger;
        }

        public async Task<IEnumerable<TypeService>> GetAllAsync(CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct).ConfigureAwait(false);
            var serviceNameColumn = await ResolveServiceNameColumnAsync(conn, ct).ConfigureAwait(false);
            var sql = $@"
                SELECT
                    Id,
                    [{serviceNameColumn}] AS Name
                FROM dbo.TypeService
                ORDER BY [{serviceNameColumn}];
            ";

            return await conn.QueryAsync<TypeService>(new CommandDefinition(sql, cancellationToken: ct)).ConfigureAwait(false);
        }

        public async Task<TypeService?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct).ConfigureAwait(false);
            var serviceNameColumn = await ResolveServiceNameColumnAsync(conn, ct).ConfigureAwait(false);
            var sql = $@"
                SELECT
                    Id,
                    [{serviceNameColumn}] AS Name
                FROM dbo.TypeService
                WHERE Id = @Id;
            ";

            return await conn.QuerySingleOrDefaultAsync<TypeService>(
                new CommandDefinition(sql, new { Id = id }, cancellationToken: ct)
            ).ConfigureAwait(false);
        }

        public async Task<int> InsertAsync(TypeService typeService, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct).ConfigureAwait(false);
            var serviceNameColumn = await ResolveServiceNameColumnAsync(conn, ct).ConfigureAwait(false);
            var sql = $@"
                INSERT INTO dbo.TypeService ([{serviceNameColumn}])
                VALUES (@Name);

                SELECT CAST(SCOPE_IDENTITY() AS int);
            ";

            return await conn.ExecuteScalarAsync<int>(
                new CommandDefinition(sql, typeService, cancellationToken: ct)
            ).ConfigureAwait(false);
        }

        public async Task UpdateAsync(TypeService typeService, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct).ConfigureAwait(false);
            var serviceNameColumn = await ResolveServiceNameColumnAsync(conn, ct).ConfigureAwait(false);
            var sql = $@"
                UPDATE dbo.TypeService
                SET [{serviceNameColumn}] = @Name
                WHERE Id = @Id;
            ";

            await conn.ExecuteAsync(new CommandDefinition(sql, typeService, cancellationToken: ct)).ConfigureAwait(false);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            const string sql = @"
                DELETE FROM dbo.TypeService
                WHERE Id = @Id;
            ";

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct).ConfigureAwait(false);
            await conn.ExecuteAsync(new CommandDefinition(sql, new { Id = id }, cancellationToken: ct)).ConfigureAwait(false);
        }

        private static async Task<string> ResolveServiceNameColumnAsync(SqlConnection conn, CancellationToken ct)
        {
            const string sql = @"
                SELECT TOP 1 COLUMN_NAME
                FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_SCHEMA = 'dbo'
                  AND TABLE_NAME = 'TypeService'
                  AND COLUMN_NAME IN ('ServiceName', 'Name', 'Nome')
                ORDER BY
                    CASE COLUMN_NAME
                        WHEN 'ServiceName' THEN 1
                        WHEN 'Name' THEN 2
                        WHEN 'Nome' THEN 3
                        ELSE 99
                    END;
            ";

            var columnName = await conn.QuerySingleOrDefaultAsync<string>(
                new CommandDefinition(sql, cancellationToken: ct)
            ).ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(columnName))
            {
                throw new InvalidOperationException(
                    "No service-name column found in dbo.TypeService. Expected one of: ServiceName, Name, Nome.");
            }

            return columnName;
        }
    }
}