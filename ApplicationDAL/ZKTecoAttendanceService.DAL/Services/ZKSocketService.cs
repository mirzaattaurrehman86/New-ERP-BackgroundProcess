//using System.Net;
//using System.Net.Sockets;
//using System.Text;

//public class ZKSocketService
//{
//    private UdpClient _client;
//    private IPEndPoint _endpoint;

//    private string _ip;
//    private int _port;

//    private ushort _sessionId = 0;
//    private ushort _replyId = 0;

//    public ZKSocketService(string ip, int port = 4370)
//    {
//        _ip = ip;
//        _port = port;
//        _endpoint = new IPEndPoint(IPAddress.Parse(ip), port);

//        _client = new UdpClient();
//        _client.Client.ReceiveTimeout = 5000;
//    }

//    // ✅ CONNECT
//    public (bool connected, string message) Connect()
//    {
//        byte[] command = CreateHeader(1000); // CMD_CONNECT

//        _client.Send(command, command.Length, _endpoint);

//        try
//        {
//            var response = _client.Receive(ref _endpoint);

//            if (response.Length > 0)
//            {
//                _sessionId = BitConverter.ToUInt16(response, 4);
//                _replyId = BitConverter.ToUInt16(response, 6);

//                return (true, "Connected successfully");
//            }

//            return (false, "No response from device");
//        }
//        catch (Exception ex)
//        {
//            return (false, ex.Message);
//        }
//    }

//    // ✅ GET ATTENDANCE
//    public List<object[]> GetAttendance()
//    {
//        List<object[]> attendance = new List<object[]>();

//        byte[] command = CreateHeader(13); // CMD_ATTLOG_RRQ
//        _client.Send(command, command.Length, _endpoint);

//        List<byte> fullData = new List<byte>();

//        try
//        {
//            while (true)
//            {
//                var response = _client.Receive(ref _endpoint);

//                if (response.Length == 0)
//                    break;

//                // ✅ Ignore small ACK packets
//                if (response.Length <= 16)
//                    continue;

//                fullData.AddRange(response);

//                if (response.Length < 1024)
//                    break;
//            }
//        }
//        catch (SocketException)
//        {
//            // ✅ This is expected → device finished sending data
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine("Unexpected error: " + ex.Message);
//        }

//        byte[] data = fullData.ToArray();

//        Console.WriteLine("Total bytes: " + data.Length);
//        Console.WriteLine("Records: " + (data.Length / 40));

//        if (data.Length < 10)
//            return attendance;

//        int pos = 10;

//        while (pos + 40 <= data.Length)
//        {
//            byte[] record = data.Skip(pos).Take(40).ToArray();

//            int uid = record[4] + (record[5] << 8);

//            string userId = Encoding.ASCII
//                .GetString(record, 6, 8)
//                .Trim('\0');

//            int state = record[33];

//            //int timeInt = BitConverter.ToInt32(record, 29);

//            byte[] timeBytes = record.Skip(29).Take(4).ToArray();
//            Array.Reverse(timeBytes);

//            int timeInt = BitConverter.ToInt32(timeBytes, 0);

//            //timeInt contains minus number then DecodeTime function make it invalid date, so we need to check it before decode


//            if (timeInt > 0)
//            {



//                string timestamp = DecodeTime(timeInt);

//                attendance.Add(new object[]
//                {
//                uid,
//                userId,
//                state,
//                timestamp
//                });
//            }
//            pos += 40;
//        }

//        return attendance;
//    }


//    private byte[] CreateHeader(ushort command)
//    {
//        byte[] buffer = new byte[8];

//        BitConverter.GetBytes(command).CopyTo(buffer, 0);
//        BitConverter.GetBytes((ushort)0).CopyTo(buffer, 2); // temp checksum
//        BitConverter.GetBytes(_sessionId).CopyTo(buffer, 4);
//        BitConverter.GetBytes(_replyId++).CopyTo(buffer, 6);

//        ushort checksum = CalculateChecksum(buffer);
//        BitConverter.GetBytes(checksum).CopyTo(buffer, 2);

//        return buffer;
//    }


//    private ushort CalculateChecksum(byte[] data)
//    {
//        int checksum = 0;

//        for (int i = 0; i < data.Length; i += 2)
//        {
//            ushort value = (i + 1 < data.Length)
//                ? BitConverter.ToUInt16(data, i)
//                : data[i];

//            checksum += value;
//        }

//        checksum = (checksum & 0xFFFF) + (checksum >> 16);
//        return (ushort)(~checksum);
//    }

