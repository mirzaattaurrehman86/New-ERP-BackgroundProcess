using Npgsql;
using ZKTecoAttendanceService.DTO.Dto.PostgresSQL;

//using ZKTecoAttendanceService.PostgreSQL.DTOs;
using ZKTecoAttendanceService.PostgreSQL.Infrastructure;

namespace ZKTecoAttendanceService.PostgreSQL.Services
{
    //public class AttendanceService
    //{
    //    private readonly PostgreSqlDatabase _database = new();

    //    public async Task<List<AttendanceRecordDto>> GetAllPunchesAsync()
    //    {
    //        var list = new List<AttendanceRecordDto>();

    //        await using var connection = _database.GetConnection();

    //        await connection.OpenAsync();

    //        string sql = @"SELECT
    //                            emp_code AS EmployeeCode,
    //                            upload_time AT TIME ZONE 'Asia/Karachi' AS DateTimeStamp,
    //                            CAST(upload_time AT TIME ZONE 'Asia/Karachi' AS DATE) AS DateStamp,
    //                            CAST(upload_time AT TIME ZONE 'Asia/Karachi' AS TIME) AS TimeStamp,

    //                            CAST(punch_state AS INTEGER) AS StatusId,
    //                            punch_state AS StatusName,

    //                            verify_type AS VerifyModeId,
    //                            verify_type::text AS VerifyModeName,

    //                            area_alias AS DeviceOffice,
    //                            area_alias AS DeviceOfficeName
    //                        FROM iclock_transaction
    //                        ORDER BY upload_time DESC;";

    //        await using var cmd = new NpgsqlCommand(sql, connection);

    //        await using var reader = await cmd.ExecuteReaderAsync();

    //        while (await reader.ReadAsync())
    //        {
    //            list.Add(new AttendanceRecordDto
    //            {
    //                EmployeeCode = reader.GetString(0),

    //                DateTimeStamp = reader.GetDateTime(1),

    //                DateStamp = reader.GetFieldValue<DateOnly>(2),

    //                TimeStamp = reader.GetFieldValue<TimeOnly>(3),

    //                StatusId = reader.GetInt32(4),

    //                StatusName = ZKTecoAttendanceService.helper.HelperClass.MapInOutMode(reader.GetInt32(4)),

    //                VerifyModeId = reader.GetInt32(6),

    //                VerifyModeName = ZKTecoAttendanceService.helper.HelperClass.MapVerifyMode(reader.GetInt32(6)),

    //                DeviceOffice = reader.IsDBNull(8) ? string.Empty : reader.GetString(8),

    //                DeviceOfficeName = reader.IsDBNull(9) ? string.Empty : reader.GetString(9)
    //            });
    //        }

    //        return list;
    //    }
    //    public async Task<List<AttendanceRecordDto>> GetCurrentMonthPunchesAsync()
    //    {
    //        var list = new List<AttendanceRecordDto>();

    //        await using var connection = _database.GetConnection();

    //        await connection.OpenAsync();

    //        string sql = @"
    //    SELECT
    //        emp_code AS EmployeeCode,
    //        upload_time AT TIME ZONE 'Asia/Karachi' AS DateTimeStamp,
    //        CAST(upload_time AT TIME ZONE 'Asia/Karachi' AS DATE) AS DateStamp,
    //        CAST(upload_time AT TIME ZONE 'Asia/Karachi' AS TIME) AS TimeStamp,

    //        CAST(punch_state AS INTEGER) AS StatusId,
    //        punch_state AS StatusName,

    //        verify_type AS VerifyModeId,
    //        verify_type::text AS VerifyModeName,

    //        area_alias AS DeviceOffice,
    //        area_alias AS DeviceOfficeName
    //    FROM iclock_transaction
    //    WHERE upload_time >= date_trunc('month', CURRENT_TIMESTAMP)
    //      AND upload_time < date_trunc('month', CURRENT_TIMESTAMP) + interval '1 month'
    //    ORDER BY upload_time DESC;";

