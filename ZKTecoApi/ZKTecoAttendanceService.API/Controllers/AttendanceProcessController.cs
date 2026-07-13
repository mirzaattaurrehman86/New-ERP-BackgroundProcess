using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using ZKattendanceTestProject.Infrastructure;
using ZKTecoAttendanceService.Dto;
using ZKTecoAttendanceService.Services.ProcessFlow;
using ZKTecoAttendanceService.Services.ProcessLock;

namespace ZKTecoAttendanceService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttendanceProcessController : ControllerBase
    {
        private static CancellationTokenSource? _cts;
        private static AttendanceProcessLock? processLock;
        [HttpPost("run")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<AttendanceServiceRequestDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RunAttendance([FromForm] AttendanceServiceRequestDto dto)
        {
            bool checkAcquire = true;

            if (dto == null)
                throw new BadRequestException("request is null.");

            if (dto.isProcessAttendance && dto.office != null && dto.machineIp != null)
                throw new BadRequestException("When processing attendance, you must choose a single scope: either provide an office to process attendance for all machines within that office, or provide a machine IP address to process attendance for a specific machine. Do not provide both.");

            if (dto.isProcessAttendance && dto.office == null && dto.machineIp == null)
                throw new BadRequestException("For attendance processing, either an office or a machine IP address must be provided.");

            //if (dto.isProcessAttendance)
            //    if (!await _unitOfWork.AttendanceDevicesInfos.AnyAsync(x => x.IsActive))
            //        throw new BadRequestException("There are no active attendance devices, please activate at least one device.");

            if (dto.isRestartMachine && dto.office != null && dto.machineIp != null)
                throw new BadRequestException("When restarting machines, provide either an office or a machine IP address, but not both.");

            if (dto.isRestartMachine && dto.office == null && dto.machineIp == null)
                throw new BadRequestException("When restarting machines, either an office or a machine IP address must be provided.");

            if (dto.isClearDeviceLogs && dto.office != null && dto.machineIp != null)
                throw new BadRequestException("When clear machines logs, provide either an office or a machine IP address, but not both.");

            if (dto.isClearDeviceLogs && dto.office == null && dto.machineIp == null)
                throw new BadRequestException("When clear machines logs, either an office or a machine IP address must be provided.");

            if (dto.isRemoveEmployeeRegistration && string.IsNullOrEmpty(dto.employeeEnrollNumber))
                throw new BadRequestException("To remove an employee registration, a valid employee enrollment number must be provided.");

            if (dto.isClearDeviceLogs)
                throw new BadRequestException("Currently clear device logs operation not tested, So, it's bolocked for time-being.");

            if (dto.isRemoveEmployeeRegistration)
                throw new BadRequestException("Currently employee registration removal operation not tested, So, it's bolocked for time-being.");


            ProcessFlowService processFlowService = new ProcessFlowService();

            var db = new DatabaseContext();

            processLock = new AttendanceProcessLock(db.connectionString);

            if (!processLock.Acquire())
            {
                string message = "Attendance synchronization already running.";

                Console.WriteLine(message);

                await processFlowService.LogError(dto.office ?? AttendanceDeviceOffice.All, "", "", DateTime.Now, message);

                throw new BadRequestException("Attendance synchronization already running.");
            }

            checkAcquire = false;
            _cts = new CancellationTokenSource();

            _ = Task.Run(async () =>
            {
                try
                {
                    await processFlowService.LogInfo(AttendanceDeviceOffice.All, "", "", DateTime.Now, "************ ..::.. Request Received ..::.. ************");

                    string requestDetails = $"Request Details => Office:{dto.office}, MachineIp:{dto.machineIp}, EmployeeEnrollNumber:{dto.employeeEnrollNumber}, IsProcessAttendance:{dto.isProcessAttendance}, IsRestartMachine:{dto.isRestartMachine}, IsClearDeviceLogs:{dto.isClearDeviceLogs}, IsRemoveEmployeeRegistration:{dto.isRemoveEmployeeRegistration}";

                    await processFlowService.LogInfo(AttendanceDeviceOffice.All, "", "", DateTime.Now, requestDetails);

                    await processFlowService.ExecuteRequestAsync(dto, _cts.Token, checkAcquire);
                }
                catch (Exception ex)
                {
                    string message = $"Fatal Exception => {ex}";

                    Console.WriteLine(message);

                    await processFlowService.LogException(AttendanceDeviceOffice.All, "", "", DateTime.Now, message);
                }
            });

            return Ok(ApiResponse<string>.SuccessResponse("Started", "Attendance process started successfully."));
        }


        [HttpPost("stop")]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public IActionResult StopAttendance()
        {
            var killed = 0;

            foreach (var process in Process.GetProcessesByName("ZKTecoAttendanceService"))
            {
                try
                {
                    process.Kill(true);
                    killed++;
                }
                catch (Exception ex)
                {
                }
            }

            if (_cts == null && killed <= 0)
                throw new BadRequestException("No attendance process is running.");

            _cts?.Cancel();

            return Ok(ApiResponse<string>.SuccessResponse("stopped", "Attendance service stopped."));
        }
    }
}
