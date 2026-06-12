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
    public class ClientRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<ClientRepository> _logger;

        public ClientRepository(IConfiguration configuration, ILogger<ClientRepository> logger)
        {
            _connectionString =
                configuration.GetConnectionString("Default")
                ?? configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'Default' or 'DefaultConnection' not found in configuration.");

            _logger = logger;
        }

        public async Task<IEnumerable<Client>> GetAllAsync(CancellationToken ct = default)
        {
            const string sql = @"
                SELECT
                    Id,
                    Data,
                    CompanyName,
                    Address,
                    NumberAddress,
                    City,
                    PostalCode,
                    State,
                    Country,
                    Phone,
                    Email,
                    Website,
                    Note
                FROM dbo.Client
                ORDER BY CompanyName;
            ";

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct).ConfigureAwait(false);
            return await conn.QueryAsync<Client>(new CommandDefinition(sql, cancellationToken: ct)).ConfigureAwait(false);
        }

        public async Task<Client?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            const string sql = @"
                SELECT
                    Id,
                    Data,
                    CompanyName,
                    Address,
                    NumberAddress,
                    City,
                    PostalCode,
                    State,
                    Country,
                    Phone,
                    Email,
                    Website,
                    Note
                FROM dbo.Client
                WHERE Id = @Id;
            ";

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct).ConfigureAwait(false);
            return await conn.QuerySingleOrDefaultAsync<Client>(
                new CommandDefinition(sql, new { Id = id }, cancellationToken: ct)
            ).ConfigureAwait(false);
        }

        public async Task<int> InsertAsync(Client client, CancellationToken ct = default)
        {
            const string sql = @"
                INSERT INTO dbo.Client
                (
                    Data,
                    CompanyName,
                    Address,
                    NumberAddress,
                    City,
                    PostalCode,
                    State,
                    Country,
                    Phone,
                    Email,
                    Website,
                    Note
                )
                VALUES
                (
                    @Data,
                    @CompanyName,
                    @Address,
                    @NumberAddress,
                    @City,
                    @PostalCode,
                    @State,
                    @Country,
                    @Phone,
                    @Email,
                    @Website,
                    @Note
                );
                SELECT CAST(SCOPE_IDENTITY() AS int);
            ";

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct).ConfigureAwait(false);
            return await conn.ExecuteScalarAsync<int>(
                new CommandDefinition(sql, client, cancellationToken: ct)
            ).ConfigureAwait(false);
        }

        public async Task UpdateAsync(Client client, CancellationToken ct = default)
        {
            const string sql = @"
                UPDATE dbo.Client
                SET
                    Data = @Data,
                    CompanyName = @CompanyName,
                    Address = @Address,
                    NumberAddress = @NumberAddress,
                    City = @City,
                    PostalCode = @PostalCode,
                    State = @State,
                    Country = @Country,
                    Phone = @Phone,
                    Email = @Email,
                    Website = @Website,
                    Note = @Note
                WHERE Id = @Id;
            ";

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct).ConfigureAwait(false);
            await conn.ExecuteAsync(new CommandDefinition(sql, client, cancellationToken: ct)).ConfigureAwait(false);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            const string deleteMachinesSql = @"
                DELETE FROM dbo.Machines
                WHERE ClientId = @Id;
            ";

            const string deleteClientSql = @"
                DELETE FROM dbo.Client
                WHERE Id = @Id;
            ";

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct).ConfigureAwait(false);
            await using var transaction = await conn.BeginTransactionAsync(ct).ConfigureAwait(false);

            try
            {
                await conn.ExecuteAsync(
                    new CommandDefinition(deleteMachinesSql, new { Id = id }, transaction: transaction, cancellationToken: ct)
                ).ConfigureAwait(false);

                await conn.ExecuteAsync(
                    new CommandDefinition(deleteClientSql, new { Id = id }, transaction: transaction, cancellationToken: ct)
                ).ConfigureAwait(false);

                await transaction.CommitAsync(ct).ConfigureAwait(false);
            }
            catch
            {
                await transaction.RollbackAsync(ct).ConfigureAwait(false);
                throw;
            }
        }

        public async Task<IEnumerable<Client>> SearchByNameAsync(string term, CancellationToken ct = default, int limit = 20)
        {
            const string sql = @"
                SELECT TOP (@Limit)
                    Id,
                    Data,
                    CompanyName,
                    Address,
                    NumberAddress,
                    City,
                    PostalCode,
                    State,
                    Country,
                    Phone,
                    Email,
                    Website,
                    Note
                FROM dbo.Client
                WHERE CompanyName LIKE @Pattern
                ORDER BY CompanyName;
            ";

            var parameters = new { Pattern = $"%{term}%", Limit = limit };

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct).ConfigureAwait(false);
            return await conn.QueryAsync<Client>(new CommandDefinition(sql, parameters, cancellationToken: ct)).ConfigureAwait(false);
        }
    }
}