    //        await using var cmd = new NpgsqlCommand(sql, connection);

    //        await using var reader = await cmd.ExecuteReaderAsync();

    //        while (await reader.ReadAsync())
    //        {
    //            list.Add(new AttendanceRecordDto
    //            {
    //                EmployeeCode = reader.GetString(0),

    //                DateTimeStamp = reader.GetDateTime(1),

    //                DateStamp = reader.GetFieldValue<DateOnly>(2),

    //                TimeStamp = reader.GetFieldValue<TimeOnly>(3),

    //                StatusId = reader.GetInt32(4),

    //                StatusName = ZKTecoAttendanceService.helper.HelperClass.MapInOutMode(reader.GetInt32(4)),

    //                VerifyModeId = reader.GetInt32(6),

    //                VerifyModeName = ZKTecoAttendanceService.helper.HelperClass.MapVerifyMode(reader.GetInt32(6)),

    //                DeviceOffice = reader.IsDBNull(8) ? string.Empty : reader.GetString(8),

    //                DeviceOfficeName = reader.IsDBNull(9) ? string.Empty : reader.GetString(9)
    //            });
    //        }

    //        return list;
    //    }
    //    public async Task<List<AttendanceRecordDto>> GetCurrentYearPunchesAsync()
    //    {
    //        var list = new List<AttendanceRecordDto>();

    //        await using var connection = _database.GetConnection();

    //        await connection.OpenAsync();

    //        string sql = @"
    //    SELECT
    //        emp_code AS EmployeeCode,
    //        upload_time AT TIME ZONE 'Asia/Karachi' AS DateTimeStamp,
    //        CAST(upload_time AT TIME ZONE 'Asia/Karachi' AS DATE) AS DateStamp,
    //        CAST(upload_time AT TIME ZONE 'Asia/Karachi' AS TIME) AS TimeStamp,

    //        CAST(punch_state AS INTEGER) AS StatusId,
    //        punch_state AS StatusName,

    //        verify_type AS VerifyModeId,
    //        verify_type::text AS VerifyModeName,

    //        area_alias AS DeviceOffice,
    //        area_alias AS DeviceOfficeName
    //    FROM iclock_transaction
    //    WHERE upload_time >= date_trunc('year', CURRENT_TIMESTAMP)
    //      AND upload_time < date_trunc('year', CURRENT_TIMESTAMP) + interval '1 year'
    //    ORDER BY upload_time DESC;";

    //        await using var cmd = new NpgsqlCommand(sql, connection);

    //        await using var reader = await cmd.ExecuteReaderAsync();

    //        while (await reader.ReadAsync())
    //        {
    //            list.Add(new AttendanceRecordDto
    //            {
    //                EmployeeCode = reader.GetString(0),

    //                DateTimeStamp = reader.GetDateTime(1),

    //                DateStamp = reader.GetFieldValue<DateOnly>(2),

    //                TimeStamp = reader.GetFieldValue<TimeOnly>(3),

    //                StatusId = reader.GetInt32(4),

    //                StatusName = ZKTecoAttendanceService.helper.HelperClass.MapInOutMode(reader.GetInt32(4)),

    //                VerifyModeId = reader.GetInt32(6),

    //                VerifyModeName = ZKTecoAttendanceService.helper.HelperClass.MapVerifyMode(reader.GetInt32(6)),

    //                DeviceOffice = reader.IsDBNull(8) ? string.Empty : reader.GetString(8),

    //                DeviceOfficeName = reader.IsDBNull(9) ? string.Empty : reader.GetString(9)
    //            });
    //        }

    //        return list;
    //    }
    //    public async Task<List<AttendanceRecordDto>> GetCurrentAndPreviousYearPunchesAsync()
    //    {
    //        var list = new List<AttendanceRecordDto>();

    //        await using var connection = _database.GetConnection();

    //        await connection.OpenAsync();

