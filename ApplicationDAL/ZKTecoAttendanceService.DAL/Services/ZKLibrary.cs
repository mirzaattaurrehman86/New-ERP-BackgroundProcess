using System.Net;
using System.Net.Sockets;
using System.Text;

public class ZKLibrary
{
    private UdpClient _client;
    private IPEndPoint _endpoint;

    private ushort _sessionId = 0;
    private ushort _replyId = 0;

    public ZKLibrary(string ip, int port = 4370)
    {
        _endpoint = new IPEndPoint(IPAddress.Parse(ip), port);
        _client = new UdpClient();
        _client.Client.ReceiveTimeout = 15000; // 5 seconds timeout
    }

    // ---------------- CONNECT ----------------
    //public (bool connected, string message) Connect()
    //{
    //    // Bind to local ephemeral port
    //    _client = new UdpClient(new IPEndPoint(IPAddress.Any, 0));
    //    _client.Client.ReceiveTimeout = 10000; // 10 sec

    //    byte[] command = CreateHeader(1000); // CMD_CONNECT

    //    try
    //    {
    //        _client.Send(command, command.Length, _endpoint);

    //        IPEndPoint remoteEP = _endpoint;
    //        var response = _client.Receive(ref remoteEP);

    //        if (response.Length > 0)
    //        {
    //            _sessionId = BitConverter.ToUInt16(response, 4);
    //            _replyId = BitConverter.ToUInt16(response, 6);
    //            return (true, "Connected successfully");
    //        }

    //        return (false, "No response from device");
    //    }
    //    catch (SocketException se)
    //    {
    //        return (false, $"Socket error: {se.Message}");
    //    }
    //    catch (Exception ex)
    //    {
    //        return (false, $"Error: {ex.Message}");
    //    }
    //}

    public (bool connected, string message) Connect()
    {
        byte[] command = CreateHeader(1000);
        int attempts = 3;

        for (int i = 0; i < attempts; i++)
        {
            _client.Send(command, command.Length, _endpoint);

            try
            {
                _client.Send(command, command.Length, _endpoint);

                // Use _endpoint directly for Receive
                var response = _client.Receive(ref _endpoint);
                if (response.Length > 0)
                {
                    _sessionId = BitConverter.ToUInt16(response, 4);
                    _replyId = BitConverter.ToUInt16(response, 6);
                    return (true, "Connected successfully");
                }
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.TimedOut)
            {
                // retry
            }
        }

        return (false, "No response from device after retries");
    }

    // ---------------- EXECUTE COMMAND ----------------
    public byte[] ExecCommand(ushort command, byte[] payload = null)
    {
        _replyId++;
        byte[] header = CreateHeader(command, payload);
        _client.Send(header, header.Length, _endpoint);

        try
        {
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
            return _client.Receive(ref remoteEP);
        }
        catch (Exception ex)
        {
            throw new Exception($"ExecCommand failed: {ex.Message}");
        }
    }

    // ---------------- GET ATTENDANCE ----------------
    public string[] GetAttendance()
    {
        byte[] response = ExecCommand(1005); // CMD_ATTLOG_RRQ
        if (response.Length < 16) return Array.Empty<string>();

        // Skipping header (first 16 bytes)
        int count = (response.Length - 16) / 16;
        string[] logs = new string[count];

        for (int i = 0; i < count; i++)
        {
            int offset = 16 + (i * 16);
            uint userId = BitConverter.ToUInt32(response, offset);
            uint timestamp = BitConverter.ToUInt32(response, offset + 4);
            logs[i] = $"UserID: {userId}, Time: {UnixTimeStampToDateTime(timestamp)}";
        }

        return logs;
    }

    // ---------------- GET USERS ----------------
    public string[] GetUsers()
    {
        byte[] response = ExecCommand(1005); // CMD_USERTABLE_RRQ
        if (response.Length < 16) return Array.Empty<string>();

        int count = (response.Length - 16) / 28; // user record = 28 bytes
        string[] users = new string[count];

        for (int i = 0; i < count; i++)
        {
            int offset = 16 + (i * 28);
            uint userId = BitConverter.ToUInt32(response, offset);
            string name = Encoding.ASCII.GetString(response, offset + 4, 24).Trim('\0');
            users[i] = $"ID: {userId}, Name: {name}";
        }

        return users;
    }

    // ---------------- CREATE HEADER ----------------
    private byte[] CreateHeader(ushort command, byte[] payload = null)
    {
        payload ??= new byte[0];
        byte[] header = new byte[16 + payload.Length];

        // Command
        Array.Copy(BitConverter.GetBytes(command), 0, header, 0, 2);
        // Status
        header[2] = 0; header[3] = 0;
        // Session ID
        Array.Copy(BitConverter.GetBytes(_sessionId), 0, header, 4, 2);
        // Reply ID
        Array.Copy(BitConverter.GetBytes(_replyId), 0, header, 6, 2);
        // Reserved
        for (int i = 8; i < 16; i++) header[i] = 0;

        // Payload
        Array.Copy(payload, 0, header, 16, payload.Length);

        return header;
    }

    private DateTime UnixTimeStampToDateTime(uint unixTimeStamp)
    {
        DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        return epoch.AddSeconds(unixTimeStamp).ToLocalTime();
    }
}