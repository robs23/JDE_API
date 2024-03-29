﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Script.Serialization;
using JDE_API.Models;
using JDE_API.Static;
using System.Linq.Dynamic;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NLog;
using JDE_API.Interfaces;
using System.Runtime.CompilerServices;
using System.Data.SqlClient;

namespace JDE_API.Controllers
{
    public class ProcessController : ApiController
    {
        private Models.DbModel db = new Models.DbModel();
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private IQueryable<Process> FetchProcesses (int TenantId, DateTime dFrom, DateTime dTo, bool? givenTime=null, bool? finishRate=null, bool? handlingsLength=null)
        {
            IQueryable<Process> items; 
            try
            {
                items = (from p in db.JDE_Processes
                             join uuu in db.JDE_Users on p.FinishedBy equals uuu.UserId into finished
                             from fin in finished.DefaultIfEmpty()
                             join t in db.JDE_Tenants on p.TenantId equals t.TenantId
                             join u in db.JDE_Users on p.CreatedBy equals u.UserId
                             join at in db.JDE_ActionTypes on p.ActionTypeId equals at.ActionTypeId
                             join uu in db.JDE_Users on p.StartedBy equals uu.UserId into started
                             from star in started.DefaultIfEmpty()
                             join lsu in db.JDE_Users on p.LastStatusBy equals lsu.UserId into lastStatus
                             from lStat in lastStatus.DefaultIfEmpty()
                             join pl in db.JDE_Places on p.PlaceId equals pl.PlaceId
                             join comp in db.JDE_Components on p.ComponentId equals comp.ComponentId into comps
                             from components in comps.DefaultIfEmpty()
                             join s in db.JDE_Sets on pl.SetId equals s.SetId
                             join a in db.JDE_Areas on pl.AreaId equals a.AreaId
                             join h in db.JDE_Handlings on p.ProcessId equals h.ProcessId into hans
                             from ha in hans.DefaultIfEmpty()
                         where p.TenantId == TenantId && p.CreatedOn >= dFrom && p.CreatedOn <= dTo
                             group new { p, fin, t, u, at, started, lastStatus, lStat, pl, s, a, ha }
                             by new
                             {
                                 p.ProcessId,
                                 p.Description,
                                 p.StartedOn,
                                 p.StartedBy,
                                 p.FinishedOn,
                                 p.FinishedBy,
                                 p.PlannedFinish,
                                 p.PlannedStart,
                                 p.PlaceId,
                                 pl.SetId,
                                 SetName = s.Name,
                                 pl.AreaId,
                                 AreaName = a.Name,
                                 pl.Image,
                                 p.Reason,
                                 p.CreatedBy,
                                 CreatedByName = u.Name + " " + u.Surname,
                                 p.CreatedOn,
                                 p.ActionTypeId,
                                 p.Output,
                                 p.InitialDiagnosis,
                                 p.RepairActions,
                                 p.TenantId,
                                 p.MesId,
                                 p.MesDate,
                                 p.Comment,
                                 TenantName = t.TenantName,
                                 p.IsActive,
                                 p.IsCompleted,
                                 p.IsFrozen,
                                 p.IsSuccessfull,
                                 p.IsResurrected,
                                 ActionTypeName = at.Name,
                                 FinishedByName = fin.Name + " " + fin.Surname,
                                 StartedByName = star.Name + " " + star.Surname,
                                 PlaceName = pl.Name,
                                 ComponentId = p.ComponentId,
                                 ComponentName = components.Name,
                                 LastStatus = p.LastStatus == null ? (ProcessStatus?)null : (ProcessStatus)p.LastStatus, // Nullable enums handled
                                p.LastStatusBy,
                                 LastStatusByName = lStat.Name + " " + lStat.Surname,
                                 p.LastStatusOn
                             } into grp
                             orderby grp.Key.CreatedOn descending
                             select new Process
                             {
                                 ProcessId = grp.Key.ProcessId,
                                 Description = grp.Key.Description,
                                 StartedOn = grp.Key.StartedOn,
                                 StartedBy = grp.Key.StartedBy,
                                 StartedByName = grp.Key.StartedByName,
                                 FinishedOn = grp.Key.FinishedOn,
                                 FinishedBy = grp.Key.FinishedBy,
                                 FinishedByName = grp.Key.FinishedByName,
                                 ActionTypeId = grp.Key.ActionTypeId,
                                 ActionTypeName = grp.Key.ActionTypeName,
                                 IsActive = grp.Key.IsActive,
                                 IsFrozen = grp.Key.IsFrozen,
                                 IsCompleted = grp.Key.IsCompleted,
                                 IsSuccessfull = grp.Key.IsSuccessfull,
                                 PlaceId = grp.Key.PlaceId,
                                 PlaceName = grp.Key.PlaceName,
                                 PlaceImage = grp.Key.Image,
                                 SetId = grp.Key.SetId,
                                 SetName = grp.Key.SetName,
                                 AreaId = grp.Key.AreaId,
                                 AreaName = grp.Key.AreaName,
                                 Output = grp.Key.Output,
                                 TenantId = grp.Key.TenantId,
                                 TenantName = grp.Key.TenantName,
                                 CreatedOn = grp.Key.CreatedOn,
                                 CreatedBy = grp.Key.CreatedBy,
                                 CreatedByName = grp.Key.CreatedByName,
                                 MesId = grp.Key.MesId,
                                 InitialDiagnosis = grp.Key.InitialDiagnosis,
                                 RepairActions = grp.Key.RepairActions,
                                 Reason = grp.Key.Reason,
                                 MesDate = grp.Key.MesDate,
                                 Comment = grp.Key.Comment,
                                 ComponentId = grp.Key.ComponentId,
                                 ComponentName = grp.Key.ComponentName,
                                 PlannedStart = grp.Key.PlannedStart,
                                 PlannedFinish = grp.Key.PlannedFinish,
                                 LastStatus = grp.Key.LastStatus,
                                 LastStatusBy = grp.Key.LastStatusBy,
                                 LastStatusByName = grp.Key.LastStatusByName,
                                 LastStatusOn = grp.Key.LastStatusOn,
                                 IsResurrected = grp.Key.IsResurrected,
                                 OpenHandlings = grp.Where(ph => ph.ha.HandlingId > 0 && (ph.ha.IsCompleted == null || ph.ha.IsCompleted == false)).Count(),
                                 AllHandlings = grp.Where(ph => ph.ha.HandlingId > 0).Count(),
                                 AssignedUsers = (from pras in db.JDE_ProcessAssigns
                                                  join uu in db.JDE_Users on pras.UserId equals uu.UserId
                                                  where pras.ProcessId == grp.Key.ProcessId
                                                  select uu.Name + " " + uu.Surname),
                                 GivenTime = givenTime == null || givenTime == false ? 0 : (from prac in db.JDE_ProcessActions
                                                                                            join a in db.JDE_Actions on prac.ActionId equals a.ActionId
                                                                                            where prac.ProcessId == grp.Key.ProcessId
                                                                                            select a.GivenTime).Sum(),
                                 FinishRate = finishRate == null || finishRate == false ? 0 : db.JDE_ProcessActions.Count(i => i.ProcessId == grp.Key.ProcessId)==0 
                                                                                            ? 100 : (((float)db.JDE_ProcessActions.Count(i => i.ProcessId == grp.Key.ProcessId && i.IsChecked == true)
                                                                                            / (float)db.JDE_ProcessActions.Count(i => i.ProcessId == grp.Key.ProcessId))*100),
                                 HasAttachments = db.JDE_FileAssigns.Any(f => f.ProcessId == grp.Key.ProcessId),
                                 HandlingsLength = (from has in db.JDE_Handlings
                                                    where has.ProcessId == grp.Key.ProcessId
                                                    select has.FinishedOn == null ? System.Data.Entity.SqlServer.SqlFunctions.DateDiff("n", has.StartedOn, has.FinishedOn).Value : System.Data.Entity.SqlServer.SqlFunctions.DateDiff("n", has.StartedOn, has.FinishedOn).Value).Sum(),

                                 //AbandonReasons = (from pas in db.JDE_ProcessActions
                                 //                  join ars in db.JDE_AbandonReasons on pas.AbandonReasonId equals ars.AbandonReasonId into ar
                                 //                  from reasons in ar.DefaultIfEmpty()
                                 //                  where pas.ProcessId == grp.Key.ProcessId
                                 //                  select reasons.Name
                                 //                  ).Distinct()
                                 /*handlingsLength == null || handlingsLength == false ? null : grp.Where(ph => ph.ha.HandlingId > 0).Sum(h => DbFunctions.DiffMinutes(h.ha.StartedOn, h.ha.FinishedOn)) == null ? grp.Where(ph => ph.ha.HandlingId > 0).Sum(h => DbFunctions.DiffMinutes(h.ha.StartedOn, DateTime.Now)) : grp.Where(ph => ph.ha.HandlingId > 0).Sum(h => DbFunctions.DiffMinutes(h.ha.StartedOn, h.ha.FinishedOn))*/
                             }).AsNoTracking();;
            }
            catch (Exception ex)
            {

                throw;
            }
            return items;
        }

