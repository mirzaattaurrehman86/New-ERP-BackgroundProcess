using System;
using System.Collections.Generic;
using System.Text;
using ZKTecoAttendanceService.Dto;

namespace ZKTecoAttendanceService.Services.AttendanceDeviceService
{
    public static class DeviceDataService
    {
        public static AttendanceDeviceDto GetDevice(AttendanceDeviceOffice attendanceDevice, int deviceId)
        {
            if (attendanceDevice == AttendanceDeviceOffice.ProSoft && deviceId == 1)
            {
                return new AttendanceDeviceDto
                {
                    deviceOrder = 1,

                    name = "K50 Prosoft",
                    ip = "192.168.36.203",
                    serialNumber = "CQU2231860913",
                    port = 4370,
                    deviceNumber = 1,

                    office = AttendanceDeviceOffice.ProSoft,
                };
            }
            
            //**
            else if (attendanceDevice == AttendanceDeviceOffice.ShamasabadOffice && deviceId == 1)
            {
                //Team Z Floor
                return new AttendanceDeviceDto
                {
                    deviceOrder = 2,

                    name = "uFace800/ID",
                    ip = "192.168.111.20",
                    serialNumber = "6160062162605",
                    port = 4370,
                    deviceNumber = 1,

                    office = AttendanceDeviceOffice.ShamasabadOffice,
                };
            }
            else if (attendanceDevice == AttendanceDeviceOffice.ShamasabadOffice && deviceId == 2)
            {
                //Team A&H Floor
                return new AttendanceDeviceDto
                {
                    deviceOrder = 3,

                    name = "uFace800/ID",
                    ip = "192.168.111.21",
                    serialNumber = "AF4C210360084",
                    port = 4370,
                    deviceNumber = 1,

                    office = AttendanceDeviceOffice.ShamasabadOffice,
                };
            }
            else if (attendanceDevice == AttendanceDeviceOffice.ShamasabadOffice && deviceId == 3)
            {
                //Team E&N
                return new AttendanceDeviceDto
                {
                    deviceOrder = 4,

                    name = "uFace800/ID",
                    ip = "192.168.111.22",
                    serialNumber = "AF4C224060115",
                    port = 4370,
                    deviceNumber = 1,

                    office = AttendanceDeviceOffice.ShamasabadOffice,
                };
            }
            else if (attendanceDevice == AttendanceDeviceOffice.ShamasabadOffice && deviceId == 4)
            {
                //Team M&U Floor
                return new AttendanceDeviceDto
                {
                    deviceOrder = 5,

                    name = "uFace800/ID",
                    ip = "192.168.111.23",
                    serialNumber = "6160062162649",
                    port = 4370,
                    deviceNumber = 1,

                    office = AttendanceDeviceOffice.ShamasabadOffice,
                };
            }
            //**
            else if (attendanceDevice == AttendanceDeviceOffice.Pharmacy && deviceId == 1)
            {
                //Team M&U Floor
                return new AttendanceDeviceDto
                {
                    deviceOrder = 6,
                    name = string.Empty,
                    ip = "192.168.106.20",
                    serialNumber = string.Empty,
                    port = 4370,
                    deviceNumber = 1,
                    office = AttendanceDeviceOffice.Pharmacy,
                };
            }
            //**
            else if (attendanceDevice == AttendanceDeviceOffice.HeadOffice && deviceId == 1)
            {
                return new AttendanceDeviceDto
                {
                    deviceOrder = 7,

                    name = "uFace800 Plus/ID",
                    ip = "192.168.100.225",
                    serialNumber = "CKPG231860095",
                    port = 4370,
                    deviceNumber = 1,
                    office = AttendanceDeviceOffice.HeadOffice,
                };
            }
            else if (attendanceDevice == AttendanceDeviceOffice.HeadOffice && deviceId == 2)
            {
                return new AttendanceDeviceDto
                {
                    deviceOrder = 8,

                    name = "uface800/ID",
                    ip = "192.168.100.220",
                    serialNumber = "AF4C211960254",
                    port = 4370,
                    deviceNumber = 1,

                    office = AttendanceDeviceOffice.HeadOffice,
                };
            }
            else if (attendanceDevice == AttendanceDeviceOffice.HeadOffice && deviceId == 3)
            {
                return new AttendanceDeviceDto
                {
                    deviceOrder = 9,
                    name = "uface800 Plus/ID",
                    ip = "192.168.100.226",
                    serialNumber = "CKPG231860264",
                    port = 4370,
                    deviceNumber = 1,

                    office = AttendanceDeviceOffice.HeadOffice,
                };
            }
            else if (attendanceDevice == AttendanceDeviceOffice.HeadOffice && deviceId == 4)
            {
                return new AttendanceDeviceDto
                {
                    deviceOrder = 10,

                    name = "uFace800 Plus",
                    ip = "192.168.100.221",
                    serialNumber = "JYM6242300127",
                    port = 4370,
                    deviceNumber = 1,

                    office = AttendanceDeviceOffice.HeadOffice,
                };
            }
            return new AttendanceDeviceDto { };
        }

        public static List<AttendanceDeviceDto> GetAllDevices()
        {
            var devices = new List<AttendanceDeviceDto>();
            foreach (AttendanceDeviceOffice device in Enum.GetValues(typeof(AttendanceDeviceOffice)))
            {
                for (int i = 1; i <= 4; i++)
                {
                    var deviceInfo = GetDevice(device, i);
                    if (!string.IsNullOrEmpty(deviceInfo.ip))
                    {
                        devices.Add(deviceInfo);
                    }
                }
            }
            return devices.OrderBy(d => d.deviceOrder).ToList();
        }
    }
}
