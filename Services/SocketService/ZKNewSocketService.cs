



using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;


public class ZKNewSocketService
{
    private const ushort CMD_CONNECT = 1000;
    private const ushort CMD_ATTLOG_RRQ = 13;

    private UdpClient _client;
    private IPEndPoint _endpoint;
    private ushort _sessionId = 0;
    private ushort _replyId = 0;

    public ZKNewSocketService(string ip, int port = 4370)
    {
        _endpoint = new IPEndPoint(IPAddress.Parse(ip), port);
        _client = new UdpClient();
        _client.Client.ReceiveTimeout = 5000;
    }

    //private byte[] CreateHeader(ushort command, byte[] data = null)
    //{
    //    if (data == null) data = new byte[0];
    //    _replyId++;

    //    ushort length = (ushort)(8 + data.Length);
    //    byte[] packet = new byte[length];

    //    Array.Copy(BitConverter.GetBytes(command), 0, packet, 0, 2);
    //    Array.Copy(BitConverter.GetBytes(_replyId), 0, packet, 2, 2);
    //    Array.Copy(BitConverter.GetBytes(_sessionId), 0, packet, 4, 2);
    //    Array.Copy(BitConverter.GetBytes((ushort)data.Length), 0, packet, 6, 2);
    //    Array.Copy(data, 0, packet, 8, data.Length);

    //    // Calculate checksum
    //    ushort chksum = CalcChecksum(packet);
    //    Array.Copy(BitConverter.GetBytes(chksum), 0, packet, 0, 2);

    //    return packet;
    //}

    //private ushort CalcChecksum(byte[] buffer)
    //{
    //    int sum = 0;
    //    for (int i = 0; i < buffer.Length; i++)
    //        sum += buffer[i];
    //    return (ushort)(sum & 0xFFFF);
    //}

    private byte[] CreateHeader(ushort command)
    {
        byte[] buffer = new byte[8];

        BitConverter.GetBytes(command).CopyTo(buffer, 0);
        BitConverter.GetBytes((ushort)0).CopyTo(buffer, 2);
        BitConverter.GetBytes(_sessionId).CopyTo(buffer, 4);
        BitConverter.GetBytes(_replyId++).CopyTo(buffer, 6);

        ushort checksum = CalcChecksum(buffer);
        BitConverter.GetBytes(checksum).CopyTo(buffer, 2);

        return buffer;
    }
    private ushort CalcChecksum(byte[] data)
    {
        int checksum = 0;

        for (int i = 0; i < data.Length; i += 2)
        {
            ushort value = (i + 1 < data.Length)
                ? BitConverter.ToUInt16(data, i)
                : data[i];

            checksum += value;
        }

        checksum = (checksum & 0xFFFF) + (checksum >> 16);
        return (ushort)(~checksum);
    }

    public bool Connect()
    {
        byte[] command = CreateHeader(CMD_CONNECT);
        _client.Send(command, command.Length, _endpoint);

        var response = _client.Receive(ref _endpoint);
        if (response.Length >= 8)
        {
            _sessionId = BitConverter.ToUInt16(response, 4);
            _replyId = BitConverter.ToUInt16(response, 6);
            return true;
        }
        return false;
    }

    public List<AttendanceRecord> GetAttendanceLogs()
    {
        var logs = new List<AttendanceRecord>();

        byte[] command = CreateHeader(CMD_ATTLOG_RRQ);
        _client.Send(command, command.Length, _endpoint);

        var allData = new List<byte>();

        try
        {
            while (true)
            {
                var response = _client.Receive(ref _endpoint);

                // If response is too short, break
                if (response.Length <= 8) break;

                // Append payload (skip 8-byte header)
                allData.AddRange(response.AsSpan(8).ToArray());
            }
        }
        catch (SocketException)
        {
            // Timeout reached, stop receiving
        }

        //// Parse combined payload
        //string raw = Encoding.ASCII.GetString(allData.ToArray());
        //string[] records = raw.Split('\n');

        //foreach (var rec in records)
        //{
        //    if (string.IsNullOrWhiteSpace(rec)) continue;
        //    string[] parts = rec.Split('\t');
        //    if (parts.Length >= 5)
        //    {
        //        logs.Add(new AttendanceRecord
        //        {
        //            UserId = parts[0],
        //            Date = parts[1],
        //            Time = parts[2],
        //            VerifyMode = parts[3],
        //            InOutMode = parts[4]
        //        });
        //    }
        //}

        //return logs;


        //return ParseBinaryLogs(allData.ToArray());
        return ParseAsciiLogs(allData.ToArray());
    }

