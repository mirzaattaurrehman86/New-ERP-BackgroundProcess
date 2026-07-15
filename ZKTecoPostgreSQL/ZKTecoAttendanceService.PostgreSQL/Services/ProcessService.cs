using ZKTecoAttendanceService.Dto;
using ZKTecoAttendanceService.Infrastructure;

namespace ZKTecoAttendanceService.PostgreSQL.Services
{
    public class ProcessService
    {
        private DatabaseContext dbContext;
        private AttendanceService repo;
        public ProcessService()
        {
            dbContext = new DatabaseContext();
            repo = new AttendanceService();
        }
        public async Task loadAllPunchesAsync()
        {
            var Records = await repo.GetAllPunchesAsync();

            Console.WriteLine($"ALL PUNCHES COUNT: {Records.Count}");

            List<EmployeeAttendanceDto> attendanceLogs = Records.Select(r => new EmployeeAttendanceDto
            {
                EmployeeCode = r.EmployeeCode,
                DateTimeStamp = r.DateTimeStamp,
                StatusId = r.StatusId,
                StatusName = r.StatusName,
                VerifyModeId = r.VerifyModeId,
                VerifyModeName = r.VerifyModeName,

                Office = AttendanceDeviceOffice.All,
                MachineIP = "0.0.0.0",
                MachinePort = "0",
                WorkCodeUniqueTransactionId = 0
            }).ToList();

            Console.WriteLine($"ALL ATTENDANCE LOGS COUNT: {attendanceLogs.Count}");

            dbContext.SaveAttendanceRecord(attendanceLogs, AttendanceDeviceOffice.All);

            Console.WriteLine("************ ..::.. ATTENDANCE LOGS SAVED ..::.. ************");
        }
        public async Task loadCurrentMonthPunchesAsync()
        {
            var Records = await repo.GetCurrentMonthPunchesAsync();

            Console.WriteLine($"CURRENT MONTH RECORDS COUNT: {Records.Count}");

            List<EmployeeAttendanceDto> attendanceLogs = Records.Select(r => new EmployeeAttendanceDto
            {
                EmployeeCode = r.EmployeeCode,
                DateTimeStamp = r.DateTimeStamp,
                StatusId = r.StatusId,
                StatusName = r.StatusName,
                VerifyModeId = r.VerifyModeId,
                VerifyModeName = r.VerifyModeName,

                Office = AttendanceDeviceOffice.All,
                MachineIP = "0.0.0.0",
                MachinePort = "0",
                WorkCodeUniqueTransactionId = 0
            }).ToList();

            Console.WriteLine($"CURRENT MONTH ATTENDANCE LOGS COUNT: {attendanceLogs.Count}");

            dbContext.SaveAttendanceRecord(attendanceLogs, AttendanceDeviceOffice.All);

            Console.WriteLine("************ ..::.. ATTENDANCE LOGS SAVED ..::.. ************");
        }
        public async Task loadCurrentYearPunchesAsync()
        {
            var Records = await repo.GetCurrentYearPunchesAsync();

            Console.WriteLine($"CURRENT YEAR RECORDS COUNT: {Records.Count}");

            List<EmployeeAttendanceDto> attendanceLogs = Records.Select(r => new EmployeeAttendanceDto
            {
                EmployeeCode = r.EmployeeCode,
                DateTimeStamp = r.DateTimeStamp,
                StatusId = r.StatusId,
                StatusName = r.StatusName,
                VerifyModeId = r.VerifyModeId,
                VerifyModeName = r.VerifyModeName,

                Office = AttendanceDeviceOffice.All,
                MachineIP = "0.0.0.0",
                MachinePort = "0",
                WorkCodeUniqueTransactionId = 0
            }).ToList();

            Console.WriteLine($"CURRENT YEAR ATTENDANCE LOGS COUNT: {attendanceLogs.Count}");

            dbContext.SaveAttendanceRecord(attendanceLogs, AttendanceDeviceOffice.All);

            Console.WriteLine("************ ..::.. ATTENDANCE LOGS SAVED ..::.. ************");
        }
        public async Task loadCurrentAndPreviousYearPunchesAsync()
        {
            var Records = await repo.GetCurrentAndPreviousYearPunchesAsync();

            Console.WriteLine($"CURRENT AND PREVIOUS YEAR RECORDS COUNT: {Records.Count}");

            List<EmployeeAttendanceDto> attendanceLogs = Records.Select(r => new EmployeeAttendanceDto
            {
                EmployeeCode = r.EmployeeCode,
                DateTimeStamp = r.DateTimeStamp,
                StatusId = r.StatusId,
                StatusName = r.StatusName,
                VerifyModeId = r.VerifyModeId,
                VerifyModeName = r.VerifyModeName,

                Office = AttendanceDeviceOffice.All,
                MachineIP = "0.0.0.0",
                MachinePort = "0",
                WorkCodeUniqueTransactionId = 0
            }).ToList();

            Console.WriteLine($"CURRENT AND PREVIOUS YEAR ATTENDANCE LOGS COUNT: {attendanceLogs.Count}");

            dbContext.SaveAttendanceRecord(attendanceLogs, AttendanceDeviceOffice.All);

            Console.WriteLine("************ ..::.. ATTENDANCE LOGS SAVED ..::.. ************");
        }
        public async Task loadPunchesByDateRangeAsync(DateOnly startDate, DateOnly endDate)
        {
            var Records = await repo.GetPunchesByDateRangeAsync(startDate, endDate);

            Console.WriteLine($"PUNCHES BY DATE RANGE RECORDS COUNT: {Records.Count}");

            List<EmployeeAttendanceDto> attendanceLogs = Records.Select(r => new EmployeeAttendanceDto
            {
                EmployeeCode = r.EmployeeCode,
                DateTimeStamp = r.DateTimeStamp,
                StatusId = r.StatusId,
                StatusName = r.StatusName,
                VerifyModeId = r.VerifyModeId,
                VerifyModeName = r.VerifyModeName,

                Office = AttendanceDeviceOffice.All,
                MachineIP = "0.0.0.0",
                MachinePort = "0",
                WorkCodeUniqueTransactionId = 0
            }).ToList();

            Console.WriteLine($"PUNCHES BY DATE RANGE ATTENDANCE LOGS COUNT: {attendanceLogs.Count}");

            dbContext.SaveAttendanceRecord(attendanceLogs, AttendanceDeviceOffice.All);

            Console.WriteLine("************ ..::.. ATTENDANCE LOGS SAVED ..::.. ************");
        }
        public async Task loadAllPunchesByEmployeeAsync(string employeeCode)
        {
            var Records = await repo.GetAllPunchesByEmployeeAsync(employeeCode);

            Console.WriteLine($"ALL PUNCHES BY EMPLOYEE RECORDS COUNT: {Records.Count}");

            List<EmployeeAttendanceDto> attendanceLogs = Records.Select(r => new EmployeeAttendanceDto
            {
                EmployeeCode = r.EmployeeCode,
                DateTimeStamp = r.DateTimeStamp,
                StatusId = r.StatusId,
                StatusName = r.StatusName,
                VerifyModeId = r.VerifyModeId,
                VerifyModeName = r.VerifyModeName,

                Office = AttendanceDeviceOffice.All,
                MachineIP = "0.0.0.0",
                MachinePort = "0",
                WorkCodeUniqueTransactionId = 0
            }).ToList();

            Console.WriteLine($"ALL PUNCHES BY EMPLOYEE ATTENDANCE LOGS COUNT: {attendanceLogs.Count}");

            dbContext.SaveAttendanceRecord(attendanceLogs, AttendanceDeviceOffice.All);

            Console.WriteLine("************ ..::.. ATTENDANCE LOGS SAVED ..::.. ************");
        }
        public async Task loadCurrentMonthPunchesByEmployeeAsync(string employeeCode)
        {
            var Records = await repo.GetCurrentMonthPunchesByEmployeeAsync(employeeCode);

            Console.WriteLine($"GET CURRENT MONTH PUNCHES BY EMPLOYEE RECORDS COUNT: {Records.Count}");

            List<EmployeeAttendanceDto> attendanceLogs = Records.Select(r => new EmployeeAttendanceDto
            {
                EmployeeCode = r.EmployeeCode,
                DateTimeStamp = r.DateTimeStamp,
                StatusId = r.StatusId,
                StatusName = r.StatusName,
                VerifyModeId = r.VerifyModeId,
                VerifyModeName = r.VerifyModeName,

                Office = AttendanceDeviceOffice.All,
                MachineIP = "0.0.0.0",
                MachinePort = "0",
                WorkCodeUniqueTransactionId = 0
            }).ToList();

            Console.WriteLine($"GET CURRENT MONTH PUNCHES BY EMPLOYEE ATTENDANCE LOGS COUNT: {attendanceLogs.Count}");

            dbContext.SaveAttendanceRecord(attendanceLogs, AttendanceDeviceOffice.All);

            Console.WriteLine("************ ..::.. ATTENDANCE LOGS SAVED ..::.. ************");
        }
        public async Task loadEmployeePunchesByDateRangeAsync(string employeeCode, DateOnly startDate, DateOnly endDate)
        {
            var Records = await repo.GetEmployeePunchesByDateRangeAsync(employeeCode, startDate, endDate);

            Console.WriteLine($"GET EMPLOYEE PUNCHES BY DATE RANGE RECORDS COUNT: {Records.Count}");

            List<EmployeeAttendanceDto> attendanceLogs = Records.Select(r => new EmployeeAttendanceDto
            {
                EmployeeCode = r.EmployeeCode,
                DateTimeStamp = r.DateTimeStamp,
                StatusId = r.StatusId,
                StatusName = r.StatusName,
                VerifyModeId = r.VerifyModeId,
                VerifyModeName = r.VerifyModeName,

                Office = AttendanceDeviceOffice.All,
                MachineIP = "0.0.0.0",
                MachinePort = "0",
                WorkCodeUniqueTransactionId = 0
            }).ToList();

            Console.WriteLine($"GET EMPLOYEE PUNCHES BY DATE RANGE ATTENDANCE LOGS COUNT: {attendanceLogs.Count}");

            dbContext.SaveAttendanceRecord(attendanceLogs, AttendanceDeviceOffice.All);

            Console.WriteLine("************ ..::.. ATTENDANCE LOGS SAVED ..::.. ************");
        }
    }
}
