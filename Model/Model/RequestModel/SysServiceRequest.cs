using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.RequestModel
{
    public class SysAppVersionRequest
    {
        public string AppName { get; set; } = string.Empty;
        public string Platform { get; set; } = string.Empty;
        public string VersionName { get; set; } = string.Empty;
        public int Version { get; set; }
        public string ReleaseNotes { get; set; } = string.Empty;
        public bool IsAwaitingApproval { get; set; }
        public bool IsForceDev { get; set; } = false;
        public bool IsForceUpdate { get; set; } = false;
    }
}