using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace ZKTecoAttendanceService.Dto
{
    public class EmployeeAttendanceDto
    {
        public string MachineIP { get; set; } = null!;
        public string MachinePort { get; set; } = null!;
        public string EmployeeCode { get; set; } = null!;
        public DateTime DateTimeStamp { get; set; }
        public int StatusId { get; set; }
        public string StatusName { get; set; } = null!;
        public int VerifyModeId { get; set; }
        public string VerifyModeName { get; set; } = null!;
        public int WorkCodeUniqueTransactionId { get; set; }


        public AttendanceDeviceOffice Office { get; set; }
    }
}
