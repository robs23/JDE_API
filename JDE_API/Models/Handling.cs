using JDE_API.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace JDE_API.Models
{
    public class Handling : IProcessable
    {
        public int HandlingId { get; set; }
        public int ProcessId { get; set; }
        public int? PlaceId { get; set; }
        public string PlaceName { get; set; }
        public int? SetId { get; set; }
        public string SetName { get; set; }
        public int? AreaId { get; set; }
        public string AreaName { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public DateTime? StartedOn { get; set; }
        public DateTime? FinishedOn { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsFrozen { get; set; }
        public bool? IsCompleted { get; set; }
        public bool? IsSuccessfull { get; set; }
        public string Output { get; set; }
        public int? TenantId { get; set; }
        public string TenantName { get; set; }
        public int? ActionTypeId { get; set; }
        public string ActionTypeName { get; set; }
        public int? Length
        {
            get
            {
                if (StartedOn == null)
                {
                    return null;
                }
                else
                {
                    if (FinishedOn == null)
                    {
                        return (int)DateTime.Now.Subtract((DateTime)StartedOn).TotalMinutes;
                    }
                    else
                    {
                        return (int)((DateTime)FinishedOn).Subtract((DateTime)StartedOn).TotalMinutes;
                    }
                }
            }
        }
        public string AssignedUserNames { get; }
    }
}