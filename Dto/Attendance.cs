using System;
using System.Collections.Generic;
using System.Text;

namespace ZKattendanceTestProject.Dto
{
    public class Attendance
    {
        public string EmpId { get; set; }
        public int Type { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public string DateTime { get; set; }
        public string StartTime { get; set; } = "00:00:00";
        public string EndTime { get; set; } = "00:00:00";
        public int ShiftId { get; set; } = 0;
        public bool IsDeleted { get; set; } = false;
        //public string CreatedAt { get; set; } = DateTime.Now

        //public Attendance()
        //{
        //    CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        //}
    }
}
