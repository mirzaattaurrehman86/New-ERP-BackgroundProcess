
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ZKTecoAttendanceService.SQL.Configurations;

namespace ZKTecoAttendanceService.SQL.Infrastructure
{
    public class SqlDatabaseConfig
    {
        private readonly string _connectionString;

        public SqlDatabaseConfig()
        {
            _connectionString =
                AppConfiguration.Configuration.GetConnectionString("SqlServer")!;
        }
        public SqlConnection GetConnection() => new SqlConnection(_connectionString);
    }
}