        private async Task<List<Process>> FetchProcessesAsync(int TenantId, DateTime dFrom, DateTime dTo, bool? givenTime = null, bool? finishRate = null, bool? handlingsLength = null)
        {
            Task<List<Process>> itemsQuery;
            try
            {
                //var processActionsQuery = db.JDE_ProcessActions.Where(p => p.AbandonReasonId != null).ToListAsync();

                itemsQuery = (from p in db.JDE_Processes
                         join uuu in db.JDE_Users on p.FinishedBy equals uuu.UserId into finished
                         from fin in finished.DefaultIfEmpty()
                         join t in db.JDE_Tenants on p.TenantId equals t.TenantId
                         join u in db.JDE_Users on p.CreatedBy equals u.UserId
                         join at in db.JDE_ActionTypes on p.ActionTypeId equals at.ActionTypeId
                         join uu in db.JDE_Users on p.StartedBy equals uu.UserId into started
                         from star in started.DefaultIfEmpty()
                         join lsu in db.JDE_Users on p.LastStatusBy equals lsu.UserId into lastStatus
                         from lStat in lastStatus.DefaultIfEmpty()
                         join pl in db.JDE_Places on p.PlaceId equals pl.PlaceId
                         join comp in db.JDE_Components on p.ComponentId equals comp.ComponentId into comps
                         from components in comps.DefaultIfEmpty()
                         join s in db.JDE_Sets on pl.SetId equals s.SetId
                         join a in db.JDE_Areas on pl.AreaId equals a.AreaId
                         join h in db.JDE_Handlings on p.ProcessId equals h.ProcessId into hans
                         from ha in hans.DefaultIfEmpty()
                         where p.TenantId == TenantId && p.CreatedOn >= dFrom && p.CreatedOn <= dTo
                         group new { p, fin, t, u, at, started, lastStatus, lStat, pl, s, a, ha }
                         by new
                         {
                             p.ProcessId,
                             p.Description,
                             p.StartedOn,
                             p.StartedBy,
                             p.FinishedOn,
                             p.FinishedBy,
                             p.PlannedFinish,
                             p.PlannedStart,
                             p.PlaceId,
                             pl.SetId,
                             SetName = s.Name,
                             pl.AreaId,
                             AreaName = a.Name,
                             pl.Image,
                             p.Reason,
                             p.CreatedBy,
                             CreatedByName = u.Name + " " + u.Surname,
                             p.CreatedOn,
                             p.ActionTypeId,
                             p.Output,
                             p.InitialDiagnosis,
                             p.RepairActions,
                             p.TenantId,
                             p.MesId,
                             p.MesDate,
                             p.Comment,
                             TenantName = t.TenantName,
                             p.IsActive,
                             p.IsCompleted,
                             p.IsFrozen,
                             p.IsSuccessfull,
                             p.IsResurrected,
                             ActionTypeName = at.Name,
                             FinishedByName = fin.Name + " " + fin.Surname,
                             StartedByName = star.Name + " " + star.Surname,
                             PlaceName = pl.Name,
                             ComponentId = p.ComponentId,
                             ComponentName = components.Name,
                             LastStatus = p.LastStatus == null ? (ProcessStatus?)null : (ProcessStatus)p.LastStatus, // Nullable enums handled
                                 p.LastStatusBy,
                             LastStatusByName = lStat.Name + " " + lStat.Surname,
                             p.LastStatusOn
                         } into grp
                         orderby grp.Key.CreatedOn descending
                         select new Process
                         {
                             ProcessId = grp.Key.ProcessId,
                             Description = grp.Key.Description,
                             StartedOn = grp.Key.StartedOn,
                             StartedBy = grp.Key.StartedBy,
                             StartedByName = grp.Key.StartedByName,
                             FinishedOn = grp.Key.FinishedOn,
                             FinishedBy = grp.Key.FinishedBy,
                             FinishedByName = grp.Key.FinishedByName,
                             ActionTypeId = grp.Key.ActionTypeId,
                             ActionTypeName = grp.Key.ActionTypeName,
                             IsActive = grp.Key.IsActive,
                             IsFrozen = grp.Key.IsFrozen,
                             IsCompleted = grp.Key.IsCompleted,
                             IsSuccessfull = grp.Key.IsSuccessfull,
                             PlaceId = grp.Key.PlaceId,
                             PlaceName = grp.Key.PlaceName,
                             PlaceImage = grp.Key.Image,
                             SetId = grp.Key.SetId,
                             SetName = grp.Key.SetName,
                             AreaId = grp.Key.AreaId,
                             AreaName = grp.Key.AreaName,
                             Output = grp.Key.Output,
                             TenantId = grp.Key.TenantId,
                             TenantName = grp.Key.TenantName,
                             CreatedOn = grp.Key.CreatedOn,
                             CreatedBy = grp.Key.CreatedBy,
                             CreatedByName = grp.Key.CreatedByName,
                             MesId = grp.Key.MesId,
                             InitialDiagnosis = grp.Key.InitialDiagnosis,
                             RepairActions = grp.Key.RepairActions,
                             Reason = grp.Key.Reason,
                             MesDate = grp.Key.MesDate,
                             Comment = grp.Key.Comment,
                             ComponentId = grp.Key.ComponentId,
                             ComponentName = grp.Key.ComponentName,
                             PlannedStart = grp.Key.PlannedStart,
                             PlannedFinish = grp.Key.PlannedFinish,
                             LastStatus = grp.Key.LastStatus,
                             LastStatusBy = grp.Key.LastStatusBy,
                             LastStatusByName = grp.Key.LastStatusByName,
                             LastStatusOn = grp.Key.LastStatusOn,
                             IsResurrected = grp.Key.IsResurrected,
                             OpenHandlings = grp.Where(ph => ph.ha.HandlingId > 0 && (ph.ha.IsCompleted == null || ph.ha.IsCompleted == false)).Count(),
                             AllHandlings = grp.Where(ph => ph.ha.HandlingId > 0).Count(),
                             //AssignedUsers = (from pras in db.JDE_ProcessAssigns
                             //                 join uu in db.JDE_Users on pras.UserId equals uu.UserId
                             //                 where pras.ProcessId == grp.Key.ProcessId
                             //                 select uu.Name + " " + uu.Surname),
                             //GivenTime = givenTime == null || givenTime == false ? 0 : (from prac in db.JDE_ProcessActions
                             //                                                           join a in db.JDE_Actions on prac.ActionId equals a.ActionId
                             //                                                           where prac.ProcessId == grp.Key.ProcessId
                             //                                                           select a.GivenTime).Sum(),
                             //FinishRate = finishRate == null || finishRate == false ? 0 : db.JDE_ProcessActions.Count(i => i.ProcessId == grp.Key.ProcessId) == 0
                             //                                                           ? 100 : (((float)db.JDE_ProcessActions.Count(i => i.ProcessId == grp.Key.ProcessId && i.IsChecked == true)
                             //                                                           / (float)db.JDE_ProcessActions.Count(i => i.ProcessId == grp.Key.ProcessId)) * 100),
                             HasAttachments = db.JDE_FileAssigns.Any(f => f.ProcessId == grp.Key.ProcessId),
                             HandlingsLength = (from has in db.JDE_Handlings
                                                where has.ProcessId == grp.Key.ProcessId
                                                select has.FinishedOn == null ? System.Data.Entity.SqlServer.SqlFunctions.DateDiff("n", has.StartedOn, has.FinishedOn).Value : System.Data.Entity.SqlServer.SqlFunctions.DateDiff("n", has.StartedOn, has.FinishedOn).Value).Sum(),

                             //AbandonReasons = (from pas in db.JDE_ProcessActions
                             //                  join ars in db.JDE_AbandonReasons on pas.AbandonReasonId equals ars.AbandonReasonId into ar
                             //                  from reasons in ar.DefaultIfEmpty()
                             //                  where pas.ProcessId == grp.Key.ProcessId
                             //                  select reasons.Name
                             //                  ).Distinct()
                             /*handlingsLength == null || handlingsLength == false ? null : grp.Where(ph => ph.ha.HandlingId > 0).Sum(h => DbFunctions.DiffMinutes(h.ha.StartedOn, h.ha.FinishedOn)) == null ? grp.Where(ph => ph.ha.HandlingId > 0).Sum(h => DbFunctions.DiffMinutes(h.ha.StartedOn, DateTime.Now)) : grp.Where(ph => ph.ha.HandlingId > 0).Sum(h => DbFunctions.DiffMinutes(h.ha.StartedOn, h.ha.FinishedOn))*/
                         }).ToListAsync(); ;
            }
            catch (Exception ex)
            {

                throw;
            }
            return await itemsQuery;
        }

        private async Task<List<Process>> FetchProcessesWithoutGroupings(int TenantId, DateTime dFrom, DateTime dTo, bool? givenTime = null, bool? finishRate = null, bool? handlingsLength = null)
        {
            Task<List<Process>> itemsQuery;
            try
            {

                itemsQuery = (from p in db.JDE_Processes
                              join uuu in db.JDE_Users on p.FinishedBy equals uuu.UserId into finished
                              from fin in finished.DefaultIfEmpty()
                              join t in db.JDE_Tenants on p.TenantId equals t.TenantId
                              join u in db.JDE_Users on p.CreatedBy equals u.UserId
                              join at in db.JDE_ActionTypes on p.ActionTypeId equals at.ActionTypeId
                              join uu in db.JDE_Users on p.StartedBy equals uu.UserId into started
                              from star in started.DefaultIfEmpty()
                              join lsu in db.JDE_Users on p.LastStatusBy equals lsu.UserId into lastStatus
                              from lStat in lastStatus.DefaultIfEmpty()
                              join pl in db.JDE_Places on p.PlaceId equals pl.PlaceId into places
                              from pla in places.DefaultIfEmpty()
                              join comp in db.JDE_Components on p.ComponentId equals comp.ComponentId into comps
                              from components in comps.DefaultIfEmpty()
                              join s in db.JDE_Sets on pla.SetId equals s.SetId into sets
                              from set in sets.DefaultIfEmpty()
                              join a in db.JDE_Areas on pla.AreaId equals a.AreaId into areas
                              from area in areas.DefaultIfEmpty()
                              where p.TenantId == TenantId && p.CreatedOn >= dFrom && p.CreatedOn <= dTo
                              orderby p.CreatedOn descending
                              select new Process
                              {
                                  ProcessId = p.ProcessId,
                                  Description = p.Description,
                                  StartedOn = p.StartedOn,
                                  StartedBy = p.StartedBy,
                                  StartedByName = star.Name + " " + star.Surname,
                                  FinishedOn = p.FinishedOn,
                                  FinishedBy = p.FinishedBy,
                                  FinishedByName = fin.Name + " " + fin.Surname,
                                  ActionTypeId = p.ActionTypeId,
                                  ActionTypeName = at.Name,
                                  IsActive = p.IsActive,
                                  IsFrozen = p.IsFrozen,
                                  IsCompleted = p.IsCompleted,
                                  IsSuccessfull = p.IsSuccessfull,
                                  PlaceId = p.PlaceId,
                                  PlaceName = pla.Name,
                                  PlaceImage = pla.Image,
                                  SetId = pla.SetId,
                                  SetName = set.Name,
                                  AreaId = pla.AreaId,
                                  AreaName = area.Name,
                                  Output = p.Output,
                                  TenantId = p.TenantId,
                                  TenantName = t.TenantName,
                                  CreatedOn = p.CreatedOn,
                                  CreatedBy = p.CreatedBy,
                                  CreatedByName = u.Name + " " + u.Surname,
                                  MesId = p.MesId,
                                  InitialDiagnosis = p.InitialDiagnosis,
                                  RepairActions = p.RepairActions,
                                  Reason = p.Reason,
                                  MesDate = p.MesDate,
                                  Comment = p.Comment,
                                  ComponentId = p.ComponentId,
                                  ComponentName = components.Name,
                                  PlannedStart = p.PlannedStart,
                                  PlannedFinish = p.PlannedFinish,
                                  LastStatus = (ProcessStatus)p.LastStatus,
                                  LastStatusBy = p.LastStatusBy,
                                  LastStatusByName = lStat.Name + " " + lStat.Surname,
                                  LastStatusOn = p.LastStatusOn,
                                  IsResurrected = p.IsResurrected,
                                  HasAttachments = db.JDE_FileAssigns.Any(f => f.ProcessId == p.ProcessId),
                              }).ToListAsync(); ;
            }
            catch (Exception ex)
            {

                throw;
            }
            return await itemsQuery;
        }

        private async Task<List<Process>> FetchHandlings()
        {
            using (Models.DbModel _db = new Models.DbModel())
            {
                var items = _db.JDE_Handlings.Where(x => x.ProcessId > 0 && x.ProcessId !=null).Select(h => new
                {
                    ProcessId = h.ProcessId,
                    HandlingId = h.HandlingId,
                    IsCompleted = h.IsCompleted,
                    StartedOn = h.StartedOn,
                    FinishedOn = h.FinishedOn
                }).GroupBy(pa => new { pa.ProcessId }).Select(pr => new Process
                {
                    ProcessId = (int)pr.Key.ProcessId,
                    OpenHandlings = pr.Count(item=>item.HandlingId > 0 && item.IsCompleted != true),
                    AllHandlings = pr.Count(item=>item.HandlingId > 0),
                    HandlingsLength = (from has in pr
                                       where has.ProcessId == pr.Key.ProcessId
                                       select has.FinishedOn == null ? System.Data.Entity.SqlServer.SqlFunctions.DateDiff("n", has.StartedOn, has.FinishedOn).Value : System.Data.Entity.SqlServer.SqlFunctions.DateDiff("n", has.StartedOn, has.FinishedOn).Value).Sum(),
                }).ToListAsync();

                return await items;
            }
        }

        private async Task<List<Process>> GetGivenTimes()
        {
            using (Models.DbModel _db = new Models.DbModel())
            {
                var items = (from prac in _db.JDE_ProcessActions
                             join a in _db.JDE_Actions on prac.ActionId equals a.ActionId
                             where a.GivenTime != null 
                             select new Process
                             {
                                 ProcessId = (int)prac.ProcessId,
                                 GivenTime = a.GivenTime
                             }).ToListAsync();


                return await items;
            }
        }

        private async Task<List<Process>> GetFinishRates()
        {
            using (Models.DbModel _db = new Models.DbModel())
            {
                var items = _db.JDE_ProcessActions.Select(p => new
                {
                    ProcessId = p.ProcessId,
                    IsChecked = p.IsChecked
                }).GroupBy(pa => new { pa.ProcessId }).Select(pr => new Process
                {
                    ProcessId = (int)pr.Key.ProcessId,
                    FinishRate = pr.Count() == 0 ? 100 : ((float)pr.Count(item => item.IsChecked == true) / (float)pr.Count()) * 100
                }).ToListAsync();

                return await items;
            }
        }

        private async Task<List<ProcessAssign>> GetProcessAssigns()
        {
            using (Models.DbModel _db = new Models.DbModel())
            {
                var items = (from pras in _db.JDE_ProcessAssigns
                              join uu in _db.JDE_Users on pras.UserId equals uu.UserId
                              select new ProcessAssign
                              {
                                  ProcessAssignId = pras.ProcessAssignId,
                                  ProcessId = pras.ProcessId,
                                  UserId = pras.UserId,
                                  UserName = uu.Name + " " + uu.Surname
                              }).ToListAsync();

                return await items;
            }
        }

        private async Task<List<ProcessActionAbandonReason>> GetAbandonReasons()
        {
            using (Models.DbModel _db = new Models.DbModel())
            {
                var items = (from pas in _db.JDE_ProcessActions
                             join ars in _db.JDE_AbandonReasons on pas.AbandonReasonId equals ars.AbandonReasonId into ar
                             from reasons in ar.DefaultIfEmpty()
                             where pas.AbandonReasonId != null
                             select new ProcessActionAbandonReason
                             {
                                 ProcessId = (int)pas.ProcessId,
                                 AbandonReasonId = pas.AbandonReasonId,
                                 AbandonReasonName = reasons.Name
                             }
                                 ).Distinct().ToListAsync();
                return await items;
            }
        }

        private IHttpActionResult PrepareResponse(IEnumerable<IProcessable> items, int page, int total, int? PageSize=null)
        {

            if (total == 0 && page > 0)
            {
                int pageSize = RuntimeSettings.PageSize;
                if (PageSize != null)
                {
                    pageSize = (int)PageSize;
                }
                var skip = pageSize * (page - 1);
                if (skip < items.Count())
                {
                    items = items.Skip(skip).Take(pageSize);
                    return Ok(items);
                }
                else
                {
                    return NotFound();
                }
            }
            else if (total > 0 && page == 0)
            {
                items = items.Take(total);
                return Ok(items);
            }
            else
            {
                return Ok(items);
            }
        }