    //        string sql = @"
    //    SELECT
    //        emp_code AS EmployeeCode,
    //        upload_time AT TIME ZONE 'Asia/Karachi' AS DateTimeStamp,
    //        CAST(upload_time AT TIME ZONE 'Asia/Karachi' AS DATE) AS DateStamp,
    //        CAST(upload_time AT TIME ZONE 'Asia/Karachi' AS TIME) AS TimeStamp,

    //        CAST(punch_state AS INTEGER) AS StatusId,
    //        punch_state AS StatusName,

    //        verify_type AS VerifyModeId,
    //        verify_type::text AS VerifyModeName,

    //        area_alias AS DeviceOffice,
    //        area_alias AS DeviceOfficeName
    //    FROM iclock_transaction
    //    WHERE upload_time >= date_trunc('year', CURRENT_TIMESTAMP) - interval '1 year'
    //      AND upload_time < date_trunc('year', CURRENT_TIMESTAMP) + interval '1 year'
    //    ORDER BY upload_time DESC;";

    //        await using var cmd = new NpgsqlCommand(sql, connection);

    //        await using var reader = await cmd.ExecuteReaderAsync();

    //        while (await reader.ReadAsync())
    //        {
    //            list.Add(new AttendanceRecordDto
    //            {
    //                EmployeeCode = reader.GetString(0),

    //                DateTimeStamp = reader.GetDateTime(1),

    //                DateStamp = reader.GetFieldValue<DateOnly>(2),

    //                TimeStamp = reader.GetFieldValue<TimeOnly>(3),

    //                StatusId = reader.GetInt32(4),

    //                StatusName = ZKTecoAttendanceService.helper.HelperClass.MapInOutMode(reader.GetInt32(4)),

    //                VerifyModeId = reader.GetInt32(6),

    //                VerifyModeName = ZKTecoAttendanceService.helper.HelperClass.MapVerifyMode(reader.GetInt32(6)),

    //                DeviceOffice = reader.IsDBNull(8) ? string.Empty : reader.GetString(8),

    //                DeviceOfficeName = reader.IsDBNull(9) ? string.Empty : reader.GetString(9)
    //            });
    //        }

    //        return list;
    //    }
    //    public async Task<List<AttendanceRecordDto>> GetPunchesByDateRangeAsync(DateOnly startDate, DateOnly endDate)
    //    {
    //        var list = new List<AttendanceRecordDto>();

    //        await using var connection = _database.GetConnection();

    //        await connection.OpenAsync();

    //        string sql = @"
    //    SELECT
    //        emp_code AS EmployeeCode,
    //        upload_time AT TIME ZONE 'Asia/Karachi' AS DateTimeStamp,
    //        CAST(upload_time AT TIME ZONE 'Asia/Karachi' AS DATE) AS DateStamp,
    //        CAST(upload_time AT TIME ZONE 'Asia/Karachi' AS TIME) AS TimeStamp,

    //        CAST(punch_state AS INTEGER) AS StatusId,
    //        punch_state AS StatusName,

    //        verify_type AS VerifyModeId,
    //        verify_type::text AS VerifyModeName,

    //        area_alias AS DeviceOffice,
    //        area_alias AS DeviceOfficeName
    //    FROM iclock_transaction
    //    WHERE upload_time >= @startDate
    //      AND upload_time < @endDate
    //    ORDER BY upload_time DESC;";

    //        await using var cmd = new NpgsqlCommand(sql, connection);

    //        // Inclusive start date
    //        cmd.Parameters.AddWithValue("startDate", startDate.ToDateTime(TimeOnly.MinValue));

    //        // Exclusive end date (end date + 1 day)
    //        cmd.Parameters.AddWithValue("endDate", endDate.AddDays(1).ToDateTime(TimeOnly.MinValue));

    //        await using var reader = await cmd.ExecuteReaderAsync();

    //        while (await reader.ReadAsync())
    //        {
    //            list.Add(new AttendanceRecordDto
    //            {
    //                EmployeeCode = reader.GetString(0),

    //                DateTimeStamp = reader.GetDateTime(1),

