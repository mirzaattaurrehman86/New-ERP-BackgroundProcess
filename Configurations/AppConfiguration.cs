using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace ZKTecoAttendanceService.Configurations
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