        private IQueryable<IProcessable> AdvancedFilter(IQueryable<Process> items, string length = null, string status = null, string assignedUserNames = null, string timingStatus=null, string timingVsPlan = null, string processLength = null, string handlingsLength = null)
        {
            if (!string.IsNullOrEmpty(length) || !string.IsNullOrEmpty(status) || !string.IsNullOrEmpty(assignedUserNames) || !string.IsNullOrEmpty(timingStatus) || !string.IsNullOrEmpty(timingVsPlan) || !string.IsNullOrEmpty(processLength) || !string.IsNullOrEmpty(handlingsLength))
            {
                List<IProcessable> nItems = items.ToList<IProcessable>();
                if (!string.IsNullOrEmpty(length)) { nItems = Static.Utilities.FilterByLength(nItems, length); }
                if (!string.IsNullOrEmpty(status)) { nItems = Static.Utilities.FilterByStatus(nItems, status); }
                if (!string.IsNullOrEmpty(assignedUserNames)) { nItems = Static.Utilities.FilterByAssignedUserNames(nItems, assignedUserNames); }
                if (!string.IsNullOrEmpty(timingStatus)) { nItems = Static.Utilities.FilterByTimingStatus(nItems, timingStatus); }
                if (!string.IsNullOrEmpty(timingVsPlan)) { nItems = Static.Utilities.FilterByTimingVsPlan(nItems, timingVsPlan); }
                if (!string.IsNullOrEmpty(processLength)) { nItems = Static.Utilities.FilterByProcessLength(nItems, processLength); }
                if (!string.IsNullOrEmpty(handlingsLength)) { nItems = Static.Utilities.FilterByHandlingsLength(nItems, handlingsLength); }
                return nItems.AsQueryable();
            }
            else
            {
                return items;
            }
            
        }

