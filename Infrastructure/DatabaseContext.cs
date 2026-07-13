using Microsoft.Data.SqlClient;
using System;
using System.Data;
using ZKattendanceTestProject.Dto;
using ZKTecoAttendanceService.Dto;

namespace ZKattendanceTestProject.Infrastructure
{
    public class DatabaseContext
    {
        public string connectionString = "Server=PROLPT\\MSSQLSERVER01;Database=BellMedExErpDb;User Id=sa;Password=phLpeh8f@0921;Encrypt=True;TrustServerCertificate=True;";
        public void SaveAttendanceRecord(List<EmployeeAttendanceDto> attendanceLogs, AttendanceDeviceOffice deviceOffice)
        {
            if (attendanceLogs == null || attendanceLogs.Count == 0)
                return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand("SaveAllAttendanceMachineLogs", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    var table = new DataTable();
                    table.Columns.Add("MachineIP", typeof(string));
                    table.Columns.Add("MachinePort", typeof(int));
                    table.Columns.Add("EmployeeCode", typeof(string));
                    table.Columns.Add("DateTimeStamp", typeof(DateTime));
                    table.Columns.Add("DateStamp", typeof(DateTime));
                    table.Columns.Add("TimeStamp", typeof(TimeSpan));
                    table.Columns.Add("StatusId", typeof(int));
                    table.Columns.Add("StatusName", typeof(string));
                    table.Columns.Add("VerifyModeId", typeof(int));
                    table.Columns.Add("VerifyModeName", typeof(string));

                    foreach (var log in attendanceLogs)
                    {
                        table.Rows.Add(
                            log.MachineIP,
                            log.MachinePort,
                            log.EmployeeCode,
                            log.DateTimeStamp,
                            log.DateTimeStamp.Date,
                            log.DateTimeStamp.TimeOfDay,
                            log.StatusId,
                            log.StatusName,
                            log.VerifyModeId,
                            log.VerifyModeName
                        );
                    }

                    var param = cmd.Parameters.AddWithValue("@AttendanceLogs", table);
                    param.SqlDbType = SqlDbType.Structured;

                    param.SqlDbType = SqlDbType.Structured;
                    param.TypeName = "dbo.AttendanceTableType";

                    // Enum parameter
                    cmd.Parameters.AddWithValue("@DeviceOffice", (int)deviceOffice);
                    cmd.Parameters.AddWithValue("@DeviceOfficeName", deviceOffice.ToString());

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                // Log properly instead of swallowing
                throw new Exception("Error saving attendance records", ex);
            }
        }
        public void RunAttendanceSync()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand("ProcessMonthlyAttendance", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error executing ProcessMonthlyAttendance stored procedure.", ex);
            }
        }

        public void SaveAttendanceBackgroundServiceLog(string logType, AttendanceDeviceOffice deviceOffice, string machineIP, string machinePort, DateTime dateTimeStamp, string message)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand("SaveAttendanceBackgroundServiceLog", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@LogType", logType);
                    cmd.Parameters.AddWithValue("@DeviceOffice", (int)deviceOffice);
                    cmd.Parameters.AddWithValue("@DeviceOfficeName", deviceOffice.ToString());
                    cmd.Parameters.AddWithValue("@MachineIP", machineIP);
                    cmd.Parameters.AddWithValue("@MachinePort", machinePort);
                    cmd.Parameters.AddWithValue("@DateTimeStamp", dateTimeStamp);
                    cmd.Parameters.AddWithValue("@Message", message);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error saving attendance background service log.", ex);
            }
        }

        public async Task<List<AttendanceDeviceDto>> GetAttendanceDevicesAsync()
        {
            var devices = new List<AttendanceDeviceDto>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand("sp_GetAttendanceDevices", connection))
            {
                command.CommandType = CommandType.StoredProcedure;

                await connection.OpenAsync();

                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        devices.Add(new AttendanceDeviceDto
                        {
                            office = (AttendanceDeviceOffice)reader.GetInt32(reader.GetOrdinal("EnumOffice")),
                            name = reader["name"].ToString()!,
                            ip = reader["ip"].ToString()!,
                            port = Convert.ToInt32(reader["port"]),
                            serialNumber = reader["serialNumber"].ToString()!,
                            deviceNumber = Convert.ToInt32(reader["deviceNumber"]),
                            deviceOrder = reader["deviceOrder"] == DBNull.Value
                                ? 0/*null*/
                                : Convert.ToInt32(reader["deviceOrder"])
                        });
                    }
                }
            }
            return devices;
        }
    }
}