    //                DateStamp = reader.GetFieldValue<DateOnly>(2),

    //                TimeStamp = reader.GetFieldValue<TimeOnly>(3),

    //                StatusId = reader.GetInt32(4),

    //                StatusName = ZKTecoAttendanceService.helper.HelperClass.MapInOutMode(reader.GetInt32(4)),

    //                VerifyModeId = reader.GetInt32(6),

    //                VerifyModeName = ZKTecoAttendanceService.helper.HelperClass.MapVerifyMode(reader.GetInt32(6)),

    //                DeviceOffice = reader.IsDBNull(8) ? string.Empty : reader.GetString(8),

    //                DeviceOfficeName = reader.IsDBNull(9) ? string.Empty : reader.GetString(9)
    //            });
    //        }

    //        return list;
    //    }
    //    public async Task<List<AttendanceRecordDto>> GetAllPunchesByEmployeeAsync(string employeeCode)
    //    {
    //        var list = new List<AttendanceRecordDto>();

    //        await using var connection = _database.GetConnection();

    //        await connection.OpenAsync();

    //        string sql = @"SELECT emp_code AS EmployeeCode,
    //                              upload_time AT TIME ZONE 'Asia/Karachi' AS DateTimeStamp,
    //                              CAST(upload_time AT TIME ZONE 'Asia/Karachi' AS DATE) AS DateStamp,
    //                              CAST(upload_time AT TIME ZONE 'Asia/Karachi' AS TIME) AS TimeStamp,

    //                              CAST(punch_state AS INTEGER) AS StatusId,
    //                              punch_state AS StatusName,

    //                              verify_type AS VerifyModeId,
    //                              verify_type::text AS VerifyModeName,

    //                              area_alias AS DeviceOffice,
    //                              area_alias AS DeviceOfficeName
    //                              FROM iclock_transaction
    //                              WHERE emp_code = @emp
    //                              ORDER BY upload_time DESC;";

    //        await using var cmd = new NpgsqlCommand(sql, connection);

    //        cmd.Parameters.AddWithValue("emp", employeeCode);

    //        await using var reader = await cmd.ExecuteReaderAsync();

    //        while (await reader.ReadAsync())
    //        {
    //            list.Add(new AttendanceRecordDto
    //            {
    //                EmployeeCode = reader.GetString(0),

    //                DateTimeStamp = reader.GetDateTime(1),

    //                DateStamp = reader.GetFieldValue<DateOnly>(2),

    //                TimeStamp = reader.GetFieldValue<TimeOnly>(3),

    //                StatusId = reader.GetInt32(4),

    //                StatusName = ZKTecoAttendanceService.helper.HelperClass.MapInOutMode(reader.GetInt32(4)),

    //                VerifyModeId = reader.GetInt32(6),

    //                VerifyModeName = ZKTecoAttendanceService.helper.HelperClass.MapVerifyMode(reader.GetInt32(6)),

    //                DeviceOffice = reader.IsDBNull(8) ? "" : reader.GetString(8),

    //                DeviceOfficeName = reader.IsDBNull(9) ? "" : reader.GetString(9)
    //            });
    //        }

    //        return list;
    //    }
    //    public async Task<List<AttendanceRecordDto>> GetCurrentMonthPunchesByEmployeeAsync(string employeeCode)
    //    {
    //        var list = new List<AttendanceRecordDto>();

    //        await using var connection = _database.GetConnection();

    //        await connection.OpenAsync();

    //        string sql = @"SELECT emp_code AS EmployeeCode,
    //                              upload_time AT TIME ZONE 'Asia/Karachi' AS DateTimeStamp,
    //                              CAST(upload_time AT TIME ZONE 'Asia/Karachi' AS DATE) AS DateStamp,
    //                              CAST(upload_time AT TIME ZONE 'Asia/Karachi' AS TIME) AS TimeStamp,

    //                              CAST(punch_state AS INTEGER) AS StatusId,
    //                              punch_state AS StatusName,

