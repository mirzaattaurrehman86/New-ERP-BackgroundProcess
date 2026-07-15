using ClosedXML.Excel;
using System.Diagnostics;
using System.Text.Json;
using zkemkeeper;
using ZKTecoAttendanceService.Dto;
using ZKTecoAttendanceService.helper;
using ZKTecoAttendanceService.Infrastructure;
using ZKTecoAttendanceService.Services.ProcessLock;

namespace ZKTecoAttendanceService.Services.ProcessFlow
{
    public class ProcessFlowService
    {
        private const int MachineNumber = 1;
        private const int TransactionCode = 6;
        // Watchdog settings
        private static DateTime _lastActivity = DateTime.UtcNow;
        private static Timer? _watchdogTimer;
        private static AttendanceProcessLock? processLock;
        public async Task<AttendanceServiceRequestDto> BuildRequestAsync(string[] args)
        {
            if (args.Length == 0)
            {
                await LogInfo(AttendanceDeviceOffice.All, "", "", DateTime.Now, "No arguments supplied. Running default attendance process.");

                return new AttendanceServiceRequestDto
                {
                    office = AttendanceDeviceOffice.All,
                    isProcessAttendance = true
                };
            }

            string jsonArgs = JsonSerializer.Serialize(args);

            Console.WriteLine($"Args Received: {jsonArgs}");

            await LogInfo(AttendanceDeviceOffice.All, "", "", DateTime.Now, $"Args Received: {jsonArgs}");

            AttendanceServiceRequestDto? dto = JsonSerializer.Deserialize<AttendanceServiceRequestDto>(args[0]);

            if (dto == null)
            {
                string message = "Request deserialized as null.";

                Console.WriteLine(message);

                await LogError(AttendanceDeviceOffice.All, "", "", DateTime.Now, message);

                throw new Exception(message);
            }

            return dto;
        }
        public async Task<bool> appLocked(AttendanceServiceRequestDto dto, CancellationToken token, bool checkAcquire = true)
        {
            var db = new DatabaseContext();

            processLock = new AttendanceProcessLock(db.connectionString);

            if (checkAcquire)
            {
                if (!processLock.Acquire())
                {
                    string message = "Attendance synchronization already running.";

                    Console.WriteLine(message);

                    await LogError(dto.office ?? AttendanceDeviceOffice.All, "", "", DateTime.Now, message);

                    return true;
                }
            }

            return false;
        }
        public async Task ExecuteRequestAsync(AttendanceServiceRequestDto dto, CancellationToken token, bool checkAcquire = true)
        {
            await LogInfo(AttendanceDeviceOffice.All, "", "", DateTime.Now, "************ ..::.. Process Started ..::.. ************");

            var db = new DatabaseContext();

            processLock = new AttendanceProcessLock(db.connectionString);

            if (checkAcquire)
            {
                if (await appLocked(dto, token))
                {
                    return;
                }
            }
            try
            {
                await Touch("Process Started");

                await StartWatchdog();

                if (dto.isProcessAttendance)
                {
                    await LogInfo(dto.office ?? AttendanceDeviceOffice.All, dto.machineIp ?? "", "", DateTime.Now, "Running attendance process.");

                    await RunAttendanceProcess(token, dto.office);

                    string message = $"Finished attendance process.";
                    await LogInfo(dto.office ?? AttendanceDeviceOffice.All, dto.machineIp ?? "", "", DateTime.Now, message);

                    Console.WriteLine(message);

                    return;
                }

                if (dto.isRestartMachine)
                {
                    await LogInfo(dto.office ?? AttendanceDeviceOffice.All, dto.machineIp ?? "", "", DateTime.Now, "Running restart machine operation.");

                    await RestartDeviceAsync(dto.office, dto.machineIp, token);

                    string message = $"Finished restart machine operation.";
                    await LogInfo(dto.office ?? AttendanceDeviceOffice.All, dto.machineIp ?? "", "", DateTime.Now, message);
                    Console.WriteLine(message);

                    return;
                }

                if (dto.isClearDeviceLogs)
                {
                    await LogInfo(dto.office ?? AttendanceDeviceOffice.All, dto.machineIp ?? "", "", DateTime.Now, "Running clear device logs operation.");

                    await LogInfo(dto.office ?? AttendanceDeviceOffice.All, dto.machineIp ?? "", "", DateTime.Now, "Currently clear device logs operation not tested, So, it's bolocked for time-being.");
                    return;
                    await ClearDeviceLogsAsync(dto.office, dto.machineIp, token);

                    string message = $"Finished clear device logs operation.";
                    await LogInfo(dto.office ?? AttendanceDeviceOffice.All, dto.machineIp ?? "", "", DateTime.Now, message);
                    Console.WriteLine(message);

                    return;
                }

                if (dto.isRemoveEmployeeRegistration)
                {
                    await LogInfo(dto.office ?? AttendanceDeviceOffice.All, "", dto.employeeEnrollNumber ?? "", DateTime.Now, "Running employee registration removal operation.");

                    await LogInfo(dto.office ?? AttendanceDeviceOffice.All, dto.machineIp ?? "", "", DateTime.Now, "Currently employee registration removal operation not tested, So, it's bolocked for time-being.");
                    return;

                    await RemoveEmployeeRegistrationFromAllDevicesAsync(dto.employeeEnrollNumber!, true, token);

                    string message = $"Finished employee registration removal operation.";
                    await LogInfo(dto.office ?? AttendanceDeviceOffice.All, dto.machineIp ?? "", "", DateTime.Now, message);
                    Console.WriteLine(message);

                    return;
                }

                var DeviceHealth = CheckDeviceHealth("192.168.36.203", 4370, 1); // this function is used to get health of the device

                var DeviceDiagnostic = RunDeviceDiagnostic("192.168.36.203", 4370, 1); // this function is for to get Device Diagnostic

                await LogError(dto.office ?? AttendanceDeviceOffice.All, "", "", DateTime.Now, "No valid operation selected.");
            }
            catch (Exception ex)
            {
                await LogException(dto.office ?? AttendanceDeviceOffice.All, dto.machineIp ?? "", dto.employeeEnrollNumber ?? "", DateTime.Now, $"Process Exception: {ex}");
            }
            finally
            {
                StopWatchdog();

                processLock.Release();

                processLock.releaseAppLock();
            }
        }
        public async Task RunAttendanceProcess(CancellationToken token, AttendanceDeviceOffice? selectedOffice = null)
        {
            var db = new DatabaseContext();

            AttendanceDeviceOffice deviceOffice = new AttendanceDeviceOffice();
            string ip = string.Empty;
            int port = 0;
            CZKEM zk = new CZKEM();
            try
            {
                List<AttendanceDeviceDto> devices = (await db.GetAttendanceDevicesAsync())/*.Where(x => x.office == AttendanceDeviceOffice.HeadOffice)*/.ToList();
                token.ThrowIfCancellationRequested();

                if (selectedOffice != null && selectedOffice != AttendanceDeviceOffice.All)
                {
                    token.ThrowIfCancellationRequested();
                    devices = devices.Where(x => x.office == selectedOffice.Value).ToList();


                    await LogInfo(selectedOffice.Value, "", "", DateTime.Now, $"Running for office: {selectedOffice.Value}");
                    token.ThrowIfCancellationRequested();
                }

                if (devices.Count() > 0)
                {
                    foreach (var device in devices)
                    {
                        await Touch($"Starting device {device.ip}");
                        token.ThrowIfCancellationRequested();

                        zk = new CZKEM();
                        token.ThrowIfCancellationRequested();

                        deviceOffice = device.office;
                        ip = device.ip;
                        port = device.port;
                        token.ThrowIfCancellationRequested();

                        await LogInfo(deviceOffice, ip, port.ToString(), DateTime.Now, $" ..::.. ...Starting connection... ..::.. ");
                        token.ThrowIfCancellationRequested();

                        if (!(await ConnectDevice(zk, ip, port, deviceOffice)))
                        {
                            zk.Disconnect();

                            token.ThrowIfCancellationRequested();
                            continue;
                        }

                        await Touch($"Connected {device.ip}");
                        token.ThrowIfCancellationRequested();

                        var attendance = await FetchAttendance(zk, ip, port, deviceOffice);
                        await Touch($"Attendance loaded {attendance.Count}");
                        token.ThrowIfCancellationRequested();

                        if (attendance.Any())
                        {
                            token.ThrowIfCancellationRequested();
                            //PrintAttendance(attendance, deviceOffice);
                            //PrintAttendanceInExcel(attendance, deviceOffice);

                            db.SaveAttendanceRecord(attendance, deviceOffice);
                            await Touch($"Attendance saved {attendance.Count}");
                            await Log(deviceOffice, ip, port.ToString(), DateTime.Now, $"Total attendance saved: {attendance.Count}");
                            token.ThrowIfCancellationRequested();
                        }
                        else
                        {
                            token.ThrowIfCancellationRequested();
                            await LogError(deviceOffice, ip, port.ToString(), DateTime.Now, $"No attendance found.");
                        }
                        zk.Disconnect();

                        token.ThrowIfCancellationRequested();
                        await Touch($"Disconnected {device.ip}");
                        await Log(deviceOffice, ip, port.ToString(), DateTime.Now, $"Disconnected successfully.");
                    }

                    //db.RunAttendanceSync();
                    token.ThrowIfCancellationRequested();
                    await LogInfo(selectedOffice ?? AttendanceDeviceOffice.All, "", "", DateTime.Now, $"ProcessMonthlyAttendance SP executed successfully.");
                }

                await LogInfo(deviceOffice, ip, port.ToString(), DateTime.Now, "Application finished.");
            }
            catch (Exception ex)
            {
                StopWatchdog();
                //processLock?.Release();
                zk.Disconnect();
                await LogException(deviceOffice, ip, port.ToString(), DateTime.Now, $"Exception: {ex}");

                //if (ex.Message.Trim().ToLower() == "The operation was canceled.".ToLower())
                //{
                //    throw new Exception(ex.Message);
                //}
            }
            finally
            {
                //processLock?.Release();
            }
        }
        public async Task<bool> ConnectDevice(CZKEM zk, string ip, int port, AttendanceDeviceOffice deviceOffice)
        {
            const int maxRetries = 3;
            //zk.SetCommuTimeOut(5000);

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                await LogInfo(deviceOffice, ip, port.ToString(), DateTime.Now, $"Connection attempt {attempt} of {maxRetries}.");

                if (zk.Connect_Net(ip, port))
                {
                    int logCount = 0;

                    zk.GetDeviceStatus(MachineNumber, TransactionCode, ref logCount);

                    await Log(deviceOffice, ip, port.ToString(), DateTime.Now, $"Connected successfully. Device log count: {logCount}");

                    return true;
                }

                int errorCode = 0;
                zk.GetLastError(ref errorCode);

                await LogError(deviceOffice, ip, port.ToString(), DateTime.Now, $"Connection attempt {attempt} failed. ErrorCode: {errorCode}");

                // Wait before next retry (except after last attempt)
                if (attempt < maxRetries)
                {
                    Thread.Sleep(3000); // wait 3 seconds
                }
            }
            await LogError(deviceOffice, ip, port.ToString(), DateTime.Now, $"All {maxRetries} connection attempts failed.");
            return false;
        }
        public async Task<List<EmployeeAttendanceDto>> FetchAttendance(CZKEM zk, string ip, int port, AttendanceDeviceOffice office)
        {
            var attendance = new List<EmployeeAttendanceDto>();

            if (!zk.ReadGeneralLogData(MachineNumber))
            {
                await LogError(office, ip, port.ToString(), DateTime.Now, $"Failed to read logs from device.");

                return attendance;
            }

            await Log(office, ip, port.ToString(), DateTime.Now, $"Reading logs.");

            string enrollNumber;
            int verifyMode, inOutMode, year, month, day, hour, minute, second;
            int workCode = 0;

            while (zk.SSR_GetGeneralLogData(
                MachineNumber,
                out enrollNumber,
                out verifyMode,
                out inOutMode,
                out year,
                out month,
                out day,
                out hour,
                out minute,
                out second,
                ref workCode))
            {
                try
                {
                    var logTime = new DateTime(year, month, day, hour, minute, second);

                    attendance.Add(new EmployeeAttendanceDto
                    {
                        MachineIP = ip,
                        MachinePort = port.ToString(),
                        EmployeeCode = enrollNumber,
                        StatusId = inOutMode,
                        StatusName = HelperClass.MapInOutMode(inOutMode),
                        VerifyModeId = verifyMode,
                        VerifyModeName = HelperClass.MapVerifyMode(verifyMode),
                        DateTimeStamp = logTime,
                        WorkCodeUniqueTransactionId = workCode,
                        Office = office
                    });
                }
                catch (Exception ex)
                {
                    await LogException(office, ip, port.ToString(), DateTime.Now, $"Invalid log skipped. Error: {ex.Message}");
                }
            }

            await Log(office, ip, port.ToString(), DateTime.Now, $"Total loaded attendance logs: {attendance.Count}");

            return attendance;
        }
        public void PrintAttendance(List<EmployeeAttendanceDto> attendance, AttendanceDeviceOffice deviceOffice)
        {
            //DateTime fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            //DateTime toDate = fromDate.AddMonths(1);

            DateTime fromDate = DateTime.Today.AddDays(-1); // Yesterday
            DateTime toDate = DateTime.Today;               // Today

            attendance = attendance.Where(x => x.DateTimeStamp >= fromDate && x.DateTimeStamp <= toDate).ToList();

            string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

            Directory.CreateDirectory(folderPath);

            string filePath = Path.Combine(folderPath, "attendance_log.txt");

            foreach (var item in attendance.OrderBy(x => x.DateTimeStamp))
            {
                Console.WriteLine(
                    $"Employee-ID: {item.EmployeeCode} | " +
                    $"Time: {item.DateTimeStamp} | " +
                    $"Mode: {item.StatusName} ({item.StatusId}) | " +
                    $"Verify: {item.VerifyModeName} ({item.VerifyModeId}) | " +
                    $"WorkCode: {item.WorkCodeUniqueTransactionId} | " +
                    $"Office: {deviceOffice}");
            }

            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                foreach (var item in attendance.OrderBy(x => x.DateTimeStamp))
                {
                    writer.WriteLine(
                        $"Office: {deviceOffice} | " +
                        $"MachineIP: {item.MachineIP} | " +
                        $"MachinePort: {item.MachinePort} | " +
                        $"Employee-ID: {item.EmployeeCode} | " +
                        $"Time: {item.DateTimeStamp} | " +
                        $"Mode: {item.StatusName} ({item.StatusId}) | " +
                        $"Verify: {item.VerifyModeName} ({item.VerifyModeId}) | " +
                        $"WorkCode: {item.WorkCodeUniqueTransactionId}");
                }
            }
        }
        public void PrintAttendanceInExcel(List<EmployeeAttendanceDto> attendance, AttendanceDeviceOffice deviceOffice)
        {
            // Yesterday -> Now
            DateTime fromDate = DateTime.Today.AddDays(-1);
            DateTime toDate = DateTime.Now;

            //attendance = attendance
            //    .Where(x => x.DateTimeStamp >= fromDate && x.DateTimeStamp <= toDate)
            //    .OrderBy(x => x.DateTimeStamp)
            //    .ToList();



            attendance = attendance
                                    .Where(x =>
                                           (x.DateTimeStamp.Date == new DateTime(2026, 5, 26) ||
                                            x.DateTimeStamp.Date == new DateTime(2026, 5, 28) ||
                                            x.DateTimeStamp.Date == new DateTime(2026, 5, 29)))
                                    .OrderBy(x => x.DateTimeStamp)
                                    .ToList();




            string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

            Directory.CreateDirectory(folderPath);

            // Excel file name
            string filePath = Path.Combine(
                folderPath,
                $"attendance_log_{deviceOffice.ToString()}_{DateTime.Now:yyyyMMdd}.xlsx");

            XLWorkbook workbook;
            IXLWorksheet worksheet;

            // If file exists → append data
            if (File.Exists(filePath))
            {
                workbook = new XLWorkbook(filePath);

                worksheet = workbook.Worksheet("Attendance");
            }
            else
            {
                workbook = new XLWorkbook();

                worksheet = workbook.Worksheets.Add("Attendance");

                // Headers
                worksheet.Cell(1, 1).Value = "Office";
                worksheet.Cell(1, 2).Value = "Machine IP";
                worksheet.Cell(1, 3).Value = "Machine Port";
                worksheet.Cell(1, 4).Value = "Employee ID";
                worksheet.Cell(1, 5).Value = "Date Time";
                worksheet.Cell(1, 6).Value = "Status";
                worksheet.Cell(1, 7).Value = "Status ID";
                worksheet.Cell(1, 8).Value = "Verify Mode";
                worksheet.Cell(1, 9).Value = "Verify Mode ID";
                worksheet.Cell(1, 10).Value = "WorkCode";

                // Header Style
                var headerRange = worksheet.Range(1, 1, 1, 10);

                headerRange.Style.Font.Bold = true;
            }

            // Find next empty row
            int lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 1;

            int row = lastRow + 1;

            foreach (var item in attendance)
            {
                worksheet.Cell(row, 1).Value = deviceOffice.ToString();
                worksheet.Cell(row, 2).Value = item.MachineIP;
                worksheet.Cell(row, 3).Value = item.MachinePort;
                worksheet.Cell(row, 4).Value = item.EmployeeCode;
                worksheet.Cell(row, 5).Value = item.DateTimeStamp;
                worksheet.Cell(row, 6).Value = item.StatusName;
                worksheet.Cell(row, 7).Value = item.StatusId;
                worksheet.Cell(row, 8).Value = item.VerifyModeName;
                worksheet.Cell(row, 9).Value = item.VerifyModeId;
                worksheet.Cell(row, 10).Value = item.WorkCodeUniqueTransactionId;

                row++;

                Console.WriteLine(
                    $"Employee-ID: {item.EmployeeCode} | " +
                    $"Time: {item.DateTimeStamp} | " +
                    $"Mode: {item.StatusName} ({item.StatusId}) | " +
                    $"Verify: {item.VerifyModeName} ({item.VerifyModeId}) | " +
                    $"WorkCode: {item.WorkCodeUniqueTransactionId} | " +
                    $"Office: {deviceOffice}");
            }

            // Auto fit columns
            worksheet.Columns().AdjustToContents();

            // Save file
            workbook.SaveAs(filePath);

            workbook.Dispose();

            Console.WriteLine($"Excel log saved: {filePath}");
        }
        public async Task Log(AttendanceDeviceOffice deviceOffice, string machineIP, string machinePort, DateTime dateTimeStamp, string message)
        {
            await Touch();

            var db = new DatabaseContext();

            db.SaveAttendanceBackgroundServiceLog("[OK]", deviceOffice, machineIP, machinePort, dateTimeStamp, message);

            Console.WriteLine($"[OK] - Office:{deviceOffice} - MachineIP:{machineIP} - " + $"MachinePort:{machinePort} - DateTimeStamp:{dateTimeStamp} - Message:{message}\n");
        }
        public async Task LogError(AttendanceDeviceOffice deviceOffice, string machineIP, string machinePort, DateTime dateTimeStamp, string message)
        {
            await Touch();

            var db = new DatabaseContext();

            db.SaveAttendanceBackgroundServiceLog("[ERROR]", deviceOffice, machineIP, machinePort, dateTimeStamp, message);

            Console.WriteLine($"[ERROR] - Office:{deviceOffice} - MachineIP:{machineIP} - " + $"MachinePort:{machinePort} - DateTimeStamp:{dateTimeStamp} - Message:{message}\n");
        }
        public async Task LogException(AttendanceDeviceOffice deviceOffice, string machineIP, string machinePort, DateTime dateTimeStamp, string message)
        {
            await Touch();

            var db = new DatabaseContext();

            db.SaveAttendanceBackgroundServiceLog("[Exception]", deviceOffice, machineIP, machinePort, dateTimeStamp, message);

            Console.WriteLine($"[Exception] - Office:{deviceOffice} - MachineIP:{machineIP} - " + $"MachinePort:{machinePort} - DateTimeStamp:{dateTimeStamp} - Message:{message}\n");
        }
        public async Task LogInfo(AttendanceDeviceOffice deviceOffice, string machineIP, string machinePort, DateTime dateTimeStamp, string message)
        {
            await Touch();

            var db = new DatabaseContext();

            db.SaveAttendanceBackgroundServiceLog("[Info]", deviceOffice, machineIP, machinePort, dateTimeStamp, message);

            Console.WriteLine($"[Info] - Office:{deviceOffice} - MachineIP:{machineIP} - " + $"MachinePort:{machinePort} - DateTimeStamp:{dateTimeStamp} - Message:{message}\n");
        }
        public async Task testconnection()
        {

            CZKEM objCZKEM = new CZKEM();

            // 1. Define your local Virtual COM Port index (e.g., if it is COM3, pass 3)
            int comPortNumber = 3;

            // 2. Define the Device ID (Machine Number)
            // CRITICAL: In serial daisy-chains, every machine has an ID (Default is usually 1)
            int machineNumber = 1;

            // 3. Define the Baud Rate
            // ZKTeco hardware defaults to 115200 bps for modern serial execution. 
            // If that fails, the fallback legacy speeds are 38400 or 9600.
            int baudRate = 115200;

            Console.WriteLine($"Attempting serial connection via COM{comPortNumber}...");

            // Connect_Com takes: (ComPort, MachineNumber, BaudRate)
            bool isConnected = objCZKEM.Connect_Com(comPortNumber, machineNumber, baudRate);

            if (isConnected)
            {
                Console.WriteLine("Connected successfully over Serial Bridge!");

                // Read the general log buffer into memory exactly the same way
                if (objCZKEM.ReadAllGLogData(machineNumber))
                {
                    string dwEnrollNumber = "";
                    int dwVerifyMode = 0, dwInOutMode = 0;
                    int dwYear = 0, dwMonth = 0, dwDay = 0, dwHour = 0, dwMinute = 0, dwSecond = 0;
                    int dwWorkCode = 0;

                    while (objCZKEM.SSR_GetGeneralLogData(machineNumber, out dwEnrollNumber, out dwVerifyMode,
                           out dwInOutMode, out dwYear, out dwMonth, out dwDay,
                           out dwHour, out dwMinute, out dwSecond, ref dwWorkCode))
                    {
                        DateTime punchTime = new DateTime(dwYear, dwMonth, dwDay, dwHour, dwMinute, dwSecond);
                        Console.WriteLine($"Employee ID: {dwEnrollNumber} | Punch: {punchTime}");
                    }
                }

                objCZKEM.Disconnect();
            }
            else
            {
                Console.WriteLine("Serial handshake failed. Check port mapping or Machine Number ID.");
            }


        }
        public async Task Touch(string activity = "")
        {
            _lastActivity = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(activity))
            {
                Console.WriteLine($"[HEARTBEAT] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {activity}");
            }
        }
        public async Task StartWatchdog()
        {
            _watchdogTimer = new Timer(async _ =>
            {
                Console.WriteLine(
                    $"Watchdog Check: Idle={(DateTime.UtcNow - _lastActivity).TotalSeconds}s");

                var idleTime = DateTime.UtcNow - _lastActivity;

                if (idleTime > TimeSpan.FromMinutes(30))
                {
                    Console.WriteLine("WATCHDOG FIRING");

                    processLock?.Release();

                    processLock?.releaseAppLock();

                    Process.GetCurrentProcess().Kill();

                    await LogError(AttendanceDeviceOffice.All, string.Empty, string.Empty, DateTime.Now, "WATCHDOG FIRING.");
                }

            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
        }
        public void StopWatchdog()
        {
            _watchdogTimer?.Dispose();
        }
        public async Task RestartDeviceAsync(AttendanceDeviceOffice? officeFilter, string? machineIpFilter, CancellationToken token)
        {
            try
            {
                var db = new DatabaseContext();
                token.ThrowIfCancellationRequested();

                List<AttendanceDeviceDto> devices = (await db.GetAttendanceDevicesAsync())/*.Where(x=>x.office == AttendanceDeviceOffice.ProSoft)*/.ToList();
                token.ThrowIfCancellationRequested();

                if (officeFilter.HasValue && officeFilter.Value != AttendanceDeviceOffice.All)
                {
                    token.ThrowIfCancellationRequested();
                    devices = devices.Where(x => x.office == officeFilter.Value).ToList();

                    token.ThrowIfCancellationRequested();
                    await LogInfo(officeFilter.Value, string.Empty, string.Empty, DateTime.Now, $"Running restart operation for office: {officeFilter.Value}");
                }

                if (!devices.Any())
                {
                    token.ThrowIfCancellationRequested();
                    await LogError(officeFilter ?? AttendanceDeviceOffice.All, string.Empty, string.Empty, DateTime.Now, "No devices found.");

                    return;
                }

                machineIpFilter = machineIpFilter?.Trim();
                token.ThrowIfCancellationRequested();

                if (!string.IsNullOrWhiteSpace(machineIpFilter))
                {
                    devices = devices.Where(x => x.ip.Equals(machineIpFilter, StringComparison.OrdinalIgnoreCase)).ToList();
                    token.ThrowIfCancellationRequested();

                    if (!devices.Any())
                    {
                        token.ThrowIfCancellationRequested();
                        await LogError(officeFilter ?? AttendanceDeviceOffice.All, machineIpFilter, string.Empty, DateTime.Now, $"Machine IP '{machineIpFilter}' is not valid.");

                        return;
                    }
                }

                foreach (var device in devices)
                {
                    token.ThrowIfCancellationRequested();

                    await RestartSingleDeviceAsync(device, token);

                    token.ThrowIfCancellationRequested();
                }

                string successMessage = $"...... FINISHED RESTART DEVICES PROCESS ......";
                Console.WriteLine(successMessage);
                await LogInfo(officeFilter ?? AttendanceDeviceOffice.All, machineIpFilter ?? string.Empty, string.Empty, DateTime.Now, successMessage);
            }
            catch (Exception ex)
            {
                string message = $"Error restarting device(s). Office: {officeFilter?.ToString() ?? "All"}, MachineIp: {machineIpFilter ?? "N/A"}, Exception: {ex.Message}";

                Console.WriteLine(message);

                await LogException(officeFilter ?? AttendanceDeviceOffice.All, machineIpFilter ?? string.Empty, string.Empty, DateTime.Now, message);
            }
        }
        public async Task RestartSingleDeviceAsync(AttendanceDeviceDto device, CancellationToken token)
        {
            const int maxRetries = 3;
            token.ThrowIfCancellationRequested();

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                CZKEM zk = new CZKEM();
                token.ThrowIfCancellationRequested();

                try
                {
                    string infoMessage = $"Attempt {attempt}/{maxRetries}: Connecting to device {device.ip}:{device.port}.";
                    Console.WriteLine(infoMessage);
                    await LogInfo(device.office, device.ip, string.Empty, DateTime.Now, infoMessage);

                    token.ThrowIfCancellationRequested();
                    bool connected = zk.Connect_Net(device.ip, device.port);

                    token.ThrowIfCancellationRequested();
                    if (!connected)
                    {
                        token.ThrowIfCancellationRequested();

                        string message = $"Attempt {attempt}/{maxRetries}: Failed to connect to device {device.ip}:{device.port}.";
                        Console.WriteLine(message);
                        await LogError(device.office, device.ip, string.Empty, DateTime.Now, message);

                        if (attempt < maxRetries)
                            await Task.Delay(2000);

                        token.ThrowIfCancellationRequested();
                        continue;
                    }

                    token.ThrowIfCancellationRequested();
                    infoMessage = $"Attempt {attempt}/{maxRetries}: Sending restart command to {device.ip}:{device.port}.";
                    Console.WriteLine(infoMessage);
                    await LogInfo(device.office, device.ip, string.Empty, DateTime.Now, infoMessage);

                    token.ThrowIfCancellationRequested();
                    bool restarted = zk.RestartDevice(MachineNumber);

                    token.ThrowIfCancellationRequested();
                    if (!restarted)
                    {
                        token.ThrowIfCancellationRequested();
                        string message = $"Attempt {attempt}/{maxRetries}: Restart command failed for device {device.ip}:{device.port}.";
                        Console.WriteLine(message);
                        await LogError(device.office, device.ip, string.Empty, DateTime.Now, message);

                        token.ThrowIfCancellationRequested();
                        if (attempt < maxRetries)
                            await Task.Delay(2000);

                        token.ThrowIfCancellationRequested();
                        continue;
                    }

                    token.ThrowIfCancellationRequested();
                    string successMessage = $"Device restarted successfully: {device.ip}:{device.port}.";
                    Console.WriteLine(successMessage);
                    await LogInfo(device.office, device.ip, string.Empty, DateTime.Now, successMessage);

                    token.ThrowIfCancellationRequested();
                    return;
                }
                catch (Exception ex)
                {
                    string message = $"Attempt {attempt}/{maxRetries}: Error restarting device {device.ip}:{device.port}. Exception: {ex.Message}";
                    Console.WriteLine(message);
                    await LogException(device.office, device.ip, string.Empty, DateTime.Now, message);

                    if (attempt < maxRetries)
                        await Task.Delay(2000);
                }
                finally
                {
                    zk.Disconnect();
                }
            }

            token.ThrowIfCancellationRequested();
            string finalMessage = $"Failed to restart device {device.ip}:{device.port} after {maxRetries} attempts.";
            Console.WriteLine(finalMessage);
            await LogError(device.office, device.ip, string.Empty, DateTime.Now, finalMessage);

            token.ThrowIfCancellationRequested();
        }
        public async Task ClearDeviceLogsAsync(AttendanceDeviceOffice? officeFilter, string? machineIpFilter, CancellationToken token)
        {
            try
            {
                //List<AttendanceDeviceDto> devices = DeviceDataService.GetAllDevices();
                token.ThrowIfCancellationRequested();
                var db = new DatabaseContext();
                List<AttendanceDeviceDto> devices = await db.GetAttendanceDevicesAsync();

                token.ThrowIfCancellationRequested();
                if (officeFilter.HasValue && officeFilter.Value != AttendanceDeviceOffice.All)
                {
                    devices = devices.Where(x => x.office == officeFilter.Value).ToList();
                    token.ThrowIfCancellationRequested();

                    await LogInfo(officeFilter.Value, string.Empty, string.Empty, DateTime.Now, $"Running clear logs operation for office: {officeFilter.Value}");
                }

                if (!devices.Any())
                {
                    token.ThrowIfCancellationRequested();
                    await LogError(officeFilter ?? AttendanceDeviceOffice.All, string.Empty, string.Empty, DateTime.Now, "No devices found.");
                    return;
                }

                machineIpFilter = machineIpFilter?.Trim();
                token.ThrowIfCancellationRequested();

                if (!string.IsNullOrWhiteSpace(machineIpFilter))
                {
                    devices = devices.Where(x => x.ip.Equals(machineIpFilter, StringComparison.OrdinalIgnoreCase)).ToList();
                    token.ThrowIfCancellationRequested();

                    if (!devices.Any())
                    {
                        token.ThrowIfCancellationRequested();
                        await LogError(officeFilter ?? AttendanceDeviceOffice.All, machineIpFilter, string.Empty, DateTime.Now, $"Machine IP '{machineIpFilter}' is not valid.");
                        return;
                    }
                }

                foreach (var device in devices)
                {
                    token.ThrowIfCancellationRequested();
                    await ClearSingleDeviceLogsAsync(device, token);
                }

                string successMessage = $"...... FINISHED CLEAR DEVICE LOGS PROCESS ......";
                Console.WriteLine(successMessage);
                await LogInfo(officeFilter ?? AttendanceDeviceOffice.All, machineIpFilter ?? string.Empty, string.Empty, DateTime.Now, successMessage);
            }
            catch (Exception ex)
            {
                string message = $"Error clearing device logs. Office: {officeFilter?.ToString() ?? "All"}, MachineIp: {machineIpFilter ?? "N/A"}, Exception: {ex.Message}";

                Console.WriteLine(message);

                await LogException(officeFilter ?? AttendanceDeviceOffice.All, machineIpFilter ?? string.Empty, string.Empty, DateTime.Now, message);
            }
        }
        public async Task ClearSingleDeviceLogsAsync(AttendanceDeviceDto device, CancellationToken token)
        {
            CZKEM zk = new CZKEM();

            try
            {
                token.ThrowIfCancellationRequested();

                string infoMessage = $"Trying to connect to device for clearing the device logs: {device.ip}:{device.port}.";
                Console.WriteLine(infoMessage);
                await LogInfo(device.office, device.ip, string.Empty, DateTime.Now, infoMessage);

                token.ThrowIfCancellationRequested();
                bool connected = zk.Connect_Net(device.ip, device.port);


                if (!connected)
                {
                    string message = $"Failed to connect to device {device.ip}:{device.port} for log clearing.";

                    Console.WriteLine(message);

                    await LogError(device.office, device.ip, string.Empty, DateTime.Now, message);

                    token.ThrowIfCancellationRequested();

                    return;
                }

                infoMessage = $"Enable => false for clearing the device logs: {device.ip}:{device.port}.";
                Console.WriteLine(infoMessage);
                await LogInfo(device.office, device.ip, string.Empty, DateTime.Now, infoMessage);
                token.ThrowIfCancellationRequested();

                zk.EnableDevice(MachineNumber, false);
                token.ThrowIfCancellationRequested();

                infoMessage = $"Going to clear the device logs: {device.ip}:{device.port}.";
                Console.WriteLine(infoMessage);
                await LogInfo(device.office, device.ip, string.Empty, DateTime.Now, infoMessage);
                token.ThrowIfCancellationRequested();

                bool logsCleared = zk.ClearGLog(MachineNumber);
                token.ThrowIfCancellationRequested();

                zk.RefreshData(MachineNumber);

                token.ThrowIfCancellationRequested();
                infoMessage = $"Enable => true for clearing the device logs: {device.ip}:{device.port}.";
                Console.WriteLine(infoMessage);
                await LogInfo(device.office, device.ip, string.Empty, DateTime.Now, infoMessage);

                token.ThrowIfCancellationRequested();
                zk.EnableDevice(MachineNumber, true);

                if (!logsCleared)
                {
                    token.ThrowIfCancellationRequested();

                    string message = $"Failed to clear logs for device {device.ip}:{device.port}.";

                    Console.WriteLine(message);

                    await LogError(device.office, device.ip, string.Empty, DateTime.Now, message);

                    return;
                }

                string successMessage = $"Logs cleared successfully for device {device.ip}:{device.port}.";

                Console.WriteLine(successMessage);

                await LogInfo(device.office, device.ip, string.Empty, DateTime.Now, successMessage);
                token.ThrowIfCancellationRequested();
            }
            catch (Exception ex)
            {
                string message = $"Error clearing logs for device {device.ip}:{device.port}. Exception: {ex.Message}";

                Console.WriteLine(message);

                await LogException(device.office, device.ip, string.Empty, DateTime.Now, message);
            }
            finally
            {
                zk.Disconnect();
            }
        }
        public async Task<bool> RemoveEmployeeRegistrationFromAllDevicesAsync(string enrollNumber, bool reconfirmation = false, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            if (!reconfirmation) return false;

            if (string.IsNullOrWhiteSpace(enrollNumber))
            {
                await LogError(AttendanceDeviceOffice.All, string.Empty, string.Empty, DateTime.Now, "Employee enroll number is required.");
                return false;
            }

            token.ThrowIfCancellationRequested();

            int foundCount = 0;
            int deletedCount = 0;
            int notFoundCount = 0;

            string infoMessage = $"Starting employee deletion process for EnrollNumber={enrollNumber}.";
            Console.WriteLine(infoMessage);
            await LogInfo(AttendanceDeviceOffice.All, string.Empty, string.Empty, DateTime.Now, infoMessage);

            //var devices = DeviceDataService.GetAllDevices();
            token.ThrowIfCancellationRequested();
            var db = new DatabaseContext();
            List<AttendanceDeviceDto> devices = await db.GetAttendanceDevicesAsync();

            token.ThrowIfCancellationRequested();
            foreach (var device in devices)
            {
                CZKEM zk = new CZKEM();
                token.ThrowIfCancellationRequested();
                try
                {


                    infoMessage = $"Trying to connect to device for remove employee registration from Device: {device.ip}:{device.port}.";
                    Console.WriteLine(infoMessage);
                    await LogInfo(device.office, device.ip, enrollNumber, DateTime.Now, infoMessage);

                    token.ThrowIfCancellationRequested();
                    if (!zk.Connect_Net(device.ip, device.port))
                    {
                        string message = $"Unable to connect to device {device.ip}:{device.port}.";
                        Console.WriteLine(message);
                        await LogError(device.office, device.ip, enrollNumber, DateTime.Now, message);

                        token.ThrowIfCancellationRequested();
                        continue;
                    }

                    token.ThrowIfCancellationRequested();
                    string name = string.Empty;
                    string password = string.Empty;
                    int privilege = 0;
                    bool enabled = false;

                    token.ThrowIfCancellationRequested();
                    infoMessage = $"Get user info for remove employee registration from Device: {device.ip}:{device.port}.";
                    Console.WriteLine(infoMessage);
                    await LogInfo(device.office, device.ip, enrollNumber, DateTime.Now, infoMessage);

                    token.ThrowIfCancellationRequested();
                    bool employeeExists = zk.SSR_GetUserInfo(MachineNumber, enrollNumber, out name, out password, out privilege, out enabled);

                    if (!employeeExists)
                    {
                        notFoundCount++;

                        string message = $"Employee '{enrollNumber}' not found on device {device.ip}:{device.port}.";
                        Console.WriteLine(message);

                        await LogInfo(device.office, device.ip, enrollNumber, DateTime.Now, message);

                        token.ThrowIfCancellationRequested();
                        continue;
                    }

                    foundCount++;

                    token.ThrowIfCancellationRequested();
                    infoMessage = $"Enable => false for remove employee registration from Device: {device.ip}:{device.port}.";
                    Console.WriteLine(infoMessage);
                    await LogInfo(device.office, device.ip, enrollNumber, DateTime.Now, infoMessage);
                    zk.EnableDevice(MachineNumber, false);

                    token.ThrowIfCancellationRequested();
                    infoMessage = $"Delete enrollment data of '{enrollNumber}' for remove employee registration from Device: {device.ip}:{device.port}.";
                    Console.WriteLine(infoMessage);
                    await LogInfo(device.office, device.ip, enrollNumber, DateTime.Now, infoMessage);
                    bool deleted = zk.SSR_DeleteEnrollData(MachineNumber, enrollNumber, 12);

                    token.ThrowIfCancellationRequested();
                    infoMessage = $"ZKBioTeco refresh data for remove employee registration from Device: {device.ip}:{device.port}.";
                    Console.WriteLine(infoMessage);
                    await LogInfo(device.office, device.ip, enrollNumber, DateTime.Now, infoMessage);
                    zk.RefreshData(MachineNumber);

                    token.ThrowIfCancellationRequested();
                    infoMessage = $"Enable => true for remove employee registration from Device: {device.ip}:{device.port}.";
                    Console.WriteLine(infoMessage);
                    await LogInfo(device.office, device.ip, enrollNumber, DateTime.Now, infoMessage);

                    token.ThrowIfCancellationRequested();
                    zk.EnableDevice(MachineNumber, true);

                    token.ThrowIfCancellationRequested();
                    if (deleted)
                    {
                        deletedCount++;

                        string message = $"Employee '{enrollNumber}' deleted successfully from device {device.ip}:{device.port}.";
                        Console.WriteLine(message);

                        await LogInfo(device.office, device.ip, enrollNumber, DateTime.Now, message);

                        token.ThrowIfCancellationRequested();
                    }
                    else
                    {
                        string message = $"Failed to delete employee '{enrollNumber}' from device {device.ip}:{device.port}.";
                        Console.WriteLine(message);

                        await LogError(device.office, device.ip, enrollNumber, DateTime.Now, message);

                        token.ThrowIfCancellationRequested();
                    }
                }
                catch (Exception ex)
                {
                    string message = $"Error deleting employee '{enrollNumber}' from device {device.ip}:{device.port}. Exception: {ex.Message}";
                    Console.WriteLine(message);

                    await LogException(device.office, device.ip, enrollNumber, DateTime.Now, message);
                }
                finally
                {
                    zk.Disconnect();
                }
            }

            string summaryMessage = $"Employee deletion completed. EnrollNumber={enrollNumber}, FoundOnDevices={foundCount}, DeletedFromDevices={deletedCount}, NotFoundOnDevices={notFoundCount}.";

            Console.WriteLine(summaryMessage);

            await LogInfo(AttendanceDeviceOffice.All, string.Empty, enrollNumber, DateTime.Now, summaryMessage);

            token.ThrowIfCancellationRequested();
            return deletedCount > 0;
        }
        public DeviceHealthCheck CheckDeviceHealth(string ip, int port, int machineNumber = 1)
        {
            var result = new DeviceHealthCheck
            {
                IpAddress = ip,
                Port = port
            };

            CZKEM zk = new CZKEM();

            try
            {
                if (!zk.Connect_Net(ip, port))
                {
                    result.IsOnline = false;
                    result.ErrorMessage = "Unable to connect.";
                    return result;
                }

                result.IsOnline = true;

                //string firmware = string.Empty;
                //zk.GetFirmwareVersion(ref firmware);
                //result.FirmwareVersion = firmware;

                string firmware = string.Empty;
                result.FirmwareVersion = zk.GetFirmwareVersion(machineNumber, ref firmware)
                    ? firmware
                    : "Unknown";

                //string serial = string.Empty;
                //result.SerialNumber = zk.GetSerialNumber(machineNumber, ref serial)
                //    ? serial
                //    : "Unknown";

                string serial = string.Empty;

                if (zk.GetSerialNumber(machineNumber, out serial))
                {
                    result.SerialNumber = serial;
                }
                else
                {
                    result.SerialNumber = "Unknown";
                }



                //string serial = string.Empty;
                //zk.GetSerialNumber(machineNumber, out serial);
                //result.SerialNumber = serial;

                int users = 0;
                zk.GetDeviceStatus(machineNumber, 2, ref users);
                result.UserCount = users;

                int fingerprints = 0;
                zk.GetDeviceStatus(machineNumber, 3, ref fingerprints);
                result.FingerprintCount = fingerprints;

                int logs = 0;
                zk.GetDeviceStatus(machineNumber, 6, ref logs);
                result.AttendanceLogCount = logs;

                int year = 0;
                int month = 0;
                int day = 0;
                int hour = 0;
                int minute = 0;
                int second = 0;

                if (zk.GetDeviceTime(machineNumber, ref year, ref month, ref day, ref hour, ref minute, ref second))
                {
                    result.DeviceTime = new DateTime(year, month, day, hour, minute, second);
                    result.TimeSynced = Math.Abs((DateTime.Now - result.DeviceTime).TotalMinutes) <= 5;
                }

                zk.Disconnect();
            }
            catch (Exception ex)
            {
                result.IsOnline = false;
                result.ErrorMessage = ex.Message;

                try
                {
                    zk.Disconnect();
                }
                catch
                {
                }
            }

            return result;
        }
        public DeviceDiagnostic RunDeviceDiagnostic(string ip, int port, int machineNumber = 1)
        {
            var result = new DeviceDiagnostic();

            try
            {
                CZKEM zk = new CZKEM();

                if (!zk.Connect_Net(ip, port))
                {
                    result.IsOnline = false;
                    result.DiagnosticMessage = "Device Offline";
                    return result;
                }

                result.IsOnline = true;

                string firmware = "";
                if (zk.GetFirmwareVersion(machineNumber, ref firmware))
                    result.FirmwareVersion = firmware;

                string serial = "";
                if (zk.GetSerialNumber(machineNumber, out serial))
                    result.SerialNumber = serial;

                //zk.GetDeviceStatus(machineNumber, 2, ref result.UserCount);




                //zk.GetDeviceStatus(machineNumber, 3, ref result.FingerprintCount);
                //zk.GetDeviceStatus(machineNumber, 6, ref result.LogCount);

                //zk.GetDeviceStatus(machineNumber, 8, ref result.UserCapacity);
                //zk.GetDeviceStatus(machineNumber, 7, ref result.FingerCapacity);
                //zk.GetDeviceStatus(machineNumber, 9, ref result.LogCapacity);


                int userCount = 0;
                zk.GetDeviceStatus(machineNumber, 2, ref userCount);
                result.UserCount = userCount;

                int fingerprintCount = 0;
                zk.GetDeviceStatus(machineNumber, 3, ref fingerprintCount);
                result.FingerprintCount = fingerprintCount;

                int logCount = 0;
                zk.GetDeviceStatus(machineNumber, 6, ref logCount);
                result.LogCount = logCount;

                int userCapacity = 0;
                zk.GetDeviceStatus(machineNumber, 8, ref userCapacity);
                result.UserCapacity = userCapacity;

                int fingerCapacity = 0;
                zk.GetDeviceStatus(machineNumber, 7, ref fingerCapacity);
                result.FingerCapacity = fingerCapacity;

                int logCapacity = 0;
                zk.GetDeviceStatus(machineNumber, 9, ref logCapacity);
                result.LogCapacity = logCapacity;



                List<string> warnings = new();

                if (result.UserCapacity > 0)
                {
                    double userUsage = (double)result.UserCount / result.UserCapacity * 100;
                    if (userUsage > 90)
                        warnings.Add($"User storage {userUsage:F0}% full");
                }

                if (result.FingerCapacity > 0)
                {
                    double fpUsage = (double)result.FingerprintCount / result.FingerCapacity * 100;
                    if (fpUsage > 90)
                        warnings.Add($"Fingerprint storage {fpUsage:F0}% full");
                }

                if (result.LogCapacity > 0)
                {
                    double logUsage = (double)result.LogCount / result.LogCapacity * 100;
                    if (logUsage > 90)
                        warnings.Add($"Attendance logs {logUsage:F0}% full");
                }

                result.DiagnosticMessage = warnings.Count == 0
                    ? "Healthy"
                    : string.Join(", ", warnings);

                zk.Disconnect();
            }
            catch (Exception ex)
            {
                result.DiagnosticMessage = ex.Message;
            }

            return result;
        }
    }
}