//    private string DecodeTime(int data)
//    {
//        try
//        {
//            int second = data % 60;
//            data /= 60;
//            int minute = data % 60;
//            data /= 60;
//            int hour = data % 24;
//            data /= 24;
//            int day = (data % 31) + 1;
//            data /= 31;
//            int month = (data % 12) + 1;
//            data /= 12;
//            int year = data + 2000;

//            // ✅ Validate before creating DateTime
//            if (year < 2000 || year > 2100)
//                return "Invalid Date";

//            DateTime dt = new DateTime(year, month, day, hour, minute, second);
//            return dt.ToString("yyyy-MM-dd HH:mm:ss");
//        }
//        catch (Exception ex)
//        {
//            return "Invalid Date";
//        }
//    }
//}



using System.Net;
using System.Net.Sockets;
using System.Text;

public class ZKSocketService
{

    private const ushort CMD_CONNECT = 1000;



    private UdpClient _client;
    private IPEndPoint _endpoint;

    private ushort _sessionId = 0;
    private ushort _replyId = 0;

    public ZKSocketService(string ip, int port = 4370)
    {
        _endpoint = new IPEndPoint(IPAddress.Parse(ip), port);

        _client = new UdpClient();
        _client.Client.ReceiveTimeout = 5000;
    }

    // ✅ CONNECT
    //public (bool connected, string message) Connect()
    //{
    //    byte[] command = CreateHeader(1000); // CMD_CONNECT
    //    _client.Send(command, command.Length, _endpoint);

    //    try
    //    {
    //        var response = _client.Receive(ref _endpoint);

    //        if (response.Length > 0)
    //        {
    //            _sessionId = BitConverter.ToUInt16(response, 4);
    //            _replyId = BitConverter.ToUInt16(response, 6);
    //            return (true, "Connected successfully");
    //        }

    //        return (false, "No response from device");
    //    }
    //    catch (Exception ex)
    //    {
    //        return (false, ex.Message);
    //    }
    //}
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


    // ✅ GET ATTENDANCE (Improved)
    public List<object[]> GetAttendance()
    {
        List<object[]> attendance = new List<object[]>();

        byte[] command = CreateHeader(13); // CMD_ATTLOG_RRQ
        _client.Send(command, command.Length, _endpoint);

        List<byte> fullData = new List<byte>();

        try
        {
            while (true)
            {
                var response = _client.Receive(ref _endpoint);

                // Ignore empty
                if (response == null || response.Length == 0)
                    break;

                // Ignore ACK packets
                if (response.Length <= 16)
                    continue;

                fullData.AddRange(response);

                // 🔥 Better stop condition:
                // If packet smaller than expected chunk → likely last packet
                if (response.Length < 1024)
                    break;
            }
        }
        catch (SocketException)
        {
            // Expected when device stops sending
        }

        byte[] data = fullData.ToArray();

        if (data.Length < 40)
            return attendance;

        int pos = 10;

        while (pos + 40 <= data.Length)
        {
            try
            {
                byte[] record = data.Skip(pos).Take(40).ToArray();

                int uid = record[4] + (record[5] << 8);

                string userId = Encoding.ASCII
                    .GetString(record, 6, 8)
                    .Trim('\0');

                int state = record[33];

                // Time decoding
                byte[] timeBytes = record.Skip(29).Take(4).ToArray();
                Array.Reverse(timeBytes);

                int timeInt = BitConverter.ToInt32(timeBytes, 0);

                if (timeInt <= 0)
                {
                    pos += 40;
                    continue;
                }

                string timestamp = DecodeTimeSafe(timeInt);

                // Skip invalid dates
                if (timestamp == null)
                {
                    pos += 40;
                    continue;
                }

                attendance.Add(new object[]
                {
                    uid,
                    userId,
                    state,
                    timestamp
                });
            }
            catch
            {
                // Skip corrupted record
            }

            pos += 40;
        }

        return attendance;
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

    // ✅ SAFE TIME DECODE
    private string? DecodeTimeSafe(int data)
    {
        try
        {
            int second = data % 60;
            data /= 60;
            int minute = data % 60;
            data /= 60;
            int hour = data % 24;
            data /= 24;
            int day = (data % 31) + 1;
            data /= 31;
            int month = (data % 12) + 1;
            data /= 12;
            int year = data + 2000;

            if (year < 2000 || year > 2100)
                return null;

            DateTime dt = new DateTime(year, month, day, hour, minute, second);
            return dt.ToString("yyyy-MM-dd HH:mm:ss");
        }
        catch
        {
            return null;
        }
    }
}
