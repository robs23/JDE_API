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
using JDE_API.Models;
using JDE_API.Static;

namespace JDE_API.Controllers
{
    public class LogController : ApiController
    {
        private Models.DbModel db = new Models.DbModel();

        [HttpGet]
        [Route("GetLogs")]
        public IHttpActionResult GetLogs(string token, int page=0, int total=0)
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
                                 select new
                                 {
                                     LogId = l.LogId,
                                     Timestamp = l.Timestamp,
                                     TenantId = t.TenantId,
                                     TenantName = t.TenantName,
                                     UserId = l.UserId,
                                     UserName = u.Name + " " + u.Surname,
                                     Descripiton = l.Description,
                                     OldValue = l.OldValue,
                                     NewValue = l.NewValue
                                 });
                    if (items.Any())
                    {

                        if(total==0 && page > 0)
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
                        }else if(total>0 && page == 0)
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
        [Route("GetLog")]
        public IHttpActionResult GetLog(string token, int id)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = (from l in db.JDE_Logs
                                 join u in db.JDE_Users on l.UserId equals u.UserId
                                 join t in db.JDE_Tenants on l.TenantId equals t.TenantId
                                 where l.TenantId == tenants.FirstOrDefault().TenantId && l.LogId==id
                                 select new
                                 {
                                     LogId = l.LogId,
                                     Timestamp = l.Timestamp,
                                     TenantId = t.TenantId,
                                     TenantName = t.TenantName,
                                     UserId = l.UserId,
                                     UserName = u.Name + " " + u.Surname,
                                     Descripiton = l.Description,
                                     OldValue = l.OldValue,
                                     NewValue = l.NewValue
                                 });

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

        [HttpPost]
        [Route("CreateLog")]
        [ResponseType(typeof(JDE_Logs))]
        public IHttpActionResult CreateLog(string token, JDE_Logs item)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    item.TenantId = tenants.FirstOrDefault().TenantId;
                    db.JDE_Logs.Add(item);
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
        [Route("DeleteLog")]
        [ResponseType(typeof(JDE_Logs))]
        public IHttpActionResult DeleteLog(string token, int id)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = db.JDE_Logs.Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.LogId == id);
                    if (items.Any())
                    {
                        db.JDE_Logs.Remove(items.FirstOrDefault());
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

        private bool JDE_LogsExists(int id)
        {
            return db.JDE_Logs.Count(e => e.LogId == id) > 0;
        }
    }
}