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
    
    public partial class JDE_ProcessActions
    {
        public int ProcessActionId { get; set; }
        public Nullable<int> ProcessId { get; set; }
        public Nullable<int> ActionId { get; set; }
        public Nullable<int> HandlingId { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public Nullable<int> LmBy { get; set; }
        public Nullable<System.DateTime> LmOn { get; set; }
        public Nullable<int> TenantId { get; set; }
        public Nullable<bool> IsChecked { get; set; }
    }
}
