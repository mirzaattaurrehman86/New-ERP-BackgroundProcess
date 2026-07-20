//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Text.Json;
//using ZKattendanceTestProject.Dto;
////using ZKattendanceTestProject.Services;

//namespace ZKattendanceTestProject
//{
//    public class ReadAttendance
//    {
//        public (List<object[]>? filtered, bool success, string message) AKDeviceResponse(string ip = "192.168.36.203", int port = 4370)
//        {
//            try
//            {
//                //var zk = new ZKSocketService("192.168.20.30", 4370);
//                var zk = new ZKSocketService(ip, port);
//                //var zk = new ZKSocketService();

//                var conn = zk.Connect(/*ip, port*/);

//                if (!conn.connected)
//                {
//                    Console.WriteLine("[WARN] Connection failed." + Environment.NewLine);
//                    return (null, false, conn.message);
//                }

//                var data = zk.GetAttendance();

//                DateTime startDate = DateTime.Now.AddDays(-7).Date;
//                DateTime endDate = DateTime.Now.Date;

//                var filtered = data.Where(x =>
//                {
//                    DateTime d = DateTime.Parse(x[3].ToString());
//                    return d >= startDate && d <= endDate;
//                }).ToList();

//                var validRecords = filtered
//        .Where(x => IsValidEmployeeId(x[1].ToString()))
//        .ToList();


//                // Map to attendance objects
//                List<Attendance> attendanceList = new List<Attendance>();
//                foreach (var x in validRecords)
//                {
//                    var dt = DateTime.Parse(x[3].ToString());
//                    attendanceList.Add(new Attendance
//                    {
//                        EmpId = x[1].ToString(),
//                        Type = Convert.ToInt32(x[2]),
//                        Date = dt.ToString("yyyy-MM-dd"),
//                        Time = dt.ToString("HH:mm:ss"),
//                        DateTime = dt.ToString("yyyy-MM-dd HH:mm:ss")
//                    });
//                }

//                // Serialize to JSON
//                string json = JsonSerializer.Serialize(attendanceList, new JsonSerializerOptions
//                {
//                    WriteIndented = true
//                });

//                return (filtered, true, conn.message);
//            }
//            catch (Exception ex)
//            {

//                throw;
//            }
//        }
//        bool IsValidEmployeeId(string empId)
//        {
//            return int.TryParse(empId, out _) && empId.Length == 4;
//        }
//    }
//}
////public class Attendance
////{
////    public string EmpId { get; set; }
////    public int Type { get; set; }
////    public string Date { get; set; }
////    public string Time { get; set; }
////    public string DateTime { get; set; }
////    public string StartTime { get; set; } = "00:00:00";
////    public string EndTime { get; set; } = "00:00:00";
////    public int ShiftId { get; set; } = 0;
////    public bool IsDeleted { get; set; } = false;
////    public string CreatedAt { get; set; } => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
////}
///
using System.Text.Json;
using ZKTecoAttendanceService.DTO.Dto;

namespace ZKTecoAttendanceService.DAL.Repository.Services
{
    public class ReadAttendance
    {
        public (List<Attendance>? data, bool success, string message) AKDeviceResponse(string ip = "192.168.36.203", int port = 4370)
        {
            try
            {
                var zk = new ZKSocketService(ip, port);
                var conn = zk.Connect();

                if (!conn.connected)
                    return (null, false, conn.message);

                var rawData = zk.GetAttendance();

                DateTime startDate = DateTime.Now.AddDays(-7).Date;
                DateTime endDate = DateTime.Now.Date;

                List<Attendance> attendanceList = new List<Attendance>();

                Console.WriteLine("rawData is => " + JsonSerializer.Serialize(rawData));

                foreach (var x in rawData)
                {
                    if (x[3] == null)
                        continue;

                    if (!DateTime.TryParse(x[3].ToString(), out DateTime dt))
                        continue;

                    if (dt < startDate || dt > endDate)
                        continue;

                    string empId = x[1]?.ToString();

                    if (!IsValidEmployeeId(empId))
                        continue;

                    attendanceList.Add(new Attendance
                    {
                        EmpId = empId,
                        Type = Convert.ToInt32(x[2]),
                        Date = dt.ToString("yyyy-MM-dd"),
                        Time = dt.ToString("HH:mm:ss"),
                        DateTime = dt.ToString("yyyy-MM-dd HH:mm:ss")
                    });
                }

                return (attendanceList, true, "Success");
            }
            catch (Exception ex)
            {
                return (null, false, ex.Message);
            }
        }

        bool IsValidEmployeeId(string empId)
        {
            return !string.IsNullOrEmpty(empId)
                   && int.TryParse(empId, out _)
                   && empId.Length == 4;
        }
    }
}