    public List<AttendanceRecord> ParseBinaryLogs(byte[] payload)
    {
        var logs = new List<AttendanceRecord>();

        int recordSize = 40; // typical size per record
        int count = payload.Length / recordSize;

        for (int i = 0; i < count; i++)
        {
            int offset = i * recordSize;

            int userId = BitConverter.ToInt32(payload, offset + 0);
            int verifyMode = BitConverter.ToInt16(payload, offset + 4);
            int inOutMode = BitConverter.ToInt16(payload, offset + 6);

            // Timestamp is packed at offset+8 (4 bytes)
            int timestamp = BitConverter.ToInt32(payload, offset + 8);
            //DateTime dt = DecodeTimestamp(timestamp);

            //logs.Add(new AttendanceRecord
            //{
            //    UserId = userId.ToString(),
            //    DateTime = dt,
            //    VerifyMode = verifyMode.ToString(),
            //    InOutMode = inOutMode.ToString()
            //});
            DateTime? dt = DecodeTimestamp(timestamp);
            if (dt != null)
            {
                logs.Add(new AttendanceRecord
                {
                    UserId = userId.ToString(),
                    DateTime = dt.Value,
                    VerifyMode = verifyMode.ToString(),
                    InOutMode = inOutMode.ToString()
                });
            }
        }

        return logs;
    }


    public List<AttendanceRecord> ParseAsciiLogs(byte[] payload)
    {
        var logs = new List<AttendanceRecord>();

        // Convert to ASCII text
        string raw = Encoding.ASCII.GetString(payload);

        // Split on newline
        string[] records = raw.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var rec in records)
        {
            // Each record is tab-delimited: PIN \t Date \t Time \t VerifyMode \t InOutMode
            string[] parts = rec.Split('\t');
            if (parts.Length >= 5)
            {
                logs.Add(new AttendanceRecord
                {
                    UserId = parts[0],
                    Date = parts[1],
                    Time = parts[2],
                    VerifyMode = parts[3],
                    InOutMode = parts[4]
                });
            }
        }

        return logs;
    }


    // Decode ZKTeco packed timestamp
    private DateTime? DecodeTimestamp(int packed)
    {
        int second = packed & 0x3F;
        int minute = (packed >> 6) & 0x3F;
        int hour = (packed >> 12) & 0x1F;
        int day = (packed >> 17) & 0x1F;
        int month = (packed >> 22) & 0x0F;
        int year = ((packed >> 26) & 0x3F) + 2000;

        // Validate ranges
        if (year < 2000 || year > 2100 ||
            month < 1 || month > 12 ||
            day < 1 || day > 31 ||
            hour < 0 || hour > 23 ||
            minute < 0 || minute > 59 ||
            second < 0 || second > 59)
        {
            return null; // invalid timestamp
        }

        return new DateTime(year, month, day, hour, minute, second);
    }

}

public class AttendanceRecord
{
    public string UserId { get; set; }
    public string Date { get; set; }
    public string Time { get; set; }
    public string VerifyMode { get; set; }
    public string InOutMode { get; set; }

    public DateTime DateTime { get; set; }
}

//// Example usage
//class Program
//{
//    static void Main()
//    {
//        var service = new ZKSocketService("192.168.36.203", 4370);

//        if (service.Connect())
//        {
//            Console.WriteLine("Connected successfully!");

//            var logs = service.GetAttendanceLogs();
//            foreach (var log in logs)
//            {
//                Console.WriteLine($"{log.UserId} | {log.Date} {log.Time} | Verify={log.VerifyMode} | InOut={log.InOutMode}");
//            }
//        }
//        else
//        {
//            Console.WriteLine("Connection failed!");
//        }
//    }
//}

