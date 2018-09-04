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

namespace JDE_API.Controllers
{
    public class ErrorController : ApiController
    {
        private Models.DbModel db = new Models.DbModel();

        [HttpGet]
        [Route("GetErrors")]
        public IHttpActionResult GetErrors(string token)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = (from e in db.JDE_Errors
                                 join u in db.JDE_Users on e.UserId equals u.UserId
                                 join t in db.JDE_Tenants on e.TenantId equals t.TenantId
                                 where e.TenantId == tenants.FirstOrDefault().TenantId
                                 select new
                                 {
                                    ErrorId = e.ErrorId,
                                    TenantId = t.TenantId,
                                    TenantName = t.TenantName,
                                    UserId = e.UserId,
                                    UserName = u.Name + " " + u.Surname,
                                    Time = e.Time,
                                    App = e.App,
                                    Class = e.Class,
                                    Method = e.Method,
                                    Message = e.Message
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

        [HttpGet]
        [Route("GetError")]
        public IHttpActionResult GetError(string token, int id)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = (from e in db.JDE_Errors
                                 join u in db.JDE_Users on e.UserId equals u.UserId
                                 join t in db.JDE_Tenants on e.TenantId equals t.TenantId
                                 where e.TenantId == tenants.FirstOrDefault().TenantId && e.ErrorId==id
                                 select new
                                 {
                                     ErrorId = e.ErrorId,
                                     TenantId = t.TenantId,
                                     TenantName = t.TenantName,
                                     UserId = e.UserId,
                                     UserName = u.Name + " " + u.Surname,
                                     Time = e.Time,
                                     App = e.App,
                                     Class = e.Class,
                                     Method = e.Method,
                                     Message = e.Message
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
        [Route("CreateError")]
        [ResponseType(typeof(JDE_Errors))]
        public IHttpActionResult CreateError(string token, JDE_Errors item)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    item.TenantId = tenants.FirstOrDefault().TenantId;
                    db.JDE_Errors.Add(item);
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
        [Route("DeleteError")]
        [ResponseType(typeof(JDE_Errors))]
        public IHttpActionResult DeleteError(string token, int id)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = db.JDE_Errors.Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.ErrorId == id);
                    if (items.Any())
                    {
                        db.JDE_Errors.Remove(items.FirstOrDefault());
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

        private bool JDE_ErrorsExists(int id)
        {
            return db.JDE_Errors.Count(e => e.ErrorId == id) > 0;
        }
    }
}