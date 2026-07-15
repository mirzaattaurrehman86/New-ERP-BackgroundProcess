using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ZKTecoAttendanceService.Configurations;

namespace ZKTecoAttendanceService.Infrastructure
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