    //                              verify_type AS VerifyModeId,
    //                              verify_type::text AS VerifyModeName,

    //                              area_alias AS DeviceOffice,
    //                              area_alias AS DeviceOfficeName
    //                              FROM iclock_transaction
    //                              WHERE emp_code = @emp
    //                                    AND upload_time >= date_trunc('month', CURRENT_TIMESTAMP)
    //                                    AND upload_time < date_trunc('month', CURRENT_TIMESTAMP) + interval '1 month'
    //                              ORDER BY upload_time DESC;";

    //        await using var cmd = new NpgsqlCommand(sql, connection);

    //        cmd.Parameters.AddWithValue("emp", employeeCode);

    //        await using var reader = await cmd.ExecuteReaderAsync();

    //        while (await reader.ReadAsync())
    //        {
    //            list.Add(new AttendanceRecordDto
    //            {
    //                EmployeeCode = reader.GetString(0),

    //                DateTimeStamp = reader.GetDateTime(1),

    //                DateStamp = reader.GetFieldValue<DateOnly>(2),

    //                TimeStamp = reader.GetFieldValue<TimeOnly>(3),

    //                StatusId = reader.GetInt32(4),

    //                StatusName = ZKTecoAttendanceService.helper.HelperClass.MapInOutMode(reader.GetInt32(4)),

    //                VerifyModeId = reader.GetInt32(6),

    //                VerifyModeName = ZKTecoAttendanceService.helper.HelperClass.MapVerifyMode(reader.GetInt32(6)),

    //                DeviceOffice = reader.IsDBNull(8) ? "" : reader.GetString(8),

    //                DeviceOfficeName = reader.IsDBNull(9) ? "" : reader.GetString(9)
    //            });
    //        }

    //        return list;
    //    }
    //    public async Task<List<AttendanceRecordDto>> GetEmployeePunchesByDateRangeAsync(string employeeCode, DateOnly startDate, DateOnly endDate)
    //    {
    //        var list = new List<AttendanceRecordDto>();

    //        await using var connection = _database.GetConnection();

    //        await connection.OpenAsync();

    //        string sql = @"
    //    SELECT
    //        emp_code AS EmployeeCode,
    //        upload_time AT TIME ZONE 'Asia/Karachi' AS DateTimeStamp,
    //        CAST(upload_time AT TIME ZONE 'Asia/Karachi' AS DATE) AS DateStamp,
    //        CAST(upload_time AT TIME ZONE 'Asia/Karachi' AS TIME) AS TimeStamp,

    //        CAST(punch_state AS INTEGER) AS StatusId,
    //        punch_state AS StatusName,

    //        verify_type AS VerifyModeId,
    //        verify_type::text AS VerifyModeName,

    //        area_alias AS DeviceOffice,
    //        area_alias AS DeviceOfficeName
    //    FROM iclock_transaction
    //    WHERE emp_code = @emp
    //          AND upload_time >= @startDate
    //          AND upload_time < @endDate
    //    ORDER BY upload_time DESC;";

    //        await using var cmd = new NpgsqlCommand(sql, connection);

    //        // Inclusive start date
    //        cmd.Parameters.AddWithValue("startDate", startDate.ToDateTime(TimeOnly.MinValue));

    //        // Exclusive end date (end date + 1 day)
    //        cmd.Parameters.AddWithValue("endDate", endDate.AddDays(1).ToDateTime(TimeOnly.MinValue));

    //        cmd.Parameters.AddWithValue("emp", employeeCode);

    //        await using var reader = await cmd.ExecuteReaderAsync();

    //        while (await reader.ReadAsync())
    //        {
    //            list.Add(new AttendanceRecordDto
    //            {
    //                EmployeeCode = reader.GetString(0),

    //                DateTimeStamp = reader.GetDateTime(1),

    //                DateStamp = reader.GetFieldValue<DateOnly>(2),

    //                TimeStamp = reader.GetFieldValue<TimeOnly>(3),

