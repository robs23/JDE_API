using JDE_API.Static;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Web;
using System.Web.Http;
using System.Net.Http;
using System.Web.Http.Description;
using JDE_API.Models;
using System.Net;
using System.Web.Script.Serialization;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace JDE_API.Controllers
{
    public class UserLogController : ApiController
    {
        private Models.DbModel db = new Models.DbModel();

        [HttpGet]
        [Route("GetUserLogs")]
        public IHttpActionResult GetUserLogs(string token, int page = 0, int pageSize = 0, int total = 0, string query = null)
        {

            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = (from ul in db.JDE_UserLogs
                                 join u in db.JDE_Users on ul.CreatedBy equals u.UserId
                                 join u2 in db.JDE_Users on ul.LmBy equals u2.UserId into modifiedBy
                                 from mb in modifiedBy.DefaultIfEmpty()
                                 join t in db.JDE_Tenants on ul.TenantId equals t.TenantId
                                 where ul.TenantId == tenants.FirstOrDefault().TenantId
                                 orderby ul.CreatedOn descending
                                 select new
                                 {
                                     UserLogId = ul.UserLogId,
                                     Platform = ul.Platform,
                                     Device = ul.Device,
                                     LogName = ul.LogName,
                                     HasTheAppCrashed = ul.HasTheAppCrashed,
                                     OnRequest = ul.OnRequest,
                                     Message = ul.Message,
                                     StackTrace =  ul.StackTrace,
                                     ErrorTime = ul.ErrorTime,
                                     Comment = ul.Comment,
                                     CreatedOn = ul.CreatedOn,
                                     CreatedBy = ul.CreatedBy,
                                     CreatedByName = u.Name + " " + u.Surname,
                                     LmOn = ul.LmOn,
                                     LmBy = ul.LmBy,
                                     LmByName = mb.Name + " " + mb.Surname,
                                     TenantId = ul.TenantId,
                                     TenantName = t.TenantName,
                                 });
                    if (items.Any())
                    {
                        if (query != null)
                        {
                            items = items.Where(query);
                        }

                        if (total == 0 && page > 0)
                        {
                            if (pageSize == 0)
                            {
                                pageSize = RuntimeSettings.PageSize;
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
        [Route("GetUserLog")]
        [ResponseType(typeof(JDE_UserLogs))]
        public IHttpActionResult GetUserLog(string token, int id)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = (from ul in db.JDE_UserLogs
                                 join u in db.JDE_Users on ul.CreatedBy equals u.UserId
                                 join u2 in db.JDE_Users on ul.LmBy equals u2.UserId into modifiedBy
                                 from mb in modifiedBy.DefaultIfEmpty()
                                 join t in db.JDE_Tenants on ul.TenantId equals t.TenantId
                                 where ul.TenantId == tenants.FirstOrDefault().TenantId && ul.UserLogId == id
                                 orderby ul.CreatedOn descending
                                 select new
                                 {
                                     UserLogId = ul.UserLogId,
                                     Platform = ul.Platform,
                                     Device = ul.Device,
                                     LogName = ul.LogName,
                                     HasTheAppCrashed = ul.HasTheAppCrashed,
                                     OnRequest = ul.OnRequest,
                                     Message = ul.Message,
                                     StackTrace = ul.StackTrace,
                                     ErrorTime = ul.ErrorTime,
                                     Comment = ul.Comment,
                                     CreatedOn = ul.CreatedOn,
                                     CreatedBy = ul.CreatedBy,
                                     CreatedByName = u.Name + " " + u.Surname,
                                     LmOn = ul.LmOn,
                                     LmBy = ul.LmBy,
                                     LmByName = mb.Name + " " + mb.Surname,
                                     TenantId = ul.TenantId,
                                     TenantName = t.TenantName,
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
        [Route("EditUserLog")]
        [ResponseType(typeof(void))]

        public IHttpActionResult EditUserLog(string token, int id, int UserId, JDE_AbandonReasons item)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = db.JDE_UserLogs.AsNoTracking().Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.UserLogId == id);
                    if (items.Any())
                    {

                        JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Edycja logu użytkownika", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(items.FirstOrDefault()), NewValue = new JavaScriptSerializer().Serialize(item) };
                        db.JDE_Logs.Add(Log);
                        item.LmBy = UserId;
                        item.LmOn = DateTime.Now;

                        db.Entry(item).State = EntityState.Modified;
                        try
                        {
                            db.SaveChanges();
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            if (!JDE_UserLogExists(id))
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
        [Route("CreateUserLog")]
        [ResponseType(typeof(JDE_UserLogs))]
        public IHttpActionResult CreateUserLog(string token, JDE_UserLogs item, int UserId)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    item.TenantId = tenants.FirstOrDefault().TenantId;
                    item.CreatedOn = DateTime.Now;
                    db.JDE_UserLogs.Add(item);
                    db.SaveChanges();
                    JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Utworzenie logu użytkownika", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, NewValue = new JavaScriptSerializer().Serialize(item) };
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
        [Route("DeleteUserLog")]
        [ResponseType(typeof(JDE_UserLogs))]
        public IHttpActionResult DeleteUserLog(string token, int id, int UserId)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = db.JDE_UserLogs.Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.UserLogId == id);
                    if (items.Any())
                    {
                        JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Usunięcie logu użytkownika", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(items.FirstOrDefault()) };
                        db.JDE_UserLogs.Remove(items.FirstOrDefault());
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

        private bool JDE_UserLogExists(int id)
        {
            return db.JDE_UserLogs.Count(e => e.UserLogId == id) > 0;
        }
    }
}