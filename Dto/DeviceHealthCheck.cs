using System;
using System.Collections.Generic;
using System.Text;

namespace ZKTecoAttendanceService.Dto
{
    public class DeviceHealthCheck
    {
        public bool IsOnline { get; set; }
        public string IpAddress { get; set; } = "";
        public int Port { get; set; }

        public string FirmwareVersion { get; set; } = "";
        public string SerialNumber { get; set; } = "";

        public int UserCount { get; set; }
        public int FingerprintCount { get; set; }
        public int AttendanceLogCount { get; set; }

        public DateTime DeviceTime { get; set; }

        public bool TimeSynced { get; set; }

        public string ErrorMessage { get; set; } = "";
    }
}
