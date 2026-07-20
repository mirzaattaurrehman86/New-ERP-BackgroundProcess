namespace ZKTecoAttendanceService.DTO.Dto
{
    public class AttendanceDeviceDto
    {
        public int deviceOrder { get; set; }

        public string? name { get; set; }
        public string ip { get; set; } = null!;
        public int port { get; set; }
        public string serialNumber { get; set; } = null!;
        public int deviceNumber { get; set; }

        public AttendanceDeviceOffice office { get; set; }

        public string? DeviceFirstnamePostgre { get; set; }
        public string? DeviceSecondnamePostgre { get; set; }


        public bool? IsActive { get; set; }
    }
}