    //                StatusId = reader.GetInt32(4),

    //                StatusName = ZKTecoAttendanceService.helper.HelperClass.MapInOutMode(reader.GetInt32(4)),

    //                VerifyModeId = reader.GetInt32(6),

    //                VerifyModeName = ZKTecoAttendanceService.helper.HelperClass.MapVerifyMode(reader.GetInt32(6)),

    //                DeviceOffice = reader.IsDBNull(8) ? string.Empty : reader.GetString(8),

    //                DeviceOfficeName = reader.IsDBNull(9) ? string.Empty : reader.GetString(9)
    //            });
    //        }

    //        return list;
    //    }
    //}

    public class AttendanceService
    {
        private static readonly TimeZoneInfo PakistanTimeZone =
    OperatingSystem.IsWindows()
        ? TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time")
        : TimeZoneInfo.FindSystemTimeZoneById("Asia/Karachi");

        private readonly PostgreSqlDatabase _database = new();
        private const string SelectClause = @"SELECT
                                               emp_code AS EmployeeCode,
                                               punch_time AT TIME ZONE 'Asia/Karachi' AS DateTimeStamp,
                                               CAST(punch_time AT TIME ZONE 'Asia/Karachi' AS DATE) AS DateStamp,
                                               CAST(punch_time AT TIME ZONE 'Asia/Karachi' AS TIME) AS TimeStamp,
                                               CAST(punch_state AS INTEGER) AS StatusId,
                                               punch_state AS StatusName,
                                               verify_type AS VerifyModeId,
                                               verify_type::text AS VerifyModeName,
                                               area_alias AS DeviceOffice,
                                               area_alias AS DeviceOfficeName
                                            FROM iclock_transaction ";
        private async Task<List<AttendanceRecordDto>> ExecuteQueryAsync(string where = "", string order = " ORDER BY punch_time DESC;", params NpgsqlParameter[] parameters)
        {
            var list = new List<AttendanceRecordDto>();

            await using var connection = _database.GetConnection();
            await connection.OpenAsync();

            await using var cmd = new NpgsqlCommand(SelectClause + where + order, connection);
            if (parameters.Length > 0)
                cmd.Parameters.AddRange(parameters);

            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
                list.Add(Map(reader));

            return list;
        }
        private static AttendanceRecordDto Map(NpgsqlDataReader r) => new()
        {
            EmployeeCode = r.GetString(0),
            DateTimeStamp = r.GetDateTime(1),
            DateStamp = r.GetFieldValue<DateOnly>(2),
            TimeStamp = r.GetFieldValue<TimeOnly>(3),
            StatusId = r.GetInt32(4),
            StatusName = ZKTecoAttendanceService.DTO.helper.HelperClass.MapInOutMode(r.GetInt32(4)),
            VerifyModeId = r.GetInt32(6),
            VerifyModeName = ZKTecoAttendanceService.DTO.helper.HelperClass.MapVerifyMode(r.GetInt32(6)),
            DeviceOffice = r.IsDBNull(8) ? string.Empty : r.GetString(8),
            DeviceOfficeName = r.IsDBNull(9) ? string.Empty : r.GetString(9)
        };
        public Task<List<AttendanceRecordDto>> GetAllPunchesAsync() => ExecuteQueryAsync();
        public Task<List<AttendanceRecordDto>> GetCurrentMonthPunchesAsync() =>
                                ExecuteQueryAsync(@"WHERE punch_time >=
                                (
                                    date_trunc('month', CURRENT_TIMESTAMP AT TIME ZONE 'Asia/Karachi')
                                    AT TIME ZONE 'Asia/Karachi'
                                )
                                AND punch_time <
                                (
                                    (
                                        date_trunc('month', CURRENT_TIMESTAMP AT TIME ZONE 'Asia/Karachi')
                                        + interval '1 month'
                                    )
                                    AT TIME ZONE 'Asia/Karachi'
                                )");
        public Task<List<AttendanceRecordDto>> GetCurrentYearPunchesAsync() =>
                                ExecuteQueryAsync(@"WHERE punch_time >=
                                (
                                    date_trunc('year', CURRENT_TIMESTAMP AT TIME ZONE 'Asia/Karachi')
                                    AT TIME ZONE 'Asia/Karachi'
                                )
                                AND punch_time <
                                (
                                    (
                                        date_trunc('year', CURRENT_TIMESTAMP AT TIME ZONE 'Asia/Karachi')
                                        + interval '1 year'
                                    )
                                    AT TIME ZONE 'Asia/Karachi'
                                )");
        public Task<List<AttendanceRecordDto>> GetCurrentAndPreviousYearPunchesAsync() =>
                                ExecuteQueryAsync(@"WHERE punch_time >=
                                (
                                    (date_trunc('year', CURRENT_TIMESTAMP AT TIME ZONE 'Asia/Karachi') - interval '1 year')
                                    AT TIME ZONE 'Asia/Karachi'
                                )
                                AND punch_time <
                                (
                                    (
                                        date_trunc('year', CURRENT_TIMESTAMP AT TIME ZONE 'Asia/Karachi')
                                        + interval '1 year'
                                    )
                                    AT TIME ZONE 'Asia/Karachi'
                                )");
        public Task<List<AttendanceRecordDto>> GetPunchesByDateRangeAsync(DateOnly start, DateOnly end)
        {
            var startUtc = TimeZoneInfo.ConvertTimeToUtc(
                start.ToDateTime(TimeOnly.MinValue),
                TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time"));

            var endUtc = TimeZoneInfo.ConvertTimeToUtc(
                end.AddDays(1).ToDateTime(TimeOnly.MinValue),
                TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time"));

            return ExecuteQueryAsync(
                @"WHERE punch_time >= @startDate
                        AND punch_time < @endDate",
                " ORDER BY punch_time DESC;",
                new NpgsqlParameter("startDate", startUtc),
                new NpgsqlParameter("endDate", endUtc));
        }
        public Task<List<AttendanceRecordDto>> GetAllPunchesByEmployeeAsync(string employeeCode) =>
            ExecuteQueryAsync("WHERE emp_code=@emp", " ORDER BY punch_time DESC;",
                new NpgsqlParameter("emp", employeeCode));
        public Task<List<AttendanceRecordDto>> GetCurrentMonthPunchesByEmployeeAsync(string employeeCode) =>
                                    ExecuteQueryAsync(@"WHERE emp_code = @emp
                                    AND punch_time >=
                                    (
                                        date_trunc('month', CURRENT_TIMESTAMP AT TIME ZONE 'Asia/Karachi')
                                        AT TIME ZONE 'Asia/Karachi'
                                    )
                                    AND punch_time <
                                    (
                                        (
                                            date_trunc('month', CURRENT_TIMESTAMP AT TIME ZONE 'Asia/Karachi')
                                            + interval '1 month'
                                        )
                                        AT TIME ZONE 'Asia/Karachi'
                                    )", " ORDER BY punch_time DESC;",
             new NpgsqlParameter("emp", employeeCode));
        public Task<List<AttendanceRecordDto>> GetEmployeePunchesByDateRangeAsync(string employeeCode, DateOnly start, DateOnly end)
        {
            var startUtc = TimeZoneInfo.ConvertTimeToUtc(
                start.ToDateTime(TimeOnly.MinValue),
                PakistanTimeZone);

            var endUtc = TimeZoneInfo.ConvertTimeToUtc(
                end.AddDays(1).ToDateTime(TimeOnly.MinValue),
                PakistanTimeZone);

            return ExecuteQueryAsync(@"WHERE emp_code = @emp
                                      AND punch_time >= @startDate
                                      AND punch_time < @endDate",
                                            " ORDER BY punch_time DESC;",
                new NpgsqlParameter("emp", employeeCode),
                new NpgsqlParameter("startDate", startUtc),
                new NpgsqlParameter("endDate", endUtc));
        }
    }
}