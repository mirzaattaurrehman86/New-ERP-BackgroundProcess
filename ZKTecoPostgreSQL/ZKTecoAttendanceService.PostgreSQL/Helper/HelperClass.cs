//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace ZKTecoAttendanceService.PostgreSQL.Helper
//{
//    public static class HelperClass
//    {
//        public static string MapVerifyMode(int mode)
//        {
//            return mode switch
//            {
//                0 => "Password",
//                1 => "Fingerprint",
//                2 => "Card",
//                3 => "Fingerprint + Password",
//                4 => "Card + Password",
//                15 => "Face",
//                _ => $"Unknown({mode})"
//            };
//        }

//        public static string MapInOutMode(int mode)
//        {
//            return mode switch
//            {
//                0 => "Check-In",
//                1 => "Check-Out",
//                2 => "Break-Out",
//                3 => "Break-In",
//                4 => "Overtime-In",
//                5 => "Overtime-Out",
//                _ => $"Unknown({mode})"
//            };
//        }

//    }
//}
