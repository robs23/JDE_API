using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace JDE_API.Models
{
    public class ImageInfo
    {
        public string Name { get; set; }
        public string LinkName { get; set; }
        public int? ProcessId { get; set; }
        public int? PlaceId { get; set; }
        public int? PartId { get; set; }
        public string Description { get; set; }
        public string CreatedByName { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int TenantId { get; set; }
        public string TenantName { get; set; }
    }
}