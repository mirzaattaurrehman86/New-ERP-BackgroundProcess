using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using ZKattendanceTestProject.Dto;

namespace ZKattendanceTestProject.Services.CopilotService
{
    public class ZKSocketCopilotService
    {


        private const ushort CMD_CONNECT = 1000;
        private const ushort CMD_ATTLOG_RRQ = 13;


        private UdpClient _client;
        private IPEndPoint _endpoint;

        private ushort _sessionId = 0;
        private ushort _replyId = 0;

        public ZKSocketCopilotService(string ip, int port = 4370)
        {
            _endpoint = new IPEndPoint(IPAddress.Parse(ip), port);

            _client = new UdpClient();
            _client.Client.ReceiveTimeout = 5000;
        }


        public (bool connected, string message) Connect()
        {
            try
            {
                byte[] command = CreateHeader(CMD_CONNECT);
                _client.Send(command, command.Length, _endpoint);

                var response = _client.Receive(ref _endpoint);

                if (response.Length >= 8)
                {
                    _sessionId = BitConverter.ToUInt16(response, 4);
                    _replyId = BitConverter.ToUInt16(response, 6);
                    return (true, "Connected successfully");
                }

                return (false, "Invalid response from device");
            }
            catch (SocketException ex)
            {
                return (false, $"Socket error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return (false, $"Unexpected error: {ex.Message}");
            }
        }

        //public (bool success, byte[]? response, string message) GetAttendanceLogs()
        //{
        //    try
        //    {
        //        byte[] command = CreateHeader(CMD_ATTLOG_RRQ);
        //        _client.Send(command, command.Length, _endpoint);

        //        var response = _client.Receive(ref _endpoint);

        //        if (response.Length > 8)
        //        {
        //            return (true, response, "Attendance logs retrieved");
        //        }

        //        return (false, null, "No logs received");
        //    }
        //    catch (Exception ex)
        //    {
        //        return (false, null, $"Error: {ex.Message}");
        //    }
        //}

        public List<AttendanceDto> GetAttendanceLogs()
        {
            List<AttendanceDto> logs = new List<AttendanceDto>();

            byte[] command = CreateHeader(CMD_ATTLOG_RRQ);
            _client.Send(command, command.Length, _endpoint);

            // Keep receiving until timeout
            while (true)
            {
                try
                {
                    var response = _client.Receive(ref _endpoint);

                    if (response.Length <= 16)
                    {
                        // Likely just ACK or end-of-data marker
                        //break;

                        continue;
                    }

                    logs.AddRange(ParseAttendanceLogs(response));
                }
                catch (SocketException)
                {
                    // Timeout reached, stop reading
                    break;
                }
            }

            return logs;
        }


        private byte[] CreateHeader(ushort command)
        {
            byte[] buffer = new byte[8];

            BitConverter.GetBytes(command).CopyTo(buffer, 0);
            BitConverter.GetBytes((ushort)0).CopyTo(buffer, 2);
            BitConverter.GetBytes(_sessionId).CopyTo(buffer, 4);
            BitConverter.GetBytes(_replyId++).CopyTo(buffer, 6);

            ushort checksum = CalculateChecksum(buffer);
            BitConverter.GetBytes(checksum).CopyTo(buffer, 2);

            return buffer;
        }
        private ushort CalculateChecksum(byte[] data)
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

        //public List<AttendanceDto> ParseAttendanceLogs(byte[] response)
        //{
        //    List<AttendanceDto> logs = new List<AttendanceDto>();

        //    int recordSize = 40; // or 36 depending on device

        //    int offset = 8;      // skip header (first 8 bytes)

        //    while (offset + recordSize <= response.Length)
        //    {
        //        byte[] record = new byte[recordSize];
        //        Array.Copy(response, offset, record, 0, recordSize);

        //        // 🔍 Debug: print raw hex for this record
        //        Console.WriteLine(BitConverter.ToString(record));


        //        // ✅ Fix UserId parsing
        //        string rawId = Encoding.ASCII.GetString(record, 2, 24);
        //        string userId = new string(rawId.Where(c => !char.IsControl(c)).ToArray()).Trim();

        //        uint seconds = BitConverter.ToUInt32(record, 28);
        //        DateTime timestamp = new DateTime(2000, 1, 1).AddSeconds(seconds);

        //        DateTime ts2000 = new DateTime(2000, 1, 1).AddSeconds(seconds);
        //        DateTime ts1970 = DateTimeOffset.FromUnixTimeSeconds(seconds).DateTime;
        //        Console.WriteLine($"2000-base: {ts2000}, 1970-base: {ts1970}");

        //        int status = record[30];
        //        int verifyType = record[31];

        //        bool isValid = !string.IsNullOrWhiteSpace(userId)
        //       && timestamp.Year >= 2000
        //       && timestamp <= DateTime.Now
        //       && !(status == 0 && verifyType == 0);

        //        if (isValid)
        //            logs.Add(new AttendanceDto
        //            {
        //                UserId = userId,
        //                Timestamp = timestamp,
        //                Status = status,
        //                VerifyType = verifyType,

        //                //RawRecord = record,
        //                //RawHex = BitConverter.ToString(record)
        //            });

        //        offset += recordSize;
        //    }

        //    return logs;
        //}




        //public List<AttendanceDto> ParseAttendanceLogs(byte[] response)
        //{
        //    List<AttendanceDto> logs = new List<AttendanceDto>();

        //    int recordSize = 40; // typical for ZKTeco devices
        //    int offset = 8;      // skip header (first 8 bytes)

        //    while (offset + recordSize <= response.Length)
        //    {
        //        byte[] record = new byte[recordSize];
        //        Array.Copy(response, offset, record, 0, recordSize);

        //        // 🔍 Debug: print raw hex for inspection
        //        //Console.WriteLine(BitConverter.ToString(record));

        //        // User ID (ASCII string, padded with zeros)
        //        string rawId = Encoding.ASCII.GetString(record, 2, 24);
        //        string userId = new string(rawId.Where(c => !char.IsControl(c)).ToArray()).Trim();

        //        // ✅ Timestamp decoding (big-endian Unix epoch)
        //        uint seconds = (uint)(
        //            (record[28] << 24) |
        //            (record[29] << 16) |
        //            (record[30] << 8) |
        //            (record[31])
        //        );
        //        DateTime timestamp = DateTimeOffset.FromUnixTimeSeconds(seconds).DateTime;

        //        int status = record[30];      // depending on firmware, may need adjustment
        //        int verifyType = record[31];  // depending on firmware, may need adjustment

        //        // Validity check
        //        bool isValid = !string.IsNullOrWhiteSpace(userId)
        //                       && timestamp.Year >= 2000
        //                       && timestamp <= DateTime.Now
        //                       && !(status == 0 && verifyType == 0);

        //        if (isValid)
        //        {
        //            logs.Add(new AttendanceDto
        //            {
        //                UserId = userId,
        //                Timestamp = timestamp,
        //                Status = status,
        //                VerifyType = verifyType
        //            });
        //        }

        //        offset += recordSize;
        //    }

        //    return logs;
        //}


        //private DateTime DecodeTime(uint value)
        //{
        //    int second = (int)(value & 0x3F);
        //    int minute = (int)((value >> 6) & 0x3F);
        //    int hour = (int)((value >> 12) & 0x1F);
        //    int day = (int)((value >> 17) & 0x1F);
        //    int month = (int)((value >> 22) & 0x0F);
        //    int year = (int)((value >> 26) & 0x3F) + 2000;

        //    return new DateTime(year, month, day, hour, minute, second);
        //}




        public List<AttendanceDto> ParseAttendanceLogs(byte[] response)
        {
            List<AttendanceDto> logs = new List<AttendanceDto>();

            int recordSize = 40;
            int offset = 8; // skip header

            while (offset + recordSize <= response.Length)
            {
                byte[] record = new byte[recordSize];
                Array.Copy(response, offset, record, 0, recordSize);

                // UserId: bytes 8–23
                string userId = Encoding.ASCII.GetString(record, 8, 16).Trim('\0');

                // Timestamp: bytes 29–32 (reverse order)
                uint rawTime = (uint)(
                    (record[32] << 24) |
                    (record[31] << 16) |
                    (record[30] << 8) |
                    (record[29])
                );

                DateTime timestamp = DecodeTime(rawTime);

                int status = record[33];
                int verifyType = record[34];

                if (!string.IsNullOrWhiteSpace(userId) && timestamp.Year >= 2000)
                {
                    logs.Add(new AttendanceDto
                    {
                        UserId = userId,
                        Timestamp = timestamp,
                        Status = status,
                        VerifyType = verifyType
                    });
                }

                offset += recordSize;
            }

            return logs;
        }

        private DateTime DecodeTime(uint value)
        {
            int second = (int)(value & 0x3F);
            int minute = (int)((value >> 6) & 0x3F);
            int hour = (int)((value >> 12) & 0x1F);
            int day = (int)((value >> 17) & 0x1F);
            int month = (int)((value >> 22) & 0x0F);
            int year = (int)((value >> 26) & 0x3F) + 2000;

            try
            {
                return new DateTime(year, month, day, hour, minute, second);
            }
            catch
            {
                return DateTime.MinValue;
            }
        }







    }
}

public class AttendanceDto
{
    public string UserId { get; set; }
    public DateTime Timestamp { get; set; }
    public int Status { get; set; }
    public int VerifyType { get; set; }
}