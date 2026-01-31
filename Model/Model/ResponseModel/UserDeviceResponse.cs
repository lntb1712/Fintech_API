using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.ResponseModel
{
    public class UserDeviceResponse
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string DeviceUUID { get; set; } = null!;
        public string? DevicePlatform { get; set; }
        public string? DeviceOs { get; set; }
        public string? DeviceModel { get; set; }
        public string? DeviceName { get; set; }
        public DateTime? LastLogin { get; set; }
        public bool? IsActive { get; set; }
    }
}