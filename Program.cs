using ClosedXML.Excel;
using System.Diagnostics;
using System.Text.Json;
using ZKattendanceTestProject.Dto;
using ZKattendanceTestProject.helper;
using ZKattendanceTestProject.Infrastructure;
using zkemkeeper;
using ZKTecoAttendanceService.Dto;
using ZKTecoAttendanceService.Services.AttendanceDeviceService;
using ZKTecoAttendanceService.Services.ProcessFlow;
using ZKTecoAttendanceService.Services.ProcessLock;

//class Program
class ConsoleProgram
{
    private static readonly ProcessFlowService processFlowService = new ProcessFlowService();
    private static CancellationTokenSource? _cts;
    static async Task Main(string[] args)
    {
        bool checkAcquire = true;
        try
        {
            await processFlowService.LogInfo(AttendanceDeviceOffice.All, "", "", DateTime.Now, "************ ..::.. Request Received ..::.. ************");

            AttendanceServiceRequestDto dto = await processFlowService.BuildRequestAsync(args);

            string requestDetails = $"Request Details => Office:{dto.office}, MachineIp:{dto.machineIp}, EmployeeEnrollNumber:{dto.employeeEnrollNumber}, IsProcessAttendance:{dto.isProcessAttendance}, IsRestartMachine:{dto.isRestartMachine}, IsClearDeviceLogs:{dto.isClearDeviceLogs}, IsRemoveEmployeeRegistration:{dto.isRemoveEmployeeRegistration}";

            Console.WriteLine(requestDetails);

            await processFlowService.LogInfo(AttendanceDeviceOffice.All, "", "", DateTime.Now, requestDetails);

            _cts = new CancellationTokenSource();

            await processFlowService.ExecuteRequestAsync(dto, _cts.Token, checkAcquire);
        }
        catch (Exception ex)
        {
            string message = $"Fatal Exception => {ex}";

            Console.WriteLine(message);

            await processFlowService.LogException(AttendanceDeviceOffice.All, "", "", DateTime.Now, message);

            throw;
        }
    }
}