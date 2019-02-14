using System;
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

namespace JDE_API.Controllers
{
    public class ProcessController : ApiController
    {
        private Models.DbModel db = new Models.DbModel();

        [HttpGet]
        [Route("GetProcesses")]
        public IHttpActionResult GetProcesses(string token, int page=0, int total=0, DateTime? dFrom = null, DateTime? dTo = null, string query = null, string length = null)
        {
            //if ext=true then there's more columns in the result sent
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    dFrom = dFrom ?? db.JDE_Processes.Min(x => x.CreatedOn).Value;
                    dTo = dTo ?? db.JDE_Processes.Max(x => x.CreatedOn).Value;

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
                                 join h in db.JDE_Handlings on p.ProcessId equals h.ProcessId into hans
                                 from ha in hans.DefaultIfEmpty()
                                 where p.TenantId == tenants.FirstOrDefault().TenantId && p.CreatedOn >= dFrom && p.CreatedOn <= dTo
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
                                     TenantName = t.TenantName,
                                     p.IsActive,
                                     p.IsCompleted,
                                     p.IsFrozen,
                                     p.IsSuccessfull,
                                     ActionTypeName = at.Name,
                                     FinishedByName = fin.Name + " " + fin.Surname,
                                     StartedByName = star.Name + " " + star.Surname,
                                     PlaceName = pl.Name,
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
                                     PlannedStart = grp.Key.PlannedStart,
                                     PlannedFinish = grp.Key.PlannedFinish,
                                     LastStatus = grp.Key.LastStatus,
                                     LastStatusBy = grp.Key.LastStatusBy,
                                     LastStatusByName = grp.Key.LastStatusByName,
                                     LastStatusOn = grp.Key.LastStatusOn,
                                     OpenHandlings = grp.Where(ph => ph.ha.IsCompleted == null && ph.ha.HandlingId > 0).Count(),
                                     AllHandlings = grp.Count()
                                 });
                    if (items.Any())
                    {
                        if (query != null)
                        {
                            items = items.Where(query);
                        }

                        if (length != null)
                        {
                            var nItems = items.ToList();
                            nItems = FilterByLength(nItems, length);
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

        public List<Process> FilterByLength(List<Process> nItems, string length)
        {
            var min = Regex.Match(length, @"\d+").Value;
            int mins = 0;
            int.TryParse(min, out mins);
            var sign = length.Substring(0, length.Length - min.Length);
            if ((sign.Equals(">") || sign.Equals("<") || sign.Equals("=<") || sign.Equals("<=") || sign.Equals("=>") || sign.Equals(">=") || sign.Equals("=")) && mins >= 0)
            {
                // don't do anything unless you've got both min and sign
                if (sign.Equals("="))
                {
                    nItems = nItems.Where(i => i.Length == mins).ToList();
                }
                else if (sign.Equals("<=") || sign.Equals("=<"))
                {
                    nItems = nItems.Where(i => i.Length <= mins).ToList();
                }
                else if (sign.Equals(">=") || sign.Equals("=>"))
                {
                    nItems = nItems.Where(i => i.Length >= mins).ToList();
                }
                else if (sign.Equals(">"))
                {
                    nItems = nItems.Where(i => i.Length > mins).ToList();
                }
                else if (sign.Equals("<"))
                {
                    nItems = nItems.Where(i => i.Length < mins).ToList();
                }

            }
            return nItems;
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
                                     PlaceId = p.PlaceId,
                                     PlaceName = pl.Name,
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
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = db.JDE_Processes.AsNoTracking().Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.ProcessId == id);
                    if (items.Any())
                    {
                        item.CreatedOn = items.FirstOrDefault().CreatedOn;
                        if(items.FirstOrDefault().StartedOn != null)
                        {
                            item.StartedOn = items.FirstOrDefault().StartedOn;
                        }
                        else
                        {
                            if (item.StartedOn != null && UseServerDates)
                            {
                                //this has just been started. Must have been planned before. Replace user's date
                                item.StartedOn = DateTime.Now;
                            }
                        }
                        string descr = "Edycja zgłoszenia";
                        if((bool)items.FirstOrDefault().IsActive && (bool)item.IsFrozen)
                        {
                            //was active and it no longer is. It has been paused
                            item.LastStatus = (int)ProcessStatus.Paused;
                        }
                        else if((bool)items.FirstOrDefault().IsFrozen && (bool)item.IsActive)
                        {
                            //was paused and now it is active - it's been resumed
                            item.LastStatus = (int)ProcessStatus.Resumed;
                        }else if (!(bool)items.FirstOrDefault().IsActive && (bool)item.IsActive)
                        {
                            //wasn't active and now it is - it's been started
                            item.LastStatus = (int)ProcessStatus.Started;
                        }else if(!(bool)items.FirstOrDefault().IsCompleted && (bool)item.IsCompleted)
                        {
                            //it's been finished
                            item.LastStatus = (int)ProcessStatus.Finished;
                        }
                        if (items.FirstOrDefault().FinishedOn==null && item.FinishedOn != null)
                        {
                            //this has just been finished. Replace user's finish time with server time
                            CompleteProcessesHandlings(item.ProcessId, UserId);
                            if (UseServerDates)
                            {
                                item.FinishedOn = DateTime.Now;
                            }
                            descr = "Zamknięcie zgłoszenia";
                        }
                        item.LastStatusBy = UserId;
                        item.LastStatusOn = DateTime.Now;
                        JDE_Logs Log = new JDE_Logs { UserId = UserId, Description =descr, TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(items.FirstOrDefault()), NewValue = new JavaScriptSerializer().Serialize(item) };
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
            }

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
        [Route("CompleteProcess")]
        public IHttpActionResult CompleteProcess(string token, int id, int UserId)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = db.JDE_Processes.Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.ProcessId == id);
                    if (items.Any())
                    {
                        var item = items.FirstOrDefault();
                        string OldValue = new JavaScriptSerializer().Serialize(item);
                        item.FinishedOn = DateTime.Now;
                        item.FinishedBy = UserId;
                        item.IsActive = false;
                        item.IsCompleted = true;
                        item.IsFrozen = false;
                        item.LastStatus = (int)ProcessStatus.Finished;
                        item.LastStatusBy = UserId;
                        item.LastStatusOn = DateTime.Now;
                        CompleteProcessesHandlings(item.ProcessId, UserId);
                        JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Zamknięcie zgłoszenia", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, OldValue = OldValue, NewValue=new JavaScriptSerializer().Serialize(item)};
                        db.JDE_Logs.Add(Log);
                        db.Entry(item).State = EntityState.Modified;
                        db.SaveChanges();

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

        public void CompleteProcessesHandlings(int ProcessId, int UserId)
        {
            //it completes all open handlings for given process
            string descr = string.Empty;
            var items = db.JDE_Handlings.AsNoTracking().Where(p => p.ProcessId == ProcessId && p.IsCompleted==false);
            var User = db.JDE_Users.AsNoTracking().Where(u => u.UserId == UserId).FirstOrDefault();

            if (items.Any())
            {
                foreach (var item in items)
                {
                    item.FinishedOn = DateTime.Now;
                    item.IsActive = false;
                    item.IsFrozen = false;
                    item.IsCompleted = true;
                    item.Output = $"Obsługa została zakończona przy zamykaniu zgłoszenia przez {User.Name + " " + User.Surname}";
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

                }
            }
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

    public class Process
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
        public int? Length { get
            {
                if(StartedOn == null)
                {
                    return null;
                }
                else
                {
                    if(FinishedOn == null)
                    {
                        return (int)DateTime.Now.Subtract((DateTime)StartedOn).TotalMinutes;
                    }
                    else
                    {
                        return (int)((DateTime)FinishedOn).Subtract((DateTime)StartedOn).TotalMinutes;
                    }
                }
            } }
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