//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace JDE_API.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class JDE_Files
    {
        public int FileId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Link { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<int> TenantId { get; set; }
        public string Token { get; set; }
        public Nullable<int> LmBy { get; set; }
        public Nullable<System.DateTime> LmOn { get; set; }
        public Nullable<bool> IsUploaded { get; set; }
        public string Type { get; set; }
        public Nullable<long> Size { get; set; }
    }
}
