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
    
    public partial class JDE_Places
    {
        public int PlaceId { get; set; }
        public string Number1 { get; set; }
        public string Number2 { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Nullable<int> AreaId { get; set; }
        public Nullable<int> SetId { get; set; }
        public string Priority { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<int> TenantId { get; set; }
        public string PlaceToken { get; set; }
        public Nullable<bool> IsArchived { get; set; }
        public string Image { get; set; }
    }
}
