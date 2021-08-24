using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace JDE_API.Models
{
    public class ProcessAssign
    {
        public int ProcessAssignId { get; set; }
        public Nullable<int> UserId { get; set; }
        public string UserName { get; set; }
        public Nullable<int> ProcessId { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public Nullable<int> LmBy { get; set; }
        public Nullable<System.DateTime> LmOn { get; set; }
        public Nullable<int> TenantId { get; set; }
    }
}