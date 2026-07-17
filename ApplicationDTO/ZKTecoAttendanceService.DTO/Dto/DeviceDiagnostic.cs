namespace ZKTecoAttendanceService.DTO.Dto
{
    public class DeviceDiagnostic
    {
        public bool IsOnline { get; set; }
        public string FirmwareVersion { get; set; } = "";
        public string SerialNumber { get; set; } = "";
        public int UserCount { get; set; }
        public int FingerprintCount { get; set; }
        public int LogCount { get; set; }
        public int UserCapacity { get; set; }
        public int FingerCapacity { get; set; }
        public int LogCapacity { get; set; }
        public string DiagnosticMessage { get; set; } = "";
    }
}
