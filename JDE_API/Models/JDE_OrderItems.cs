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
    
    public partial class JDE_OrderItems
    {
        public int OrderItemId { get; set; }
        public Nullable<int> OrderId { get; set; }
        public Nullable<int> PartId { get; set; }
        public Nullable<double> Amount { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public Nullable<int> LmBy { get; set; }
        public Nullable<System.DateTime> LmOn { get; set; }
        public Nullable<int> TenantId { get; set; }
    }
}