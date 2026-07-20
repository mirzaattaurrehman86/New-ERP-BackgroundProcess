namespace ZKTecoAttendanceService.DTO.Dto.PostgresSQL
{
    public class AttendanceRecordDto
    {
        public string EmployeeCode { get; set; } = "";

        public DateTime DateTimeStamp { get; set; }

        public DateOnly DateStamp { get; set; }

        public TimeOnly TimeStamp { get; set; }

        public int StatusId { get; set; }

        public string StatusName { get; set; } = "";

        public int VerifyModeId { get; set; }

        public string VerifyModeName { get; set; } = "";

        public string DeviceOffice { get; set; } = "";

        public string DeviceOfficeName { get; set; } = "";

        public string DeviceName { get; set; } = "";
    }
}
