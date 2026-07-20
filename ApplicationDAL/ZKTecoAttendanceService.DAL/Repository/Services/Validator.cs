using System;
using System.Collections.Generic;
using System.Text;

namespace ZKTecoAttendanceService.DAL.Repository.Services
{
    public static class Validator
    {
        public static bool IsValidIP(string? ip)
        {
            return System.Net.IPAddress.TryParse(ip, out _);
        }
        public static bool IsValidPort(string port)
        {
            return int.TryParse(port, out int portNumber) && portNumber > 0 && portNumber <= 65535;
        }
    }
}
