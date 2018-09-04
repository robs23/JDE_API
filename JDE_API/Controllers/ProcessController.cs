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

namespace JDE_API.Controllers
{
    public class ProcessController : ApiController
    {
        private Models.DbModel db = new Models.DbModel();

        [HttpGet]
        [Route("GetProcesses")]
        public IHttpActionResult GetProcesses(string token, int page=0, int total=0)
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
                                 join pl in db.JDE_Places on p.PlaceId equals pl.PlaceId
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
                                     Output = p.Output,
                                     TenantId = p.TenantId,
                                     TenantName = t.TenantName,
                                     CreatedOn = p.CreatedOn,
                                     CreatedBy = p.CreatedBy,
                                     CreatedByName = u.Name + " " + u.Surname
                                 });
                    if (items.Any())
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
                                     CreatedByName = u.Name + " " + u.Surname
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
                                     CreatedByName = u.Name + " " + u.Surname
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
                                 join pl in db.JDE_Places on p.PlaceId equals pl.PlaceId
                                 where p.TenantId == tenants.FirstOrDefault().TenantId
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
                                     CreatedByName = u.Name + " " + u.Surname
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

        public IHttpActionResult EditProcess(string token, int id, int UserId, JDE_Processes item)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = db.JDE_Processes.AsNoTracking().Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.ProcessId == id);
                    if (items.Any())
                    {
                        JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Edycja zgłoszenia", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(items.FirstOrDefault()), NewValue = new JavaScriptSerializer().Serialize(item) };
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
}