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
    
    public partial class JDE_Companies
    {
        public int CompanyId { get; set; }
        public string Name { get; set; }
        public string Street { get; set; }
        public string Street2 { get; set; }
        public string BuildingNo { get; set; }
        public string LocalNo { get; set; }
        public string ZipCode { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public Nullable<int> TypeId { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public Nullable<int> LmBy { get; set; }
        public Nullable<System.DateTime> LmOn { get; set; }
        public Nullable<int> TenantId { get; set; }
    }
}