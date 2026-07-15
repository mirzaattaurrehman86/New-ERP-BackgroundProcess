using Microsoft.Extensions.Configuration;
namespace ZKTecoAttendanceService.PostgreSQL.Configurations
{
    public static class AppConfiguration
    {
        public static IConfiguration Configuration { get; }

        static AppConfiguration()
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();
        }
    }
}
