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
    public class MachineRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<MachineRepository> _logger;

        public MachineRepository(IConfiguration configuration, ILogger<MachineRepository> logger)
        {
            _connectionString =
                configuration.GetConnectionString("Default")
                ?? configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'Default' or 'DefaultConnection' not found in configuration.");

            _logger = logger;
        }

        public async Task<IEnumerable<Machine>> GetAllAsync(CancellationToken ct = default)
        {
            const string sql = @"
                SELECT
                    Id,
                    ClientId,
                    Model,
                    Serial
                FROM dbo.Machines
                ORDER BY Model;
            ";

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct).ConfigureAwait(false);
            return await conn.QueryAsync<Machine>(new CommandDefinition(sql, cancellationToken: ct)).ConfigureAwait(false);
        }

        public async Task<Machine?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            const string sql = @"
                SELECT
                    Id,
                    ClientId,
                    Model,
                    Serial
                FROM dbo.Machines
                WHERE Id = @Id;
            ";

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct).ConfigureAwait(false);
            return await conn.QuerySingleOrDefaultAsync<Machine>(
                new CommandDefinition(sql, new { Id = id }, cancellationToken: ct)
            ).ConfigureAwait(false);
        }

        public async Task<IEnumerable<Machine>> GetByClientAsync(int clientId, CancellationToken ct = default)
        {
            const string sql = @"
                SELECT
                    Id,
                    ClientId,
                    Model,
                    Serial
                FROM dbo.Machines
                WHERE ClientId = @ClientId
                ORDER BY Model;
            ";

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct).ConfigureAwait(false);
            return await conn.QueryAsync<Machine>(
                new CommandDefinition(sql, new { ClientId = clientId }, cancellationToken: ct)
            ).ConfigureAwait(false);
        }

        public async Task<int> InsertAsync(Machine machine, CancellationToken ct = default)
        {
            const string sql = @"
                INSERT INTO dbo.Machines (ClientId, Model, Serial)
                VALUES (@ClientId, @Model, @Serial);
                SELECT CAST(SCOPE_IDENTITY() AS int);
            ";

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct).ConfigureAwait(false);
            return await conn.ExecuteScalarAsync<int>(
                new CommandDefinition(sql, machine, cancellationToken: ct)
            ).ConfigureAwait(false);
        }

        public async Task UpdateAsync(Machine machine, CancellationToken ct = default)
        {
            const string sql = @"
                UPDATE dbo.Machines
                SET ClientId = @ClientId,
                    Model = @Model,
                    Serial = @Serial
                WHERE Id = @Id;
            ";

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct).ConfigureAwait(false);
            await conn.ExecuteAsync(new CommandDefinition(sql, machine, cancellationToken: ct)).ConfigureAwait(false);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            const string sql = @"
                DELETE FROM dbo.Machines
                WHERE Id = @Id;
            ";

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct).ConfigureAwait(false);
            await conn.ExecuteAsync(new CommandDefinition(sql, new { Id = id }, cancellationToken: ct)).ConfigureAwait(false);
        }
    }
}