using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using ZKTecoAttendanceService.Infrastructure;

namespace ZKTecoAttendanceService.Services.ProcessLock
{
    public class AttendanceProcessLock
    {
        private readonly string _connectionString;

        public AttendanceProcessLock(string connectionString)
        {
            _connectionString = connectionString;
        }

        public SqlConnection Connection { get; private set; }

        public bool Acquire()
        {
            Connection = new SqlConnection(_connectionString);

            Connection.Open();

            using var cmd = new SqlCommand(
                @"DECLARE @Result INT;

              EXEC @Result = sp_getapplock
                    @Resource = 'AttendanceSync',
                    @LockMode = 'Exclusive',
                    @LockOwner = 'Session',
                    @LockTimeout = 0;

              SELECT @Result;", Connection);

            var result = Convert.ToInt32(cmd.ExecuteScalar());

            LogInfo(AttendanceDeviceOffice.All, "N/A", "N/A", DateTime.Now, $"Attempt to acquire lock returned: {result}");

            return result >= 0;
        }

        public void Release()
        {
            //if (Connection != null)
            //{
            //    Connection.Close();
            //    Connection.Dispose();
            //}
        }

        public void releaseAppLock()
        {
            if (Connection == null)
            {
                //Connection = new SqlConnection(_connectionString);
                //Connection.Open();
                return;
            }
            try
            {
                using var cmd = new SqlCommand(
                    @"EXEC sp_releaseapplock
                @Resource = 'AttendanceSync',
                @LockOwner = 'Session';",
                    Connection);

                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {

            }
            finally
            {
                Connection.Close();
                Connection.Dispose();
                Connection = null;
            }
        }

        static void LogInfo(AttendanceDeviceOffice deviceOffice, string machineIP, string machinePort, DateTime dateTimeStamp, string message)
        {
            var db = new DatabaseContext();

            db.SaveAttendanceBackgroundServiceLog("[Info]", deviceOffice, machineIP, machinePort, dateTimeStamp, message);

            Console.WriteLine($"[Info] - Office:{deviceOffice} - MachineIP:{machineIP} - " + $"MachinePort:{machinePort} - DateTimeStamp:{dateTimeStamp} - Message:{message}\n");
        }
    }
}