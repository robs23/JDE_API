using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace JDE_API.Models
{
    public class ProcessActionAbandonReason
    {
        public int ProcessId { get; set; }
        public int? AbandonReasonId { get; set; }
        public string AbandonReasonName { get; set; }
    }
}