        [HttpGet]
        [Route("GetProcessesOld")]
        public async Task<IHttpActionResult> GetProcessesOld(string token, int page=0, int total=0, DateTime? dFrom = null, DateTime? dTo = null, string query = null, string length = null, string status = null, bool? GivenTime = null, bool? FinishRate=null, int? pageSize=null, bool? handlingsLength=null)
        {
            //if ext=true then there's more columns in the result sent
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    dFrom = dFrom ?? db.JDE_Processes.Min(x => x.CreatedOn).Value.AddDays(-1);
                    dTo = dTo ?? db.JDE_Processes.Max(x => x.CreatedOn).Value.AddDays(1);
                    string assignedUserNames = null;
                    string timingStatus = null;
                    string timingVsPlan = null;
                    string processLength = null;
                    string _handlingsLength = null;

                    var items = FetchProcesses(tenants.FirstOrDefault().TenantId, (DateTime)dFrom, (DateTime)dTo, GivenTime, FinishRate, handlingsLength);

                    if (items.Any())
                    {
                        IQueryable<Process> nItems = items.AsQueryable();

                        if (query != null)
                        {
                            if (query.IndexOf("Length") >= 0 || query.IndexOf("Status") >= 0 || query.IndexOf("AssignedUserNames") >= 0 || query.IndexOf("TimingStatus") >= 0 || query.IndexOf("TimingVsPlan") >= 0 || query.IndexOf("HandlingsLength") >= 0 || query.IndexOf("ProcessLength") >= 0)
                            {
                                ProcessQuery pq = new ProcessQuery(query);
                                length = pq.Length;
                                status = pq.Status;
                                assignedUserNames = pq.AssignedUserNames;
                                timingStatus = pq.TimingStatus;
                                timingVsPlan = pq.TimingVsPlan;
                                _handlingsLength = pq.HandlingsLength;
                                processLength = pq.ProcessLength;
                                query = pq.Query;

                            }
                            if (!string.IsNullOrEmpty(query))
                            {
                                nItems = nItems.Where(query);

                            }
                        }
                        
                        IQueryable<IProcessable> nnItems = AdvancedFilter(nItems, length, status, assignedUserNames, timingStatus, timingVsPlan, processLength, _handlingsLength);

                        return PrepareResponse(nItems, page, total, pageSize);
                    }
                    else
                    {
                        return NotFound();
                    }

                }
                else
                {
                    return NotFound();
                }

            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet]
        [Route("GetProcessesAsync")]
        public async Task<IHttpActionResult> GetProcessesAsync(string token, int page = 0, int total = 0, DateTime? dFrom = null, DateTime? dTo = null, string query = null, string length = null, string status = null, bool? GivenTime = null, bool? FinishRate = null, int? pageSize = null, bool? handlingsLength = null, bool? AssignedUsers = null, bool? AbandonReasons = null)
        {
            //if ext=true then there's more columns in the result sent
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    dFrom = dFrom ?? db.JDE_Processes.Min(x => x.CreatedOn).Value.AddDays(-1);
                    dTo = dTo ?? db.JDE_Processes.Max(x => x.CreatedOn).Value.AddDays(1);
                    string assignedUserNames = null;
                    string timingStatus = null;
                    string timingVsPlan = null;
                    string processLength = null;
                    string _handlingsLength = null;

                    var tasks = new List<Task>();
                    var processesTask = FetchProcessesAsync(tenants.FirstOrDefault().TenantId, (DateTime)dFrom, (DateTime)dTo, GivenTime, FinishRate, handlingsLength);
                    tasks.Add(processesTask);

                    //Set up optional AbandonReasons fetching
                    Task<List<ProcessActionAbandonReason>> abandonsTask = null;
                    List<ProcessActionAbandonReason> abandons = new List<ProcessActionAbandonReason>();
                    if (AbandonReasons == true)
                    {
                        abandonsTask = GetAbandonReasons();
                        tasks.Add(abandonsTask);
                    }

                    //Set up optional AssignedUsers fetching
                    Task<List<ProcessAssign>> assignedUsersTask = null;
                    List<ProcessAssign> assigns = new List<ProcessAssign>();
                    if (AssignedUsers == true)
                    {
                        assignedUsersTask = GetProcessAssigns();
                        tasks.Add(assignedUsersTask);
                    }

                    //Set up optional GivenTimes fetching
                    Task<List<Process>> givenTimesTask =  null;
                    List<Process> givenTimes = new List<Process>();
                    if (GivenTime==true)
                    {
                        givenTimesTask = GetGivenTimes();
                        tasks.Add(givenTimesTask);
                    }

                    //Set up optional FinishRate fetching
                    Task<List<Process>> finishRateTask = null;
                    List<Process> finishRates = new List<Process>();
                    if (FinishRate == true)
                    {
                        finishRateTask = GetFinishRates();
                        tasks.Add(finishRateTask);
                    }

                    await Task.WhenAll(tasks);
                    var items = await processesTask;
                    if (abandonsTask != null)
                    {
                        abandons = await abandonsTask; 
                    }
                    if (assignedUsersTask != null)
                    {
                        assigns = await assignedUsersTask;  
                    }                  
                    if (givenTimesTask != null)
                    {
                        givenTimes = await givenTimesTask;
                    }
                    if (finishRateTask != null)
                    {
                        finishRates = await finishRateTask;
                    }

                    var processes = from process in items
                                    join abandon in abandons on process.ProcessId equals abandon.ProcessId into ProcessAbandons
                                    join assign in assigns on process.ProcessId equals assign.ProcessId into ProcessAssigns
                                    join givenTime in givenTimes on process.ProcessId equals givenTime.ProcessId into GivenTimes
                                    join finishRate in finishRates on process.ProcessId equals finishRate.ProcessId into FinishRates
                                    from fRates in FinishRates.DefaultIfEmpty()
                                    select new Process
                                    {
                                        ProcessId = process.ProcessId,
                                        Description = process.Description,
                                        StartedOn = process.StartedOn,
                                        StartedBy = process.StartedBy,
                                        StartedByName = process.StartedByName,
                                        FinishedOn = process.FinishedOn,
                                        FinishedBy = process.FinishedBy,
                                        FinishedByName = process.FinishedByName,
                                        ActionTypeId = process.ActionTypeId,
                                        ActionTypeName = process.ActionTypeName,
                                        IsActive = process.IsActive,
                                        IsFrozen = process.IsFrozen,
                                        IsCompleted = process.IsCompleted,
                                        IsSuccessfull = process.IsSuccessfull,
                                        PlaceId = process.PlaceId,
                                        PlaceName = process.PlaceName,
                                        PlaceImage = process.PlaceImage,
                                        SetId = process.SetId,
                                        SetName = process.SetName,
                                        AreaId = process.AreaId,
                                        AreaName = process.AreaName,
                                        Output = process.Output,
                                        TenantId = process.TenantId,
                                        TenantName = process.TenantName,
                                        CreatedOn = process.CreatedOn,
                                        CreatedBy = process.CreatedBy,
                                        CreatedByName = process.CreatedByName,
                                        MesId = process.MesId,
                                        InitialDiagnosis = process.InitialDiagnosis,
                                        RepairActions = process.RepairActions,
                                        Reason = process.Reason,
                                        MesDate = process.MesDate,
                                        Comment = process.Comment,
                                        ComponentId = process.ComponentId,
                                        ComponentName = process.ComponentName,
                                        PlannedStart = process.PlannedStart,
                                        PlannedFinish = process.PlannedFinish,
                                        LastStatus = process.LastStatus,
                                        LastStatusBy = process.LastStatusBy,
                                        LastStatusByName = process.LastStatusByName,
                                        LastStatusOn = process.LastStatusOn,
                                        IsResurrected = process.IsResurrected,
                                        OpenHandlings = process.OpenHandlings,
                                        AllHandlings = process.AllHandlings,
                                        AssignedUsers = ProcessAssigns.Select(x=>x.UserName).AsQueryable(),
                                        GivenTime = GivenTimes.Sum(x=>x.GivenTime),
                                        FinishRate = fRates == null ? null : fRates.FinishRate, //fRates.Where(f=>f.ProcessId == process.ProcessId).Any() ? finishRates.FirstOrDefault(f => f.ProcessId == process.ProcessId).FinishRate : null ,
                                        HasAttachments = process.HasAttachments
                                    };

                    if (items.Any())
                    {
                        IQueryable<Process> nItems = processes.AsQueryable();

                        if (query != null)
                        {
                            if (query.IndexOf("Length") >= 0 || query.IndexOf("Status") >= 0 || query.IndexOf("AssignedUserNames") >= 0 || query.IndexOf("TimingStatus") >= 0 || query.IndexOf("TimingVsPlan") >= 0 || query.IndexOf("HandlingsLength") >= 0 || query.IndexOf("ProcessLength") >= 0)
                            {
                                ProcessQuery pq = new ProcessQuery(query);
                                length = pq.Length;
                                status = pq.Status;
                                assignedUserNames = pq.AssignedUserNames;
                                timingStatus = pq.TimingStatus;
                                timingVsPlan = pq.TimingVsPlan;
                                _handlingsLength = pq.HandlingsLength;
                                processLength = pq.ProcessLength;
                                query = pq.Query;

                            }
                            if (!string.IsNullOrEmpty(query))
                            {
                                nItems = nItems.Where(query);

                            }
                        }

                        IQueryable<IProcessable> nnItems = AdvancedFilter(nItems, length, status, assignedUserNames, timingStatus, timingVsPlan, processLength, _handlingsLength);

                        return PrepareResponse(nItems, page, total, pageSize);
                    }
                    else
                    {
                        return NotFound();
                    }

                }
                else
                {
                    return NotFound();
                }

            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet]
        [Route("GetProcesses")]
        public async Task<IHttpActionResult> GetProcesses(string token, int page = 0, int total = 0, DateTime? dFrom = null, DateTime? dTo = null, string query = null, string length = null, string status = null, bool? GivenTime = null, bool? FinishRate = null, int? pageSize = null, bool? handlingsLength = null, bool? AssignedUsers = true, bool? AbandonReasons = true)
        {
            //if ext=true then there's more columns in the result sent
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    dFrom = dFrom ?? db.JDE_Processes.Min(x => x.CreatedOn).Value.AddDays(-1);
                    dTo = dTo ?? db.JDE_Processes.Max(x => x.CreatedOn).Value.AddDays(1);
                    string assignedUserNames = null;
                    string timingStatus = null;
                    string timingVsPlan = null;
                    string processLength = null;
                    string _handlingsLength = null;

                    var tasks = new List<Task>();
                    var processesTask = FetchProcessesWithoutGroupings(tenants.FirstOrDefault().TenantId, (DateTime)dFrom, (DateTime)dTo, GivenTime, FinishRate, handlingsLength);
                    tasks.Add(processesTask);

                    var handlingsTask = FetchHandlings();
                    tasks.Add(handlingsTask);

                    //Set up optional AbandonReasons fetching
                    Task<List<ProcessActionAbandonReason>> abandonsTask = null;
                    List<ProcessActionAbandonReason> abandons = new List<ProcessActionAbandonReason>();
                    if (AbandonReasons == true)
                    {
                        abandonsTask = GetAbandonReasons();
                        tasks.Add(abandonsTask);
                    }

                    //Set up optional AssignedUsers fetching
                    Task<List<ProcessAssign>> assignedUsersTask = null;
                    List<ProcessAssign> assigns = new List<ProcessAssign>();
                    if (AssignedUsers == true)
                    {
                        assignedUsersTask = GetProcessAssigns();
                        tasks.Add(assignedUsersTask);
                    }

                    //Set up optional GivenTimes fetching
                    Task<List<Process>> givenTimesTask = null;
                    List<Process> givenTimes = new List<Process>();
                    if (GivenTime == true)
                    {
                        givenTimesTask = GetGivenTimes();
                        tasks.Add(givenTimesTask);
                    }

                    //Set up optional FinishRate fetching
                    Task<List<Process>> finishRateTask = null;
                    List<Process> finishRates = new List<Process>();
                    if (FinishRate == true)
                    {
                        finishRateTask = GetFinishRates();
                        tasks.Add(finishRateTask);
                    }

                    await Task.WhenAll(tasks);
                    var items = await processesTask;
                    var handlings = await handlingsTask;

                    if (abandonsTask != null)
                    {
                        abandons = await abandonsTask;
                    }
                    if (assignedUsersTask != null)
                    {
                        assigns = await assignedUsersTask;
                    }
                    if (givenTimesTask != null)
                    {
                        givenTimes = await givenTimesTask;
                    }
                    if (finishRateTask != null)
                    {
                        finishRates = await finishRateTask;
                    }

                    try
                    {
                        var processes = from process in items
                                        join handling in handlings on process.ProcessId equals handling.ProcessId into Handlings
                                        from handling in Handlings.DefaultIfEmpty()
                                        join abandon in abandons on process.ProcessId equals abandon.ProcessId into ProcessAbandons
                                        join assign in assigns on process.ProcessId equals assign.ProcessId into ProcessAssigns
                                        join givenTime in givenTimes on process.ProcessId equals givenTime.ProcessId into GivenTimes
                                        join finishRate in finishRates on process.ProcessId equals finishRate.ProcessId into FinishRates
                                        from fRates in FinishRates.DefaultIfEmpty()
                                        select new Process
                                        {
                                            ProcessId = process.ProcessId,
                                            Description = process.Description,
                                            StartedOn = process.StartedOn,
                                            StartedBy = process.StartedBy,
                                            StartedByName = process.StartedByName,
                                            FinishedOn = process.FinishedOn,
                                            FinishedBy = process.FinishedBy,
                                            FinishedByName = process.FinishedByName,
                                            ActionTypeId = process.ActionTypeId,
                                            ActionTypeName = process.ActionTypeName,
                                            IsActive = process.IsActive,
                                            IsFrozen = process.IsFrozen,
                                            IsCompleted = process.IsCompleted,
                                            IsSuccessfull = process.IsSuccessfull,
                                            PlaceId = process.PlaceId,
                                            PlaceName = process.PlaceName ?? "",
                                            PlaceImage = process.PlaceImage ?? "",
                                            SetId = process.SetId,
                                            SetName = process.SetName ?? "",
                                            AreaId = process.AreaId,
                                            AreaName = process.AreaName ?? "",
                                            Output = process.Output ?? "",
                                            TenantId = process.TenantId,
                                            TenantName = process.TenantName,
                                            CreatedOn = process.CreatedOn,
                                            CreatedBy = process.CreatedBy,
                                            CreatedByName = process.CreatedByName,
                                            MesId = process.MesId,
                                            InitialDiagnosis = process.InitialDiagnosis ?? "",
                                            RepairActions = process.RepairActions ?? "",
                                            Reason = process.Reason ?? "",
                                            MesDate = process.MesDate,
                                            Comment = process.Comment ?? "",
                                            ComponentId = process.ComponentId,
                                            ComponentName = process.ComponentName,
                                            PlannedStart = process.PlannedStart,
                                            PlannedFinish = process.PlannedFinish,
                                            LastStatus = process.LastStatus,
                                            LastStatusBy = process.LastStatusBy,
                                            LastStatusByName = process.LastStatusByName,
                                            LastStatusOn = process.LastStatusOn,
                                            IsResurrected = process.IsResurrected,
                                            OpenHandlings = handling == null ? 0 : handling.OpenHandlings,
                                            AllHandlings = handling == null ? 0 : handling.AllHandlings,
                                            AssignedUsers = ProcessAssigns.Select(x => x.UserName).AsQueryable(),
                                            GivenTime = GivenTimes.Sum(x => x.GivenTime) == 0 ? null : GivenTimes.Sum(x => x.GivenTime),
                                            FinishRate = fRates == null ? 100 : fRates.FinishRate, //fRates.Where(f=>f.ProcessId == process.ProcessId).Any() ? finishRates.FirstOrDefault(f => f.ProcessId == process.ProcessId).FinishRate : null ,
                                            HasAttachments = process.HasAttachments,
                                            HandlingsLength = handling == null ? null : handling.HandlingsLength,
                                            AbandonReasons = ProcessAbandons.Select(x => x.AbandonReasonName).AsQueryable(),
                                            AbandonReasonNames = string.Join(",", ProcessAbandons.Select(x => x.AbandonReasonName))
                                        };
                        if (items.Any())
                        {
                            IQueryable<Process> nItems = processes.AsQueryable();

                            if (query != null)
                            {
                                if (query.IndexOf("Length") >= 0 || query.IndexOf("Status") >= 0 || query.IndexOf("AssignedUserNames") >= 0 || query.IndexOf("TimingStatus") >= 0 || query.IndexOf("TimingVsPlan") >= 0 || query.IndexOf("HandlingsLength") >= 0 || query.IndexOf("ProcessLength") >= 0)
                                {
                                    ProcessQuery pq = new ProcessQuery(query);
                                    length = pq.Length;
                                    status = pq.Status;
                                    assignedUserNames = pq.AssignedUserNames;
                                    timingStatus = pq.TimingStatus;
                                    timingVsPlan = pq.TimingVsPlan;
                                    _handlingsLength = pq.HandlingsLength;
                                    processLength = pq.ProcessLength;
                                    query = pq.Query;

                                }
                                if (!string.IsNullOrEmpty(query))
                                {
                                    nItems = nItems.Where(query);

                                }
                            }

                            IQueryable<IProcessable> nnItems = AdvancedFilter(nItems, length, status, assignedUserNames, timingStatus, timingVsPlan, processLength, _handlingsLength);

                            return PrepareResponse(nnItems, page, total, pageSize);
                        }
                        else
                        {
                            return NotFound();
                        }
                    }
                    catch (Exception ex)
                    {
                        return InternalServerError(ex);
                    }
                }
                else
                {
                    return NotFound();
                }

            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet]
        [Route("GetUsersOpenProcesses")]
        public IHttpActionResult GetUsersOpenProcesses(string token, int UserId, int page = 0, int total = 0, DateTime? dFrom = null, DateTime? dTo = null, string query = null, string length = null)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    dFrom = dFrom ?? db.JDE_Processes.Min(x => x.CreatedOn).Value.AddDays(-1);
                    dTo = dTo ?? db.JDE_Processes.Max(x => x.CreatedOn).Value.AddDays(1);
                    db.Database.Log = Console.Write;
                    var items = (from p in db.JDE_Processes
                                 join uuu in db.JDE_Users on p.FinishedBy equals uuu.UserId into finished
                                 from fin in finished.DefaultIfEmpty()
                                 join t in db.JDE_Tenants on p.TenantId equals t.TenantId
                                 join u in db.JDE_Users on p.CreatedBy equals u.UserId
                                 join at in db.JDE_ActionTypes on p.ActionTypeId equals at.ActionTypeId
                                 join uu in db.JDE_Users on p.StartedBy equals uu.UserId into started
                                 from star in started.DefaultIfEmpty()
                                 join lsu in db.JDE_Users on p.LastStatusBy equals lsu.UserId into lastStatus
                                 from lStat in lastStatus.DefaultIfEmpty()
                                 join pla in db.JDE_Places on p.PlaceId equals pla.PlaceId into places
                                 from place in places.DefaultIfEmpty()
                                 join s in db.JDE_Sets on place.SetId equals s.SetId into sets
                                 from set in sets.DefaultIfEmpty()
                                 join a in db.JDE_Areas on place.AreaId equals a.AreaId into areas
                                 from area in areas.DefaultIfEmpty()
                                 join h in db.JDE_Handlings on p.ProcessId equals h.ProcessId into hans
                                 from ha in hans.DefaultIfEmpty()
                                 join pa in db.JDE_ProcessAssigns on p.ProcessId equals pa.ProcessId into pas
                                 from ppas in pas.DefaultIfEmpty()
                                 where p.TenantId == tenants.FirstOrDefault().TenantId && p.CreatedOn >= dFrom && p.CreatedOn <= dTo 
                                 && ((ha.UserId == UserId && (ha.IsCompleted == false || ha.IsCompleted == null)) 
                                 || (p.LastStatusBy==UserId && p.IsCompleted==false && (p.IsActive==true || p.IsFrozen==true))
                                 || (p.LastStatus==(int)ProcessStatus.Planned && p.PlannedStart <= DateTime.Now && ppas.UserId==UserId))
                                 group new { p, fin, t, u, at, started, lastStatus, lStat, place, set, area, ha }
                                 by new
                                 {
                                     p.ProcessId,
                                     p.Description,
                                     p.StartedOn,
                                     p.StartedBy,
                                     p.FinishedOn,
                                     p.FinishedBy,
                                     p.PlannedFinish,
                                     p.PlannedStart,
                                     p.PlaceId,
                                     place.SetId,
                                     SetName = set.Name,
                                     place.AreaId,
                                     AreaName = area.Name,
                                     place.Image,
                                     p.Reason,
                                     p.CreatedBy,
                                     CreatedByName = u.Name + " " + u.Surname,
                                     p.CreatedOn,
                                     p.ActionTypeId,
                                     p.Output,
                                     p.InitialDiagnosis,
                                     p.RepairActions,
                                     p.TenantId,
                                     p.MesId,
                                     p.Comment,
                                     p.MesDate,
                                     TenantName = t.TenantName,
                                     p.IsActive,
                                     p.IsCompleted,
                                     p.IsFrozen,
                                     p.IsSuccessfull,
                                     p.IsResurrected,
                                     ActionTypeName = at.Name,
                                     FinishedByName = fin.Name + " " + fin.Surname,
                                     StartedByName = star.Name + " " + star.Surname,
                                     PlaceName = place.Name,
                                     LastStatus = p.LastStatus == null ? (ProcessStatus?)null : (ProcessStatus)p.LastStatus, // Nullable enums handled
                                     p.LastStatusBy,
                                     LastStatusByName = lStat.Name + " " + lStat.Surname,
                                     p.LastStatusOn
                                 } into grp
                                 orderby grp.Key.CreatedOn descending
                                 select new Process
                                 {
                                     ProcessId = grp.Key.ProcessId,
                                     Description = grp.Key.Description,
                                     StartedOn = grp.Key.StartedOn,
                                     StartedBy = grp.Key.StartedBy,
                                     StartedByName = grp.Key.StartedByName,
                                     FinishedOn = grp.Key.FinishedOn,
                                     FinishedBy = grp.Key.FinishedBy,
                                     FinishedByName = grp.Key.FinishedByName,
                                     ActionTypeId = grp.Key.ActionTypeId,
                                     ActionTypeName = grp.Key.ActionTypeName,
                                     IsActive = grp.Key.IsActive,
                                     IsFrozen = grp.Key.IsFrozen,
                                     IsCompleted = grp.Key.IsCompleted,
                                     IsSuccessfull = grp.Key.IsSuccessfull,
                                     PlaceId = grp.Key.PlaceId,
                                     PlaceName = grp.Key.PlaceName,
                                     PlaceImage = grp.Key.Image,
                                     SetId = grp.Key.SetId,
                                     SetName = grp.Key.SetName,
                                     AreaId = grp.Key.AreaId,
                                     AreaName = grp.Key.AreaName,
                                     Output = grp.Key.Output,
                                     TenantId = grp.Key.TenantId,
                                     TenantName = grp.Key.TenantName,
                                     CreatedOn = grp.Key.CreatedOn,
                                     CreatedBy = grp.Key.CreatedBy,
                                     CreatedByName = grp.Key.CreatedByName,
                                     MesId = grp.Key.MesId,
                                     Comment = grp.Key.Comment,
                                     InitialDiagnosis = grp.Key.InitialDiagnosis,
                                     RepairActions = grp.Key.RepairActions,
                                     Reason = grp.Key.Reason,
                                     MesDate = grp.Key.MesDate,
                                     PlannedStart = grp.Key.PlannedStart,
                                     PlannedFinish = grp.Key.PlannedFinish,
                                     LastStatus = grp.Key.LastStatus,
                                     LastStatusBy = grp.Key.LastStatusBy,
                                     LastStatusByName = grp.Key.LastStatusByName,
                                     LastStatusOn = grp.Key.LastStatusOn,
                                     IsResurrected = grp.Key.IsResurrected,
                                     OpenHandlings = grp.Where(ph => ph.ha.HandlingId > 0 && (ph.ha.IsCompleted == null || ph.ha.IsCompleted == false)).Count(),
                                     AllHandlings = grp.Where(ph => ph.ha.HandlingId > 0).Count()
                                 });
                    if (items.Any())
                    {
                        if (query != null)
                        {
                            items = items.Where(query);
                        }

                        if (length != null)
                        {
                            List<IProcessable> nItems = items.ToList<IProcessable>();
                            nItems = Static.Utilities.FilterByLength(nItems, length);
                            if (total == 0 && page > 0)
                            {
                                int pageSize = RuntimeSettings.PageSize;
                                var skip = pageSize * (page - 1);
                                if (skip < nItems.Count())
                                {
                                    nItems = nItems.Skip(skip).Take(pageSize).ToList();
                                    return Ok(nItems);
                                }
                                else
                                {
                                    return NotFound();
                                }
                            }
                            else if (total > 0 && page == 0)
                            {
                                nItems = nItems.Take(total).ToList();
                                return Ok(nItems);
                            }
                            else
                            {
                                return Ok(nItems);
                            }
                        }
                        else
                        {
                            if (total == 0 && page > 0)
                            {
                                int pageSize = RuntimeSettings.PageSize;
                                var skip = pageSize * (page - 1);
                                if (skip < items.Count())
                                {
                                    items = items.Skip(skip).Take(pageSize);
                                    return Ok(items);
                                }
                                else
                                {
                                    return NotFound();
                                }
                            }
                            else if (total > 0 && page == 0)
                            {
                                items = items.Take(total);
                                return Ok(items);
                            }
                            else
                            {
                                return Ok(items);
                            }
                        }
                    }
                    else
                    {
                        return NotFound();
                    }

                }
                else
                {
                    return NotFound();
                }

            }
            else
            {
                return NotFound();
            }
        }

        public string GetHandlingStatus(int ProcessId, bool open = false)
        {
            IQueryable<string> handlings;
            if (open)
            {
                handlings = (from h in db.JDE_Handlings
                             join u in db.JDE_Users on h.UserId equals u.UserId
                             where h.ProcessId==ProcessId && h.IsCompleted!=true
                             select u.Name + " " + u.Surname);
            }
            else
            {
                handlings = (from h in db.JDE_Handlings
                             join u in db.JDE_Users on h.UserId equals u.UserId
                             where h.ProcessId == ProcessId
                             select u.Name + " " + u.Surname);
            }

            if (handlings.Any())
            {
                string items = "Obsługiwane prze " + handlings.Aggregate((a, b) => a + ", " + b);
                return items;
            }
            else
            {
                if (open)
                {
                    return "Aktualnie nikt nie obsługuje tego zgłoszenia";
                }
                return "Nie zarejestrowano żadnej obsługi tego zgłoszenia";
            }

        }

        [HttpGet]
        [Route("GetProcessesExt")]
        public IHttpActionResult GetProcessesExt(string token, bool active = true, int page = 0, int total = 0)
        {

            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = (from p in db.JDE_Processes
                                 join uuu in db.JDE_Users on p.FinishedBy equals uuu.UserId into finished
                                 from fin in finished.DefaultIfEmpty()
                                 join t in db.JDE_Tenants on p.TenantId equals t.TenantId
                                 join u in db.JDE_Users on p.CreatedBy equals u.UserId
                                 join at in db.JDE_ActionTypes on p.ActionTypeId equals at.ActionTypeId
                                 join uu in db.JDE_Users on p.StartedBy equals uu.UserId into started
                                 from star in started.DefaultIfEmpty()
                                 join lsu in db.JDE_Users on p.LastStatusBy equals lsu.UserId into lastStatus
                                 from lStat in lastStatus.DefaultIfEmpty()
                                 join pl in db.JDE_Places on p.PlaceId equals pl.PlaceId
                                 join s in db.JDE_Sets on pl.SetId equals s.SetId
                                 join a in db.JDE_Areas on pl.AreaId equals a.AreaId
                                 where p.TenantId == tenants.FirstOrDefault().TenantId
                                 orderby p.CreatedOn descending
                                 select new
                                 {
                                     ProcessId = p.ProcessId,
                                     Description = p.Description,
                                     StartedOn = p.StartedOn,
                                     StartedBy = p.StartedBy,
                                     StartedByName = star.Name + " " + star.Surname,
                                     FinishedOn = p.FinishedOn,
                                     FinishedBy = p.FinishedBy,
                                     FinishedByName = fin.Name + " " + fin.Surname,
                                     ActionTypeId = p.ActionTypeId,
                                     ActionTypeName = at.Name,
                                     IsActive = p.IsActive,
                                     IsFrozen = p.IsFrozen,
                                     IsCompleted = p.IsCompleted,
                                     IsSuccessfull = p.IsSuccessfull,
                                     PlaceId = p.PlaceId,
                                     PlaceName = pl.Name,
                                     AreaId = a.AreaId,
                                     AreaName = a.Name,
                                     SetId = s.SetId,
                                     SetName = s.Name,
                                     Output = p.Output,
                                     TenantId = p.TenantId,
                                     TenantName = t.TenantName,
                                     CreatedOn = p.CreatedOn,
                                     CreatedBy = p.CreatedBy,
                                     CreatedByName = u.Name + " " + u.Surname,
                                     MesId = p.MesId,
                                     InitialDiagnosis = p.InitialDiagnosis,
                                     RepairActions = p.RepairActions,
                                     Reason = p.Reason,
                                     MesDate = p.MesDate,
                                     PlannedStart = p.PlannedStart,
                                     PlannedFinish = p.PlannedFinish,
                                     LastStatus = p.LastStatus == null ? (ProcessStatus?)null : (ProcessStatus)p.LastStatus, // Nullable enums handled
                                     LastStatusBy = p.LastStatusBy,
                                     LastStatusByName = lStat.Name + " " + lStat.Surname,
                                     LastStatusOn = p.LastStatusOn
                                 });
                    if (items.Any())
                    {
                        if (active)
                        {
                            items = items.Where(i => i.IsCompleted != true && i.IsSuccessfull != true);
                        }
                        if (total == 0 && page > 0)
                        {
                            int pageSize = RuntimeSettings.PageSize;
                            var skip = pageSize * (page - 1);
                            if (skip < items.Count())
                            {
                                items = items.Skip(skip).Take(pageSize);
                                return Ok(items);
                            }
                            else
                            {
                                return NotFound();
                            }
                        }
                        else if (total > 0 && page == 0)
                        {
                            items = items.Take(total);
                            return Ok(items);
                        }
                        else
                        {
                            return Ok(items);
                        }
                    }
                    else
                    {
                        return NotFound();
                    }

                }
                else
                {
                    return NotFound();
                }

            }
            else
            {
                return NotFound();
            }

        }

        [HttpGet]
        [Route("GetProcesses")]
        public IHttpActionResult GetProcesses(string token, string PlaceToken, bool active = false)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = (from p in db.JDE_Processes
                                 join uuu in db.JDE_Users on p.FinishedBy equals uuu.UserId into finished
                                 from fin in finished.DefaultIfEmpty()
                                 join t in db.JDE_Tenants on p.TenantId equals t.TenantId
                                 join u in db.JDE_Users on p.CreatedBy equals u.UserId
                                 join at in db.JDE_ActionTypes on p.ActionTypeId equals at.ActionTypeId
                                 join uu in db.JDE_Users on p.StartedBy equals uu.UserId into started
                                 from star in started.DefaultIfEmpty()
                                 join lsu in db.JDE_Users on p.LastStatusBy equals lsu.UserId into lastStatus
                                 from lStat in lastStatus.DefaultIfEmpty()
                                 join pl in db.JDE_Places on p.PlaceId equals pl.PlaceId
                                 join comp in db.JDE_Components on p.ComponentId equals comp.ComponentId into comps
                                 from components in comps.DefaultIfEmpty()
                                 where p.TenantId == tenants.FirstOrDefault().TenantId && pl.PlaceToken == PlaceToken
                                 orderby p.CreatedOn descending
                                 select new
                                 {
                                     ProcessId = p.ProcessId,
                                     Description = p.Description,
                                     StartedOn = p.StartedOn,
                                     StartedBy = p.StartedBy,
                                     StartedByName = star.Name + " " + star.Surname,
                                     FinishedOn = p.FinishedOn,
                                     FinishedBy = p.FinishedBy,
                                     FinishedByName = fin.Name + " " + fin.Surname,
                                     ActionTypeId = p.ActionTypeId,
                                     ActionTypeName = at.Name,
                                     IsActive = p.IsActive,
                                     IsFrozen = p.IsFrozen,
                                     IsCompleted = p.IsCompleted,
                                     IsSuccessfull = p.IsSuccessfull,
                                     PlaceId = p.PlaceId,
                                     PlaceName = pl.Name,
                                     ComponentId = p.ComponentId,
                                     ComponentName = components.Name,
                                     Output = p.Output,
                                     TenantId = p.TenantId,
                                     TenantName = t.TenantName,
                                     CreatedOn = p.CreatedOn,
                                     CreatedBy = p.CreatedBy,
                                     CreatedByName = u.Name + " " + u.Surname,
                                     MesId = p.MesId,
                                     InitialDiagnosis = p.InitialDiagnosis,
                                     RepairActions = p.RepairActions,
                                     Reason = p.Reason,
                                     MesDate = p.MesDate,
                                     PlannedStart = p.PlannedStart,
                                     PlannedFinish = p.PlannedFinish,
                                     IsResurrected = p.IsResurrected,
                                     LastStatus = p.LastStatus == null ? (ProcessStatus?)null : (ProcessStatus)p.LastStatus, // Nullable enums handled
                                     LastStatusBy = p.LastStatusBy,
                                     LastStatusByName = lStat.Name + " " + lStat.Surname,
                                     LastStatusOn = p.LastStatusOn
                                 });
                    if (items.Any())
                    {
                        if (active)
                        {
                            items = items.Where(i => i.IsCompleted != true && i.IsSuccessfull != true);
                        }
                        if (items.Any())
                        {
                            return Ok(items);
                        }
                        else
                        {
                            return NotFound();
                        }
                    }
                    else
                    {
                        return NotFound();
                    }

                }
                else
                {
                    return NotFound();
                }

            }
            else
            {
                return NotFound();
            }

        }

        [HttpGet]
        [Route("GetProcesses")]
        public IHttpActionResult GetProcesses(string token, int PlaceId, bool active = false)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = (from p in db.JDE_Processes
                                 join uuu in db.JDE_Users on p.FinishedBy equals uuu.UserId into finished
                                 from fin in finished.DefaultIfEmpty()
                                 join t in db.JDE_Tenants on p.TenantId equals t.TenantId
                                 join u in db.JDE_Users on p.CreatedBy equals u.UserId
                                 join at in db.JDE_ActionTypes on p.ActionTypeId equals at.ActionTypeId
                                 join uu in db.JDE_Users on p.StartedBy equals uu.UserId into started
                                 from star in started.DefaultIfEmpty()
                                 join lsu in db.JDE_Users on p.LastStatusBy equals lsu.UserId into lastStatus
                                 from lStat in lastStatus.DefaultIfEmpty()
                                 join pl in db.JDE_Places on p.PlaceId equals pl.PlaceId
                                 join comp in db.JDE_Components on p.ComponentId equals comp.ComponentId into comps
                                 from components in comps.DefaultIfEmpty()
                                 where p.TenantId == tenants.FirstOrDefault().TenantId && pl.PlaceId == PlaceId
                                 orderby p.CreatedOn descending
                                 select new
                                 {
                                     ProcessId = p.ProcessId,
                                     Description = p.Description,
                                     StartedOn = p.StartedOn,
                                     StartedBy = p.StartedBy,
                                     StartedByName = star.Name + " " + star.Surname,
                                     FinishedOn = p.FinishedOn,
                                     FinishedBy = p.FinishedBy,
                                     FinishedByName = fin.Name + " " + fin.Surname,
                                     ActionTypeId = p.ActionTypeId,
                                     ActionTypeName = at.Name,
                                     IsActive = p.IsActive,
                                     IsFrozen = p.IsFrozen,
                                     IsCompleted = p.IsCompleted,
                                     IsSuccessfull = p.IsSuccessfull,
                                     PlaceId = p.PlaceId,
                                     PlaceName = pl.Name,
                                     ComponentId = p.ComponentId,
                                     ComponentName = components.Name,
                                     Output = p.Output,
                                     TenantId = p.TenantId,
                                     TenantName = t.TenantName,
                                     CreatedOn = p.CreatedOn,
                                     CreatedBy = p.CreatedBy,
                                     CreatedByName = u.Name + " " + u.Surname,
                                     MesId = p.MesId,
                                     Comment = p.Comment,
                                     InitialDiagnosis = p.InitialDiagnosis,
                                     RepairActions = p.RepairActions,
                                     Reason = p.Reason,
                                     MesDate = p.MesDate,
                                     PlannedStart = p.PlannedStart,
                                     PlannedFinish = p.PlannedFinish,
                                     IsResurrected = p.IsResurrected,
                                     LastStatus = p.LastStatus == null ? (ProcessStatus?)null : (ProcessStatus)p.LastStatus, // Nullable enums handled
                                     LastStatusBy = p.LastStatusBy,
                                     LastStatusByName = lStat.Name + " " + lStat.Surname,
                                     LastStatusOn = p.LastStatusOn
                                 });
                    if (items.Any())
                    {
                        if (active)
                        {
                            items = items.Where(i => i.IsCompleted != true && i.IsSuccessfull != true);
                        }
                        if (items.Any())
                        {
                            return Ok(items);
                        }
                        else
                        {
                            return NotFound();
                        }
                    }
                    else
                    {
                        return NotFound();
                    }

                }
                else
                {
                    return NotFound();
                }

            }
            else
            {
                return NotFound();
            }

        }

        [HttpGet]
        [Route("GetProcess")]
        [ResponseType(typeof(JDE_Processes))]
        public IHttpActionResult GetProcess(string token, int id)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = (from p in db.JDE_Processes
                                 join uuu in db.JDE_Users on p.FinishedBy equals uuu.UserId into finished
                                 from fin in finished.DefaultIfEmpty()
                                 join t in db.JDE_Tenants on p.TenantId equals t.TenantId
                                 join u in db.JDE_Users on p.CreatedBy equals u.UserId
                                 join at in db.JDE_ActionTypes on p.ActionTypeId equals at.ActionTypeId
                                 join uu in db.JDE_Users on p.StartedBy equals uu.UserId into started
                                 from star in started.DefaultIfEmpty()
                                 join lsu in db.JDE_Users on p.LastStatusBy equals lsu.UserId into lastStatus
                                 from lStat in lastStatus.DefaultIfEmpty()
                                 join pl in db.JDE_Places on p.PlaceId equals pl.PlaceId
                                 join comp in db.JDE_Components on p.ComponentId equals comp.ComponentId into comps
                                 from components in comps.DefaultIfEmpty()
                                 join s in db.JDE_Sets on pl.SetId equals s.SetId
                                 join a in db.JDE_Areas on pl.AreaId equals a.AreaId
                                 where p.TenantId == tenants.FirstOrDefault().TenantId && p.ProcessId==id
                                 select new
                                 {
                                     ProcessId = p.ProcessId,
                                     Description = p.Description,
                                     StartedOn = p.StartedOn,
                                     StartedBy = p.StartedBy,
                                     StartedByName = star.Name + " " + star.Surname,
                                     FinishedOn = p.FinishedOn,
                                     FinishedBy = p.FinishedBy,
                                     FinishedByName = fin.Name + " " + fin.Surname,
                                     ActionTypeId = p.ActionTypeId,
                                     ActionTypeName = at.Name,
                                     IsActive = p.IsActive,
                                     IsFrozen = p.IsFrozen,
                                     IsCompleted = p.IsCompleted,
                                     IsSuccessfull = p.IsSuccessfull,
                                     IsResurrected = p.IsResurrected,
                                     PlaceId = p.PlaceId,
                                     PlaceName = pl.Name,
                                     ComponentId = p.ComponentId,
                                     ComponentName = components.Name,
                                     SetId = pl.SetId,
                                     SetName = s.Name,
                                     AreaId = pl.AreaId,
                                     AreaName = a.Name,
                                     Output = p.Output,
                                     TenantId = p.TenantId,
                                     TenantName = t.TenantName,
                                     CreatedOn = p.CreatedOn,
                                     CreatedBy = p.CreatedBy,
                                     CreatedByName = u.Name + " " + u.Surname,
                                     MesId = p.MesId,
                                     Comment = p.Comment,
                                     InitialDiagnosis = p.InitialDiagnosis,
                                     RepairActions = p.RepairActions,
                                     Reason = p.Reason,
                                     MesDate = p.MesDate,
                                     PlannedStart = p.PlannedStart,
                                     PlannedFinish = p.PlannedFinish,
                                     LastStatus = p.LastStatus == null ? (ProcessStatus?)null : (ProcessStatus)p.LastStatus, // Nullable enums handled
                                     LastStatusBy = p.LastStatusBy,
                                     LastStatusByName = lStat.Name + " " + lStat.Surname,
                                     LastStatusOn = p.LastStatusOn
                                 });
                    if (items.Any())
                    {
                        return Ok(items.FirstOrDefault());
                    }
                    else
                    {
                        return StatusCode(HttpStatusCode.NoContent);
                    }

                }
                else
                {
                    return NotFound();
                }

            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet]
        [Route("GetProcess")]
        [ResponseType(typeof(JDE_Processes))]
        public IHttpActionResult GetProcess(string token, string mesId)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = (from p in db.JDE_Processes
                                 join uuu in db.JDE_Users on p.FinishedBy equals uuu.UserId into finished
                                 from fin in finished.DefaultIfEmpty()
                                 join t in db.JDE_Tenants on p.TenantId equals t.TenantId
                                 join u in db.JDE_Users on p.CreatedBy equals u.UserId
                                 join at in db.JDE_ActionTypes on p.ActionTypeId equals at.ActionTypeId
                                 join uu in db.JDE_Users on p.StartedBy equals uu.UserId into started
                                 from star in started.DefaultIfEmpty()
                                 join lsu in db.JDE_Users on p.LastStatusBy equals lsu.UserId into lastStatus
                                 from lStat in lastStatus.DefaultIfEmpty()
                                 join pl in db.JDE_Places on p.PlaceId equals pl.PlaceId
                                 join comp in db.JDE_Components on p.ComponentId equals comp.ComponentId into comps
                                 from components in comps.DefaultIfEmpty()
                                 where p.TenantId == tenants.FirstOrDefault().TenantId && p.MesId.Equals(mesId)
                                 select new
                                 {
                                     ProcessId = p.ProcessId,
                                     Description = p.Description,
                                     StartedOn = p.StartedOn,
                                     StartedBy = p.StartedBy,
                                     StartedByName = star.Name + " " + star.Surname,
                                     FinishedOn = p.FinishedOn,
                                     FinishedBy = p.FinishedBy,
                                     FinishedByName = fin.Name + " " + fin.Surname,
                                     ActionTypeId = p.ActionTypeId,
                                     ActionTypeName = at.Name,
                                     IsActive = p.IsActive,
                                     IsFrozen = p.IsFrozen,
                                     IsCompleted = p.IsCompleted,
                                     IsSuccessfull = p.IsSuccessfull,
                                     PlaceId = p.PlaceId,
                                     PlaceName = pl.Name,
                                     ComponentId = p.ComponentId,
                                     ComponentName = components.Name,
                                     Output = p.Output,
                                     TenantId = p.TenantId,
                                     TenantName = t.TenantName,
                                     CreatedOn = p.CreatedOn,
                                     CreatedBy = p.CreatedBy,
                                     CreatedByName = u.Name + " " + u.Surname,
                                     MesId = p.MesId,
                                     Comment = p.Comment,
                                     InitialDiagnosis = p.InitialDiagnosis,
                                     RepairActions = p.RepairActions,
                                     Reason = p.Reason,
                                     MesDate = p.MesDate,
                                     PlannedStart = p.PlannedStart,
                                     PlannedFinish = p.PlannedFinish,
                                     IsResurrected = p.IsResurrected,
                                     LastStatus = p.LastStatus == null ? (ProcessStatus?)null : (ProcessStatus)p.LastStatus, // Nullable enums handled
                                     LastStatusBy = p.LastStatusBy,
                                     LastStatusByName = lStat.Name + " " +lStat.Surname,
                                     LastStatusOn = p.LastStatusOn
                                 });
                    if (items.Any())
                    {
                        return Ok(items.FirstOrDefault());
                    }
                    else
                    {
                        return StatusCode(HttpStatusCode.NoContent);
                    }

                }
                else
                {
                    return NotFound();
                }

            }
            else
            {
                return NotFound();
            }
        }

        [HttpPut]
        [Route("EditProcess")]
        [ResponseType(typeof(void))]

        public IHttpActionResult EditProcess(string token, int id, int UserId, JDE_Processes item, bool UseServerDates=true)
        {
            Logger.Info("Start EditProcess. Id={id}, UserId={UserId}", id, UserId);
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    try
                    {
                        var items = db.JDE_Processes.AsNoTracking().Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.ProcessId == id);
                        if (items.Any())
                        {
                            item.CreatedOn = items.FirstOrDefault().CreatedOn;
                            if (items.FirstOrDefault().StartedOn != null)
                            {
                                //It has already had a date. Keep it
                                item.StartedOn = items.FirstOrDefault().StartedOn;
                            }
                            else
                            {
                                if (item.StartedOn != null && UseServerDates)
                                {
                                    //this has just been started. Must have been planned before. Replace user's date
                                    item.StartedOn = DateTime.Now;
                                    item.LastStatus = (int)ProcessStatus.Started;
                                    item.LastStatusBy = UserId;
                                    item.LastStatusOn = DateTime.Now;

                                    //if ActionType of this process requires closing all previous processes of the type, do it now
                                    //Utilities.CompleteAllProcessesOfTheTypeInThePlaceAsync(db,(int)item.PlaceId, (int)item.ActionTypeId, id, UserId, "Zamknięte ponieważ nowsze zgłoszenie tego typu zostało rozpoczęte");

                                }
                            }
                            string descr = "Edycja zgłoszenia";
                            if ((bool)items.FirstOrDefault().IsCompleted && (bool)item.IsCompleted == false)
                            {
                                //it was completed and it's not anymore - it's been resurrected
                                Logger.Info("EditProcess - zgłoszenie Id={id} zostało reaktywowane przez {UserId}", id, UserId);
                                item.LastStatus = (int)ProcessStatus.Resumed;
                                item.LastStatusBy = UserId;
                                item.LastStatusOn = DateTime.Now;
                                item.IsResurrected = true;
                            }else if ((bool)items.FirstOrDefault().IsActive && (bool)item.IsFrozen)
                            {
                                Logger.Info("EditProcess - zgłoszenie Id={id} zostało wstrzymane przez {UserId}", id, UserId);
                                //was active and it no longer is. It has been paused
                                item.LastStatus = (int)ProcessStatus.Paused;
                                item.LastStatusBy = UserId;
                                item.LastStatusOn = DateTime.Now;
                            }
                            else if ((bool)items.FirstOrDefault().IsFrozen && (bool)item.IsActive)
                            {
                                Logger.Info("EditProcess - zgłoszenie Id={id} zostało wznowione przez {UserId}", id, UserId);
                                //was paused and now it is active - it's been resumed
                                item.LastStatus = (int)ProcessStatus.Resumed;
                                item.LastStatusBy = UserId;
                                item.LastStatusOn = DateTime.Now;
                            }
                            else if (!(bool)items.FirstOrDefault().IsActive && (bool)item.IsActive)
                            {
                                //wasn't active and now it is - it's been started
                                Logger.Info("EditProcess - zgłoszenie Id={id} zostało rozpoczęte przez {UserId}", id, UserId);
                                item.LastStatus = (int)ProcessStatus.Started;
                                item.LastStatusBy = UserId;
                                item.LastStatusOn = DateTime.Now;
                            }
                            else if (!(bool)items.FirstOrDefault().IsCompleted && (bool)item.IsCompleted)
                            {
                                //it's been finished
                                Logger.Info("EditProcess - zgłoszenie Id={id} zostało zakończone przez {UserId}", id, UserId);
                                item.LastStatus = (int)ProcessStatus.Finished;
                                item.LastStatusBy = UserId;
                                item.LastStatusOn = DateTime.Now;
                            }
                            if (items.FirstOrDefault().FinishedOn == null && item.FinishedOn != null)
                            {
                                //this has just been finished. Replace user's finish time with server time
                                Logger.Info("EditProcess - użytkownik {UserId} zainicjował zamykanie otwartych obsług w zgłoszeniu {id}",  UserId, id);
                                CompleteProcessesHandlings(item.ProcessId, UserId);
                                item.LastStatus = (int)ProcessStatus.Finished;
                                item.LastStatusBy = UserId;
                                item.LastStatusOn = DateTime.Now;
                                if (UseServerDates)
                                {
                                    item.FinishedOn = DateTime.Now;
                                }
                                descr = "Zamknięcie zgłoszenia";
                            }
                            JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = descr, TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(items.FirstOrDefault()), NewValue = new JavaScriptSerializer().Serialize(item) };
                            db.JDE_Logs.Add(Log);
                            db.Entry(item).State = EntityState.Modified;
                            try
                            {
                                db.SaveChanges();
                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                if (!JDE_ProcessesExists(id))
                                {
                                    return NotFound();
                                }
                                else
                                {
                                    throw;
                                }
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        Logger.Error("Błąd w EditProcess. Id={id}, UserId={UserId}. Szczegóły: {Message}", id, UserId, ex.ToString());
                        return StatusCode(HttpStatusCode.InternalServerError);
                    }
                }
                else
                {
                    Logger.Info("Process Id={id} nie zostało znalezione..", id);
                }
            }
            Logger.Info("Koniec EditProcess. Id={id}, UserId={UserId}", id, UserId);
            return StatusCode(HttpStatusCode.NoContent);
        }


        [HttpPost]
        [Route("CreateProcess")]
        [ResponseType(typeof(JDE_Processes))]
        public IHttpActionResult CreateProcess(string token, JDE_Processes item, int UserId)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    item.TenantId = tenants.FirstOrDefault().TenantId;
                    item.CreatedOn = DateTime.Now;
                    if ((bool)item.IsActive)
                    {
                        item.LastStatus = (int)ProcessStatus.Started;
                    }
                    else
                    {
                        item.LastStatus = (int)ProcessStatus.Planned;
                    }
                    item.LastStatusBy = UserId;
                    item.LastStatusOn = DateTime.Now;
                    if (item.StartedOn != null)
                    {
                        item.StartedOn = DateTime.Now;
                    }
                    db.JDE_Processes.Add(item);
                    db.SaveChanges();
                    JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Utworzenie zgłoszenia", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, NewValue = new JavaScriptSerializer().Serialize(item) };
                    db.JDE_Logs.Add(Log);
                    db.SaveChanges();
                    return Ok(item);
                    //return CreatedAtRoute("DefaultApi", new { id = item.ProcessId }, item);
                }
                else
                {
                    return NotFound();
                }
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet]
        [Route("GetProcessStats")]
        public IHttpActionResult GetProcessStats(string token, DateTime dateFrom, DateTime dateTo)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    try
                    {
                        using (SqlConnection Con = new SqlConnection(Secrets.ApiConnectionString))
                        {
                            string sql = $@"SELECT at.ActionTypeId, at.Name, at.ShowOnDashboard, at.ActionsApplicable, COUNT(p.ProcessId) AS [Count], SUM(DATEDIFF(mi, h.StartedOn, h.FinishedOn)) AS [HandlingSum], SUM(DATEDIFF(mi, p.StartedOn, p.FinishedOn)) AS [ProcessSum] 
                                FROM JDE_Handlings h LEFT JOIN JDE_Processes p ON h.ProcessId = p.ProcessId 
	                                LEFT JOIN JDE_ActionTypes at ON p.ActionTypeId = at.ActionTypeId
                                WHERE h.StartedOn >= @dateFrom AND h.FinishedOn < @dateTo
                                GROUP BY at.ActionTypeId, at.Name, at.ShowOnDashboard, at.ActionsApplicable";
                            SqlParameter[] parameters = new SqlParameter[2];
                            parameters[0] = new SqlParameter("@dateFrom", dateFrom);
                            parameters[1] = new SqlParameter("@dateTo", dateTo);

                            SqlCommand command = new SqlCommand(sql, Con);
                            command.Parameters.AddRange(parameters);

                            if (Con.State == ConnectionState.Closed || Con.State == ConnectionState.Broken)
                            {
                                Con.Open();
                            }

                            List<dynamic> processes = new List<dynamic>();
                            List<dynamic> newProcesses = new List<dynamic>();

                            SqlDataReader reader = command.ExecuteReader();
                            int totalResult = 0;

                            while (reader.Read())
                            {
                                int hs = 0;
                                int ps = 0;
                                bool parsable = int.TryParse(reader["HandlingSum"].ToString(), out hs);
                                parsable = int.TryParse(reader["ProcessSum"].ToString(), out ps);
                                int result = hs < ps ? hs : ps;
                                bool showOnDashboard = reader.IsDBNull(reader.GetOrdinal("ShowOnDashboard")) == true ? false : Convert.ToBoolean(reader["ShowOnDashboard"].ToString());
                                bool actionsApplicable = reader.IsDBNull(reader.GetOrdinal("ActionsApplicable")) == true ? false : Convert.ToBoolean(reader["ActionsApplicable"].ToString());
                                totalResult += result;

                                var item = new
                                {
                                    ActionTypeId = reader["ActionTypeId"],
                                    Name = reader["Name"],
                                    ShowOnDashboard = showOnDashboard,
                                    ActionsApplicable = actionsApplicable,
                                    Count = reader.GetInt32(reader.GetOrdinal("Count")),
                                    HandlingSum = hs,
                                    ProcessSum = ps,
                                    Result = result,
                                };
                                processes.Add(item);
                            }
                            if (processes.Any())
                            {
                                foreach(var item in processes)
                                {
                                    var newItem = new
                                    {
                                        ActionTypeId = item.ActionTypeId,
                                        Name = item.Name,
                                        ShowOnDashboard = item.ShowOnDashboard,
                                        ActionsApplicable = item.ActionsApplicable,
                                        Count = item.Count,
                                        HandlingSum = item.HandlingSum,
                                        ProcessSum = item.ProcessSum,
                                        Result = item.Result,
                                        PercentOfAll = ((double)item.Result / (double)totalResult) * 100
                                    };
                                    newProcesses.Add(newItem);
                                }
                            }

                            return Ok(newProcesses);
                        }
                    }
                    catch (Exception ex)
                    {

                        return InternalServerError(ex);
                    }
                }
                else
                {
                    return NotFound();
                }
            }
            else
            {
                return NotFound();
            }
        }

        [HttpDelete]
        [Route("DeleteProcess")]
        [ResponseType(typeof(JDE_Processes))]
        public IHttpActionResult DeleteProcess(string token, int id, int UserId)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = db.JDE_Processes.Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.ProcessId == id);
                    if (items.Any())
                    {
                        JDE_Users User = db.JDE_Users.AsNoTracking().FirstOrDefault(u => u.UserId == UserId);
                        CompleteProcessesHandlings(items.FirstOrDefault().ProcessId, UserId, $"Obsługa zakończona przy usuwaniu zgłoszenia przez {User.Name + " " + User.Surname}");
                        JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Usunięcie zgłoszenia", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(items.FirstOrDefault()) };
                        db.JDE_Processes.Remove(items.FirstOrDefault());
                        db.JDE_Logs.Add(Log);
                        db.SaveChanges();

                        return Ok(items.FirstOrDefault());
                    }
                    else
                    {
                        return NotFound();
                    }
                }
                else
                {
                    return NotFound();
                }
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPut]
        [Route("CompleteAllProcessesOfTheTypeInThePlace")]
        public IHttpActionResult CompleteAllProcessesOfTheTypeInThePlace(string token, int thePlace, int theType, int excludeProcess, int UserId, string reasonForClosure = null)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    bool? requireClosing = db.JDE_ActionTypes.Where(i => i.ActionTypeId == theType).FirstOrDefault().ClosePreviousInSamePlace;
                    if (requireClosing == null) { requireClosing = false; }
                    if ((bool)requireClosing)
                    {
                        IQueryable<JDE_Processes> processes = null;
                        processes = db.JDE_Processes.AsNoTracking().Where(p => p.PlaceId == thePlace && p.ActionTypeId == theType && p.ProcessId < excludeProcess && (p.IsCompleted == false || p.IsCompleted == null) && (p.IsSuccessfull == false || p.IsSuccessfull == null));
                        if (processes.Any())
                        {
                            try
                            {
                                foreach (var p in processes.ToList())
                                {
                                    _CompleteProcess(p.ProcessId, UserId, reasonForClosure);
                                }
                            }
                            catch (Exception ex)
                            {
                                return StatusCode(HttpStatusCode.InternalServerError);
                            }
                            return StatusCode(HttpStatusCode.NoContent);
                        }
                        else
                        {
                            return NotFound();
                        }
                    }
                    else
                    {
                        return StatusCode(HttpStatusCode.NoContent);
                    }
                }
                else
                {
                    return NotFound();
                }

            }
            else
            {
                return NotFound();
            }
            
        }

        [HttpPut]
        [Route("CompleteProcess")]
        public IHttpActionResult CompleteProcess(string token, int id, int UserId, string reasonForClosure=null)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = db.JDE_Processes.AsNoTracking().Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.ProcessId == id);
                    if (items.Any())
                    {
                        try
                        {
                            _CompleteProcess(items.FirstOrDefault().ProcessId, UserId, reasonForClosure);
                        }
                        catch(Exception ex)
                        {
                            return StatusCode(HttpStatusCode.InternalServerError);
                        }
                        

                        return StatusCode(HttpStatusCode.NoContent);
                    }
                    else
                    {
                        return NotFound();
                    }
                }
                else
                {
                    return NotFound();
                }
            }
            else
            {
                return NotFound();
            }
        }

        private void _CompleteProcess(int ProcessId, int UserId, string reasonForClosure = null)
        {
            JDE_Processes item = null;
            if (db.JDE_Processes.Any(p => p.ProcessId == ProcessId))
            {
                item = db.JDE_Processes.FirstOrDefault(p => p.ProcessId == ProcessId);
                string OldValue = new JavaScriptSerializer().Serialize(item);
                item.FinishedOn = DateTime.Now;
                item.FinishedBy = UserId;
                item.IsActive = false;
                item.IsCompleted = true;
                item.IsFrozen = false;
                item.LastStatus = (int)ProcessStatus.Finished;
                item.LastStatusBy = UserId;
                item.LastStatusOn = DateTime.Now;
                var User = db.JDE_Users.AsNoTracking().Where(u => u.UserId == UserId).FirstOrDefault();
                if (reasonForClosure == null)
                {
                    item.Output = $"Przymusowe zamknięcie zgłoszenia przez {User.Name + " " + User.Surname}";
                }
                else
                {
                    item.Output = reasonForClosure;
                }
                CompleteProcessesHandlings(item.ProcessId, UserId);
                JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Zamknięcie zgłoszenia", TenantId = item.TenantId, Timestamp = DateTime.Now, OldValue = OldValue, NewValue = new JavaScriptSerializer().Serialize(item) };
                db.JDE_Logs.Add(Log);
                db.Entry(item).State = EntityState.Modified;
                db.SaveChanges();
            }
            
        }

        public void CompleteProcessesHandlings(int ProcessId, int UserId, string reasonForClosure = null)
        {
            //it completes all open handlings for given process
            Logger.Info("Start CompleteProcessesHandlings. Id={ProcessId}, UserId={UserId}", ProcessId, UserId);
            string descr = string.Empty;
            var items = db.JDE_Handlings.AsNoTracking().Where(p => p.ProcessId == ProcessId && p.IsCompleted == false);
            var User = db.JDE_Users.AsNoTracking().Where(u => u.UserId == UserId).FirstOrDefault();

            if (items.Any())
            {
                foreach (var item in items.ToList())
                {
                    item.FinishedOn = DateTime.Now;
                    item.IsActive = false;
                    item.IsFrozen = false;
                    item.IsCompleted = true;
                    if (reasonForClosure == null)
                    {
                        item.Output = $"Obsługa została zakończona przy zamykaniu zgłoszenia przez {User.Name + " " + User.Surname}";
                    }
                    else
                    {
                        item.Output = reasonForClosure;
                    }

                    descr = "Zakończenie obsługi";
                    JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = descr, TenantId = User.TenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(items.FirstOrDefault()), NewValue = new JavaScriptSerializer().Serialize(item) };
                    db.JDE_Logs.Add(Log);
                    db.Entry(item).State = EntityState.Modified;
                }
                try
                {
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    Logger.Error("Błąd zapisu do bazy danych w CompleteProcessesHandlings. Id={ProcessId}, UserId={UserId}, Wiadomość: {Message}", ProcessId, UserId, ex.Message);
                }
            }

            Logger.Info("Koniec CompleteProcessesHandlings. Id={ProcessId}, UserId={UserId}", ProcessId, UserId);
        }

        [HttpPut]
        [Route("AddComment")]
        public IHttpActionResult AddComment(string token, int ProcessId, string comment, int UserId)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {

                    var items = db.JDE_Processes.AsNoTracking().Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.ProcessId==ProcessId);
                    if (items.Any())
                    {
                        JDE_Processes item = items.FirstOrDefault();
                        var orgProcess = new JavaScriptSerializer().Serialize(item);
                        item.Comment = comment;
                        db.Entry(item).State = EntityState.Modified;

                        JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Dodanie komentarza do zgłoszenia", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, OldValue = orgProcess, NewValue = new JavaScriptSerializer().Serialize(item) };
                        db.JDE_Logs.Add(Log);
                        try
                        {
                            db.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            Logger.Error("Błąd zapisu do bazy danych w AddComment. UserId={UserId}, Comment={Comment}, Wiadomość: {Message}", UserId, comment, ex.Message);
                        }
                    }
                    else
                    {
                        return StatusCode(HttpStatusCode.BadRequest);
                    }
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpPut]
        [Route("AssignUsers")]
        public IHttpActionResult AssignUsers(string token, int ProcessId, List<JDE_Users> Users, int UserId)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    //get assignable action types, so you won't assign a user to non-assignable action type

                    List<int> AssignableTypes = db.JDE_ActionTypes.Where(a => a.ShowInPlanning == true).Select(a=>a.ActionTypeId).ToList();

                    var items = db.JDE_Processes.AsNoTracking().Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.ProcessId == ProcessId && AssignableTypes.Contains((int)u.ActionTypeId));
                    if (items.Any())
                    {
                        //delete all current assigns that are NOT in new list

                        List<int> UserIds = Users.Select(o => o.UserId).ToList();

                        var orgUsers = new Dictionary<string, string>
                        {
                            {"ProcessId", ProcessId.ToString() },
                            {"Users", string.Join(",",db.JDE_ProcessAssigns.Where(p=>p.ProcessId==ProcessId).Select(p=>p.UserId)) }
                        };

                        var paks = db.JDE_ProcessAssigns.Where(p => p.ProcessId == ProcessId && !UserIds.Contains((int)p.UserId));
                        if (paks.Any())
                        {
                            foreach(JDE_ProcessAssigns pa in paks)
                            {
                                db.JDE_ProcessAssigns.Remove(pa);
                            }
                            db.SaveChanges();
                        }

                        var newUsers = new Dictionary<string, string>
                        {
                            {"ProcessId", ProcessId.ToString() },
                            {"Users", string.Join(",",UserIds) }
                        };

                        paks = db.JDE_ProcessAssigns.Where(p => p.ProcessId == ProcessId);
                        //assign Users to the process

                        foreach(JDE_Users u in Users)
                        {
                            if (!paks.Any(p => p.UserId == u.UserId))
                            {
                                JDE_ProcessAssigns pa = new JDE_ProcessAssigns();
                                pa.UserId = u.UserId;
                                pa.ProcessId = ProcessId;
                                pa.TenantId = tenants.FirstOrDefault().TenantId;
                                pa.CreatedBy = UserId;
                                pa.CreatedOn = DateTime.Now;
                                db.JDE_ProcessAssigns.Add(pa);
                                
                            }
                        }
                        db.SaveChanges();

                        JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Zmiana użytkowników przypisanych do zgłoszenia", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(orgUsers), NewValue = new JavaScriptSerializer().Serialize(newUsers) };
                        db.JDE_Logs.Add(Log);
                        db.SaveChanges();
                    }
                    else
                    {
                        return StatusCode(HttpStatusCode.BadRequest);
                    }
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpGet]
        [Route("GetProcessHistory")]
        public IHttpActionResult GetProcessHistory(string token, int id)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = (from l in db.JDE_Logs
                                 join u in db.JDE_Users on l.UserId equals u.UserId
                                 join t in db.JDE_Tenants on l.TenantId equals t.TenantId
                                 where l.TenantId == tenants.FirstOrDefault().TenantId
                                 orderby l.Timestamp descending
                                 select new ExtLog
                                 {
                                     LogId = l.LogId,
                                     TimeStamp = l.Timestamp,
                                     TenantId = t.TenantId,
                                     TenantName = t.TenantName,
                                     UserId = l.UserId,
                                     UserName = u.Name + " " + u.Surname,
                                     Description = l.Description,
                                     OldValue = l.OldValue,
                                     NewValue = l.NewValue
                                 });
                    if (items.Any())
                    {
                        string tId = "ProcessId\":" + id;
                        items = items.Where(i => i.NewValue.Contains(tId) || i.OldValue.Contains(tId));
                        List<ProcessHisotryItem> nItems = new List<ProcessHisotryItem>();
                        if (items.Any())
                        {

                            foreach (var item in items)
                            {

                                ProcessHisotryItem hItem = new ProcessHisotryItem
                                {
                                    LogId = item.LogId,
                                    UserId = item.UserId,
                                    UserName = item.UserName,
                                    Timestamp = item.TimeStamp,
                                    ProcessId = id,
                                    TenantId = item.TenantId,
                                    TenantName = item.TenantName
                                };
                                if (item.Description.Equals("Edycja zgłoszenia"))
                                {
                                    hItem.Description = GetProcessChange(item.OldValue, item.NewValue);
                                }
                                else
                                {
                                    hItem.Description = item.Description;
                                }
                                nItems.Add(hItem);
                            }
                            return Ok(nItems);
                        }
                        else
                        {
                            return NotFound();
                        }
                    }
                    else
                    {
                        return NotFound();
                    }
                }
                else
                {
                    return NotFound();
                }

            }
            else
            {
                return NotFound();
            }
        }

        private string GetProcessChange(string OldValue, string NewValue)
        {
            Models.DbModel db = new Models.DbModel();

            string res = "";

            try
            {
                JavaScriptSerializer js = new JavaScriptSerializer();
                dynamic nv = js.DeserializeObject(NewValue);
                dynamic ov = js.DeserializeObject(OldValue);
                if (((bool)nv["IsCompleted"] || (bool)nv["IsSuccessfull"]) && ((bool)ov["IsCompleted"] == false && (bool)ov["IsSuccessfull"] == false))
                {
                    res = "Zamknięcie zgłoszenia";
                }
                else if ((bool)nv["IsFrozen"] && (bool)ov["IsFrozen"] == false)
                {
                    res = "Wstrzymanie zgłoszenia";
                }
                else if ((bool)nv["IsFrozen"] == false && (bool)ov["IsFrozen"])
                {
                    res = "Wznowienie zgłoszenia";
                }
                else if((bool)nv["IsCompleted"] == false && (bool)ov["IsCompleted"] == true)
                {
                    res = "Reaktywacja zgłoszenia";
                }
            }
            catch (Exception ex)
            {
                res = "Deserializacja nie powiodła się";
            }
            
            return res;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool JDE_ProcessesExists(int id)
        {
            return db.JDE_Processes.Count(e => e.ProcessId == id) > 0;
        }
    }
    
    public class ProcessHisotryItem
    {
        public int LogId { get; set; }
        public int ProcessId { get; set; }
        public DateTime? Timestamp { get; set; }
        public int? UserId { get; set; }
        public string UserName{ get; set; }
        public string Description { get; set; }
        public int? TenantId { get; set; }
        public string TenantName { get; set; }
    }

    public class ProcessQuery
    {
        public string OrginalQuery { get; set; }
        public string Query { get; set; }
        public string Length { get; set; }
        public string ProcessLength { get; set; }
        public string HandlingsLength { get; set; }
        public string Status { get; set; }
        public string AssignedUserNames { get; set; }
        public string TimingStatus { get; set; }

        public string TimingVsPlan { get; set; }

        public ProcessQuery(string query)
        {
            this.OrginalQuery = query;
            this.Query = query;
            int start = 0;
            int end = 0;
            if (query.IndexOf("ProcessLength") >= 0)
            {
                //contains length
                start = query.IndexOf("ProcessLength");

                if (query.IndexOf("AND", start) >= 0)
                {
                    //it has got more parameters later
                    end = query.IndexOf(" ", start);
                }
                else
                {
                    //it's the last parameter
                    end = query.Length;
                }

                this.ProcessLength = query.Substring(start, end - start);
                this.Query = query.Replace(this.ProcessLength, "");
                //check if we need to remove some ANDs from query
                DropAnd();

                if (this.ProcessLength.Contains("ProcessLength"))
                {
                    //still contains length keyword
                    int x = this.ProcessLength.IndexOf("ProcessLength") + 6;
                    this.ProcessLength = this.ProcessLength.Remove(0, x);
                }
            }
            if (query.IndexOf("HandlingsLength") >= 0)
            {
                //contains length
                start = query.IndexOf("HandlingsLength");

                if (query.IndexOf("AND", start) >= 0)
                {
                    //it has got more parameters later
                    end = query.IndexOf(" ", start);
                }
                else
                {
                    //it's the last parameter
                    end = query.Length;
                }

                this.HandlingsLength = query.Substring(start, end - start);
                this.Query = query.Replace(this.HandlingsLength, "");
                //check if we need to remove some ANDs from query
                DropAnd();

                if (this.HandlingsLength.Contains("HandlingsLength"))
                {
                    //still contains length keyword
                    int x = this.HandlingsLength.IndexOf("HandlingsLength") + 6;
                    this.HandlingsLength = this.HandlingsLength.Remove(0, x);
                }
            }
            if (query.IndexOf("Length")>=0)
            {
                //contains length
                start = query.IndexOf("Length");

                if (query.IndexOf("AND", start) >= 0)
                {
                    //it has got more parameters later
                    end = query.IndexOf(" ", start);
                }
                else
                {
                    //it's the last parameter
                    end = query.Length;
                }
                
                this.Length = query.Substring(start, end - start);
                this.Query = query.Replace(this.Length, "");
                //check if we need to remove some ANDs from query
                DropAnd();

                if (this.Length.Contains("Length"))
                {
                    //still contains length keyword
                    int x = this.Length.IndexOf("Length")+6;
                    this.Length = this.Length.Remove(0, x);
                }
            }
            
            if (this.Query.IndexOf("TimingStatus") >= 0)
            {
                if (this.Query.Contains("!TimingStatus"))
                {
                    //this is "doesn't contain" filter
                    start = this.Query.IndexOf("!TimingStatus");
                }
                else
                {
                    start = this.Query.IndexOf("TimingStatus");
                }

                if (this.Query.IndexOf("AND", start) >= 0)
                {
                    //it has got more parameters later
                    end = this.Query.IndexOf(" ", start);
                }
                else
                {
                    //it's the last parameter
                    end = this.Query.Length;
                }
                this.TimingStatus = this.Query.Substring(start, end - start);
                this.Query = this.Query.Replace(this.TimingStatus, "");
                DropAnd();
            }
            if (this.Query.IndexOf("TimingVsPlan") >= 0)
            {
                if (this.Query.Contains("!TimingVsPlan"))
                {
                    //this is "doesn't contain" filter
                    start = this.Query.IndexOf("!TimingVsPlan");
                }
                else
                {
                    start = this.Query.IndexOf("TimingVsPlan");
                }

                if (this.Query.IndexOf("AND", start) >= 0)
                {
                    //it has got more parameters later
                    end = this.Query.IndexOf(" ", start);
                }
                else
                {
                    //it's the last parameter
                    end = this.Query.Length;
                }
                this.TimingVsPlan = this.Query.Substring(start, end - start);
                this.Query = this.Query.Replace(this.TimingVsPlan, "");
                DropAnd();
            }

            if (this.Query.IndexOf("Status") >= 0)
            {
                if (this.Query.Contains("!Status"))
                {
                    //this is "doesn't contain" filter
                    start = this.Query.IndexOf("!Status");
                }
                else
                {
                    start = this.Query.IndexOf("Status");
                }
                
                if (this.Query.IndexOf("AND", start) >= 0)
                {
                    //it has got more parameters later
                    end = this.Query.IndexOf(" ", start);
                }
                else
                {
                    //it's the last parameter
                    end = this.Query.Length;
                }
                this.Status = this.Query.Substring(start, end - start);
                this.Query = this.Query.Replace(this.Status, "");
                DropAnd();
            }
            if (this.Query.IndexOf("AssignedUserNames") >= 0)
            {
                if (this.Query.Contains("!AssignedUserNames"))
                {
                    //this is "doesn't contain" filter
                    start = this.Query.IndexOf("!AssignedUserNames");
                }
                else
                {
                    start = this.Query.IndexOf("AssignedUserNames");
                }

                if (this.Query.IndexOf("AND", start) >= 0)
                {
                    //it has got more parameters later
                    end = this.Query.IndexOf(" ", start);
                }
                else
                {
                    //it's the last parameter
                    end = this.Query.Length;
                }
                this.AssignedUserNames = this.Query.Substring(start, end - start);
                this.Query = this.Query.Replace(this.AssignedUserNames, "");
                DropAnd();
            }
        }

        public void DropAnd()
        {
            if (!string.IsNullOrEmpty(Query))
            {
                if (Query.Contains("AND"))
                {
                    string[] a = Regex.Split(Query, "AND");
                    int len = 0;
                    int start;
                    int end;
                    string output = "";

                    a = a.Where(val => val != " " && val != "  " && val != "! ").ToArray();//drop empty elements, what remains are goodies

                    for (int i = 0; i < a.Length; i++)
                    {
                        output += a[i] + "AND";
                    }
                    //drop last AND
                    output = output.Substring(0, output.Length - 3);
                    output = output.Trim();
                    Query = output;
                }
            }
        }
    }


}