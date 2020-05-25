using JDE_API.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace JDE_API.Models
{
    public class Process : IProcessable
    {
    public int ProcessId { get; set; }
    public string Description { get; set; }
    public DateTime? StartedOn { get; set; }
    public int? StartedBy { get; set; }
    public string StartedByName { get; set; }
    public DateTime? FinishedOn { get; set; }
    public int? FinishedBy { get; set; }
    public string FinishedByName { get; set; }
    public int? ActionTypeId { get; set; }
    public string ActionTypeName { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsFrozen { get; set; }
    public bool? IsCompleted { get; set; }
    public bool? IsSuccessfull { get; set; }
    public int? PlaceId { get; set; }
    public string PlaceName { get; set; }
    public int? SetId { get; set; }
    public string SetName { get; set; }
    public int? AreaId { get; set; }
    public string AreaName { get; set; }
    public string Output { get; set; }
    public int? TenantId { get; set; }
    public string TenantName { get; set; }
    public DateTime? CreatedOn { get; set; }
    public int? CreatedBy { get; set; }
    public string CreatedByName { get; set; }
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
    public string MesId { get; set; }
    public string InitialDiagnosis { get; set; }
    public string RepairActions { get; set; }
    public string Reason { get; set; }
    public DateTime? MesDate { get; set; }
    public DateTime? PlannedStart { get; set; }
    public DateTime? PlannedFinish { get; set; }
    public ProcessStatus? LastStatus { get; set; }
    public int? LastStatusBy { get; set; }
    public string LastStatusByName { get; set; }
    public DateTime? LastStatusOn { get; set; }
    public int? OpenHandlings { get; set; }
    public int? AllHandlings { get; set; }
    public IQueryable<string> AssignedUsers { get; set; }
    public string AssignedUserNames
    {
        get
        {
            string res = "";
            if (AssignedUsers != null)
            {
                if (AssignedUsers.ToList<string>().Any())
                {
                    res = string.Join(", ", AssignedUsers);
                }
            }
            return res;
        }
    }

    public int? GivenTime { get; set; }
    }
    public enum ProcessStatus
    {
        None,
        Planned,
        Started,
        Paused,
        Resumed,
        Finished
    }
}

