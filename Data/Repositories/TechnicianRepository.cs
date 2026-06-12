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
    public class TechnicianRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<TechnicianRepository> _logger;

        public TechnicianRepository(IConfiguration configuration, ILogger<TechnicianRepository> logger)
        {
            _connectionString =
                configuration.GetConnectionString("Default")
                ?? configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'Default' or 'DefaultConnection' not found in configuration.");

            _logger = logger;
        }

        public async Task<IEnumerable<Technician>> GetAllAsync(CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct).ConfigureAwait(false);

            var hasPassword = await HasPasswordColumnAsync(conn, ct).ConfigureAwait(false);
            var hasExtended = await HasExtendedColumnsAsync(conn, ct).ConfigureAwait(false);

            string sql;
            if (hasPassword && hasExtended)
            {
                sql = @"
                    SELECT
                        Id,
                        TechnicianName,
                        Surname,
                        Username,
                        Password,
                        [Role]
                    FROM dbo.Technician
                    ORDER BY TechnicianName;
                ";
            }
            else if (hasPassword)
            {
                sql = @"
                    SELECT
                        Id,
                        TechnicianName,
                        CAST(NULL AS NVARCHAR(200)) AS Surname,
                        CAST(NULL AS NVARCHAR(200)) AS Username,
                        Password,
                        CAST(NULL AS NVARCHAR(100)) AS [Role]
                    FROM dbo.Technician
                    ORDER BY TechnicianName;
                ";
            }
            else
            {
                sql = @"
                    SELECT
                        Id,
                        TechnicianName,
                        CAST(NULL AS NVARCHAR(200)) AS Surname,
                        CAST(NULL AS NVARCHAR(200)) AS Username,
                        CAST('' AS NVARCHAR(200)) AS Password,
                        CAST(NULL AS NVARCHAR(100)) AS [Role]
                    FROM dbo.Technician
                    ORDER BY TechnicianName;
                ";
            }
            return await conn.QueryAsync<Technician>(new CommandDefinition(sql, cancellationToken: ct)).ConfigureAwait(false);
        }

        public async Task<Technician?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct).ConfigureAwait(false);

            var hasPassword = await HasPasswordColumnAsync(conn, ct).ConfigureAwait(false);
            var hasExtended = await HasExtendedColumnsAsync(conn, ct).ConfigureAwait(false);

            string sql;
            if (hasPassword && hasExtended)
            {
                sql = @"
                    SELECT
                        Id,
                        TechnicianName,
                        Surname,
                        Username,
                        Password,
                        [Role]
                    FROM dbo.Technician
                    WHERE Id = @Id;
                ";
            }
            else if (hasPassword)
            {
                sql = @"
                    SELECT
                        Id,
                        TechnicianName,
                        CAST(NULL AS NVARCHAR(200)) AS Surname,
                        CAST(NULL AS NVARCHAR(200)) AS Username,
                        Password,
                        CAST(NULL AS NVARCHAR(100)) AS [Role]
                    FROM dbo.Technician
                    WHERE Id = @Id;
                ";
            }
            else
            {
                sql = @"
                    SELECT
                        Id,
                        TechnicianName,
                        CAST(NULL AS NVARCHAR(200)) AS Surname,
                        CAST(NULL AS NVARCHAR(200)) AS Username,
                        CAST('' AS NVARCHAR(200)) AS Password,
                        CAST(NULL AS NVARCHAR(100)) AS [Role]
                    FROM dbo.Technician
                    WHERE Id = @Id;
                ";
            }
            return await conn.QuerySingleOrDefaultAsync<Technician>(
                new CommandDefinition(sql, new { Id = id }, cancellationToken: ct)
            ).ConfigureAwait(false);
        }

        public async Task<int> InsertAsync(Technician technician, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct).ConfigureAwait(false);

            var hasPassword = await HasPasswordColumnAsync(conn, ct).ConfigureAwait(false);
            var hasExtended = await HasExtendedColumnsAsync(conn, ct).ConfigureAwait(false);

            string sql;
            if (hasPassword && hasExtended)
            {
                sql = @"
                    INSERT INTO dbo.Technician (TechnicianName, Surname, Username, Password, [Role])
                    VALUES (@TechnicianName, @Surname, @Username, @Password, @Role);
                    SELECT CAST(SCOPE_IDENTITY() AS int);
                ";
            }
            else if (hasPassword)
            {
                sql = @"
                    INSERT INTO dbo.Technician (TechnicianName, Password)
                    VALUES (@TechnicianName, @Password);
                    SELECT CAST(SCOPE_IDENTITY() AS int);
                ";
            }
            else
            {
                sql = @"
                    INSERT INTO dbo.Technician (TechnicianName)
                    VALUES (@TechnicianName);
                    SELECT CAST(SCOPE_IDENTITY() AS int);
                ";
            }
            var parameters = new
            {
                technician.TechnicianName,
                Surname = technician.Surname ?? string.Empty,
                Username = technician.Username ?? string.Empty,
                technician.Password,
                Role = technician.Role ?? string.Empty
            };

            return await conn.ExecuteScalarAsync<int>(
                new CommandDefinition(sql, parameters, cancellationToken: ct)
            ).ConfigureAwait(false);
        }

        public async Task UpdateAsync(Technician technician, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct).ConfigureAwait(false);

            var hasPassword = await HasPasswordColumnAsync(conn, ct).ConfigureAwait(false);
            var hasExtended = await HasExtendedColumnsAsync(conn, ct).ConfigureAwait(false);

            string sql;
            if (hasPassword && hasExtended)
            {
                sql = @"
                    UPDATE dbo.Technician
                    SET TechnicianName = @TechnicianName,
                        Surname = @Surname,
                        Username = @Username,
                        Password = @Password,
                        [Role] = @Role
                    WHERE Id = @Id;
                ";
            }
            else if (hasPassword)
            {
                sql = @"
                    UPDATE dbo.Technician
                    SET TechnicianName = @TechnicianName,
                        Password = @Password
                    WHERE Id = @Id;
                ";
            }
            else
            {
                sql = @"
                    UPDATE dbo.Technician
                    SET TechnicianName = @TechnicianName
                    WHERE Id = @Id;
                ";
            }
            var parameters = new
            {
                technician.Id,
                technician.TechnicianName,
                Surname = technician.Surname ?? string.Empty,
                Username = technician.Username ?? string.Empty,
                technician.Password,
                Role = technician.Role ?? string.Empty
            };

            await conn.ExecuteAsync(new CommandDefinition(sql, parameters, cancellationToken: ct)).ConfigureAwait(false);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            const string sql = @"
                DELETE FROM dbo.Technician
                WHERE Id = @Id;
            ";

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct).ConfigureAwait(false);
            await conn.ExecuteAsync(new CommandDefinition(sql, new { Id = id }, cancellationToken: ct)).ConfigureAwait(false);
        }

        public async Task<Technician?> ValidateCredentialsAsync(string username, string password, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct).ConfigureAwait(false);

            if (!await HasPasswordColumnAsync(conn, ct).ConfigureAwait(false))
            {
                _logger.LogWarning("Tecnician login attempted but Password column was not found in dbo.Technician.");
                return null;
            }

            var hasUsername = await HasUsernameColumnAsync(conn, ct).ConfigureAwait(false);

            string sql;
            object parameters;
            if (hasUsername)
            {
                sql = @"
                SELECT TOP 1
                    Id,
                    TechnicianName,
                    Surname,
                    Username,
                    Password,
                    [Role]
                FROM dbo.Technician
                WHERE Username = @Username
                  AND Password = @Password;
            ";
                parameters = new { Username = username, Password = password };
            }
            else
            {
                // fallback for legacy schema without Username
                sql = @"
                SELECT TOP 1
                    Id,
                    TechnicianName,
                    CAST(NULL AS NVARCHAR(200)) AS Surname,
                    CAST(NULL AS NVARCHAR(200)) AS Username,
                    Password,
                    CAST(NULL AS NVARCHAR(100)) AS [Role]
                FROM dbo.Technician
                WHERE TechnicianName = @TechnicianName
                  AND Password = @Password;
            ";
                parameters = new { TechnicianName = username, Password = password };
            }

            return await conn.QuerySingleOrDefaultAsync<Technician>(
                new CommandDefinition(
                    sql,
                    parameters,
                    cancellationToken: ct)
            ).ConfigureAwait(false);
        }

        private static async Task<bool> HasPasswordColumnAsync(SqlConnection conn, CancellationToken ct)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS
                (
                    SELECT 1
                    FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = 'dbo'
                      AND TABLE_NAME = 'Technician'
                      AND COLUMN_NAME = 'Password'
                )
                THEN CAST(1 AS BIT)
                ELSE CAST(0 AS BIT)
                END;
            ";

            return await conn.ExecuteScalarAsync<bool>(new CommandDefinition(sql, cancellationToken: ct)).ConfigureAwait(false);
        }

        private static async Task<bool> HasExtendedColumnsAsync(SqlConnection conn, CancellationToken ct)
        {
            const string sql = @"
                SELECT CASE WHEN
                (
                    EXISTS
                    (
                        SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
                        WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Technician' AND COLUMN_NAME = 'Surname'
                    )
                    AND EXISTS
                    (
                        SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
                        WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Technician' AND COLUMN_NAME = 'Username'
                    )
                    AND EXISTS
                    (
                        SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
                        WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Technician' AND COLUMN_NAME = 'Role'
                    )
                )
                THEN CAST(1 AS BIT)
                ELSE CAST(0 AS BIT)
                END;
            ";

            return await conn.ExecuteScalarAsync<bool>(new CommandDefinition(sql, cancellationToken: ct)).ConfigureAwait(false);
        }

        private static async Task<bool> HasUsernameColumnAsync(SqlConnection conn, CancellationToken ct)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS
                (
                    SELECT 1
                    FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = 'dbo'
                      AND TABLE_NAME = 'Technician'
                      AND COLUMN_NAME = 'Username'
                )
                THEN CAST(1 AS BIT)
                ELSE CAST(0 AS BIT)
                END;
            ";

            return await conn.ExecuteScalarAsync<bool>(new CommandDefinition(sql, cancellationToken: ct)).ConfigureAwait(false);
        }
    }
}