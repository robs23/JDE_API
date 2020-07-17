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
    
    public partial class JDE_Processes
    {
        public int ProcessId { get; set; }
        public string Description { get; set; }
        public Nullable<System.DateTime> StartedOn { get; set; }
        public Nullable<int> StartedBy { get; set; }
        public Nullable<System.DateTime> FinishedOn { get; set; }
        public Nullable<int> FinishedBy { get; set; }
        public Nullable<int> ActionTypeId { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public Nullable<bool> IsFrozen { get; set; }
        public Nullable<bool> IsCompleted { get; set; }
        public Nullable<bool> IsSuccessfull { get; set; }
        public Nullable<int> PlaceId { get; set; }
        public string Output { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<int> TenantId { get; set; }
        public string MesId { get; set; }
        public string InitialDiagnosis { get; set; }
        public string RepairActions { get; set; }
        public string Reason { get; set; }
        public Nullable<System.DateTime> MesDate { get; set; }
        public Nullable<System.DateTime> PlannedStart { get; set; }
        public Nullable<System.DateTime> PlannedFinish { get; set; }
        public Nullable<int> LastStatus { get; set; }
        public Nullable<int> LastStatusBy { get; set; }
        public Nullable<System.DateTime> LastStatusOn { get; set; }
        public string Comment { get; set; }
    }
}
