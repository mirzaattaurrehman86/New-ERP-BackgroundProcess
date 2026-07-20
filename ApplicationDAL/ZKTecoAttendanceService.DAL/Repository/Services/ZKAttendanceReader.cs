//using System;
//using System.Collections.Generic;
//using System.Security.Cryptography;
//using System.Text.Json;
//using zkemkeeper; // Make sure you added zkemkeeper.dll as a COM reference


//public class ZKAttendanceReader
//{
//    private CZKEM _zk = new CZKEM();

//    public (List<object[]>? data, bool success, string message) GetAttendance(string ip, int port = 4370)
//    {
//        if (string.IsNullOrEmpty(ip))
//            return (null, false, "IP address cannot be empty.");

//        bool connected = _zk.Connect_Net(ip, port);

//        if (!connected)
//            return (null, false, "Failed to connect to device.");

//        List<object[]> attendanceData = new List<object[]>();

//        _zk.ReadGeneralLogData(1); // 1 = Machine number (usually 1)

//        int enrollNumber, verifyMode, inOutMode, year, month, day, hour, minute, second;

//        while (_zk.SSR_GetGeneralLogData(1, out enrollNumber, out verifyMode, out inOutMode, out year, out month, out day, out hour, out minute, out second))
//        {
//            string timestamp = new DateTime(year, month, day, hour, minute, second).ToString("yyyy-MM-dd HH:mm:ss");

//            attendanceData.Add(new object[]
//            {
//                    enrollNumber,
//                    verifyMode,
//                    inOutMode,
//                    timestamp
//            });
//        }

//        _zk.Disconnect();

//        return (attendanceData, true, "Fetched successfully.");
//    }

//    public string GetAttendanceJson(string ip, int port = 4370)
//    {
//        var (data, success, message) = GetAttendance(ip, port);

//        if (!success)
//            return JsonSerializer.Serialize(new { success = false, message });

//        return JsonSerializer.Serialize(new { success = true, data });
//    }

//}