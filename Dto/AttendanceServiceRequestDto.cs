using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ZKTecoAttendanceService.Dto
{
    public class AttendanceServiceRequestDto 
    {
        public AttendanceDeviceOffice? office { get; set; }
        public string? machineIp { get; set; }
        public string? employeeEnrollNumber { get; set; }


        public bool isProcessAttendance { get; set; } = false;
        public bool isRestartMachine { get; set; } = false;
        public bool isClearDeviceLogs { get; set; } = false;
        public bool isRemoveEmployeeRegistration { get; set; } = false;
        
    }
}
