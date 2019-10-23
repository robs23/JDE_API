using JDE_API.Models;
using JDE_API.Static;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Script.Serialization;

namespace JDE_API.Controllers
{
    public class ProcessAssignController : ApiController
    {
        private Models.DbModel db = new Models.DbModel();

        [HttpGet]
        [Route("GetProcessAssigns")]
        public IHttpActionResult GetProcessAssigns(string token, int page = 0, int total = 0, string query = null)
        {

            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {

                    var items = (from pa in db.JDE_ProcessAssigns
                                 join p in db.JDE_Processes on pa.ProcessId equals p.ProcessId into Processes
                                 from prs in Processes.DefaultIfEmpty()
                                 join u in db.JDE_Users on pa.CreatedBy equals u.UserId
                                 join u2 in db.JDE_Users on pa.LmBy equals u2.UserId into LmByNames
                                 from lms in LmByNames.DefaultIfEmpty()
                                 join u3 in db.JDE_Users on pa.UserId equals u3.UserId into Users
                                 from usrs in Users.DefaultIfEmpty()
                                 join t in db.JDE_Tenants on pa.TenantId equals t.TenantId
                                 where pa.TenantId == tenants.FirstOrDefault().TenantId
                                 orderby pa.CreatedOn descending
                                 select new
                                 {
                                     ProcessAssignId = pa.ProcessAssignId,
                                     ProcessId = pa.ProcessId,
                                     UserId = usrs.UserId,
                                     UserName = usrs.Name + " " + usrs.Surname,
                                     CreatedBy = u.UserId,
                                     CreatedByName = u.Name + " " + u.Surname,
                                     CreatedOn = pa.CreatedOn,
                                     LmBy = pa.LmBy,
                                     LmByName = lms.Name + " " + lms.Surname,
                                     TenantId = pa.TenantId,
                                     TenantName = t.TenantName
                                 });
                    if (items.Any())
                    {
                        if (query != null)
                        {

                            items = items.Where(query);

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
        [Route("GetProcessAssign")]
        [ResponseType(typeof(JDE_ProcessAssigns))]
        public IHttpActionResult GetProcessAssign(string token, int id)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = (from pa in db.JDE_ProcessAssigns
                                 join p in db.JDE_Processes on pa.ProcessId equals p.ProcessId into Processes
                                 from prs in Processes.DefaultIfEmpty()
                                 join u in db.JDE_Users on pa.CreatedBy equals u.UserId
                                 join u2 in db.JDE_Users on pa.LmBy equals u2.UserId into LmByNames
                                 from lms in LmByNames.DefaultIfEmpty()
                                 join u3 in db.JDE_Users on pa.UserId equals u3.UserId into Users
                                 from usrs in Users.DefaultIfEmpty()
                                 join t in db.JDE_Tenants on pa.TenantId equals t.TenantId
                                 where pa.TenantId == tenants.FirstOrDefault().TenantId && pa.ProcessAssignId==id && pa.ProcessAssignId==id
                                 orderby pa.CreatedOn descending
                                 select new
                                 {
                                     ProcessAssignId = pa.ProcessAssignId,
                                     ProcessId = pa.ProcessId,
                                     UserId = usrs.UserId,
                                     UserName = usrs.Name + " " + usrs.Surname,
                                     CreatedBy = u.UserId,
                                     CreatedByName = u.Name + " " + u.Surname,
                                     CreatedOn = pa.CreatedOn,
                                     LmBy = pa.LmBy,
                                     LmByName = lms.Name + " " + lms.Surname,
                                     TenantId = pa.TenantId,
                                     TenantName = t.TenantName
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
        [Route("EditProcessAssign")]
        [ResponseType(typeof(void))]

        public IHttpActionResult EditProcessAssign(string token, int id, int UserId, JDE_ProcessAssigns item)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = db.JDE_ProcessAssigns.AsNoTracking().Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.ProcessAssignId == id);
                    if (items.Any())
                    {
                        string descr = "Edycja przypisania zgłoszenia do użytkownika";
                        item.LmOn = DateTime.Now;
                        JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = descr, TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(items.FirstOrDefault()), NewValue = new JavaScriptSerializer().Serialize(item) };
                        db.JDE_Logs.Add(Log);
                        db.Entry(item).State = EntityState.Modified;
                        try
                        {
                            db.SaveChanges();
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            if (!JDE_ProcessAssignsExists(id))
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
        [Route("CreateProcessAssign")]
        [ResponseType(typeof(JDE_ProcessAssigns))]
        public IHttpActionResult CreateProcessAssign(string token, JDE_ProcessAssigns item, int UserId)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    item.TenantId = tenants.FirstOrDefault().TenantId;
                    item.CreatedOn = DateTime.Now;
                    db.JDE_ProcessAssigns.Add(item);
                    db.SaveChanges();
                    JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Utworzenie przypisania zgłoszenia do użytkownika", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, NewValue = new JavaScriptSerializer().Serialize(item) };
                    db.JDE_Logs.Add(Log);
                    db.SaveChanges();
                    return Ok(item);
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
        [Route("DeleteProcessAssign")]
        [ResponseType(typeof(JDE_ProcessAssigns))]
        public IHttpActionResult DeleteProcessAssign(string token, int id, int UserId)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = db.JDE_ProcessAssigns.Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.ProcessAssignId == id);
                    if (items.Any())
                    {
                        JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Usunięcie przypisania zgłoszenia do użytkownika", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(items.FirstOrDefault()) };
                        db.JDE_ProcessAssigns.Remove(items.FirstOrDefault());
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

        private bool JDE_ProcessAssignsExists(int id)
        {
            return db.JDE_ProcessAssigns.Count(e => e.ProcessAssignId == id) > 0;
        }
    }
}
