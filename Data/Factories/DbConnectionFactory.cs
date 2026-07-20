using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace RapportinoServer.Data
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }

    public sealed class DbConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        public DbConnectionFactory(IConfiguration configuration)
        {
            if (configuration is null) throw new ArgumentNullException(nameof(configuration));

            // Tenta chaves comuns para connection string e faz fallback
            _connectionString =
                configuration.GetConnectionString("Default")
                ?? configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'Default' or 'DefaultConnection' not found in configuration.");
        }

        public IDbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}