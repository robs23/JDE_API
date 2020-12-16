using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace JDE_API.Models
{
    public class File
    {
        public int FileId { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public string Token { get; set; }
        public string Link { get; set; }
        public bool IsUploaded { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CreatedByName { get; set; }
        public int? TenantId { get; set; }
        public string TenantName { get; set; }
        public string Type { get; set; }
        public int? LmBy { get; set; }
        public DateTime? LmOn { get; set; }
        public long Size { get; set; }
    }
}