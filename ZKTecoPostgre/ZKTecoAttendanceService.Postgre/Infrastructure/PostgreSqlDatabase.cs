using Microsoft.Extensions.Configuration;
using Npgsql;
using ZKTecoAttendanceService.PostgreSQL.Configurations;

namespace ZKTecoAttendanceService.PostgreSQL.Infrastructure
{
    public class PostgreSqlDatabase
    {
        private readonly string _connectionString;

        public PostgreSqlDatabase()
        {
            _connectionString =
                AppConfiguration.Configuration.GetConnectionString("PostgreSql")!;
        }

        public NpgsqlConnection GetConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }
    }
}
