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
    public class AreaController : ApiController
    {
        private Models.DbModel db = new Models.DbModel();

        [HttpGet]
        [Route("GetAreas")]
        public IHttpActionResult GetAreas(string token, int page=0, int total=0)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = (from a in db.JDE_Areas
                                 join t in db.JDE_Tenants on a.TenantId equals t.TenantId
                                 join u in db.JDE_Users on a.CreatedBy equals u.UserId
                                 where a.TenantId == tenants.FirstOrDefault().TenantId
                                 orderby a.CreatedOn descending
                                 select new
                                 {
                                     AreaId = a.AreaId,
                                     Description = a.Description,
                                     Name = a.Name,
                                     TenantId = a.TenantId,
                                     TenantName = t.TenantName,
                                     CreatedOn = a.CreatedOn,
                                     CreatedBy = a.CreatedBy,
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
        [Route("GetArea")]
        [ResponseType(typeof(JDE_Areas))]
        public IHttpActionResult GetArea(string token, int id)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = (from a in db.JDE_Areas
                                 join t in db.JDE_Tenants on a.TenantId equals t.TenantId
                                 join u in db.JDE_Users on a.CreatedBy equals u.UserId
                                 where a.TenantId == tenants.FirstOrDefault().TenantId && a.AreaId == id
                                 select new
                                 {
                                     AreaId = a.AreaId,
                                     Description = a.Description,
                                     Name = a.Name,
                                     TenantId = a.TenantId,
                                     TenantName = t.TenantName,
                                     CreatedOn = a.CreatedOn,
                                     CreatedBy = a.CreatedBy,
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
        [Route("EditArea")]
        [ResponseType(typeof(void))]

        public IHttpActionResult EditArea(string token, int id, JDE_Areas item, int UserId)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = db.JDE_Areas.AsNoTracking().Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.AreaId == id);
                    if (items.Any())
                    {
                        JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Edycja obszaru", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(items.FirstOrDefault()), NewValue = new JavaScriptSerializer().Serialize(item) };
                        db.JDE_Logs.Add(Log);
                        db.Entry(item).State = EntityState.Modified;
                        try
                        {
                            db.SaveChanges();
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            if (!JDE_AreasExists(id))
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
        [Route("CreateArea")]
        [ResponseType(typeof(JDE_Areas))]
        public IHttpActionResult CreateArea(string token, JDE_Areas item, int UserId)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    item.TenantId = tenants.FirstOrDefault().TenantId;
                    db.JDE_Areas.Add(item);
                    db.SaveChanges();
                    JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Utworzenie obszaru", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, NewValue = new JavaScriptSerializer().Serialize(item) };
                    db.JDE_Logs.Add(Log);
                    db.SaveChanges();
                    return Ok(item);
                    //return CreatedAtRoute("DefaultApi", new { id = item.AreaId }, item);
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
        [Route("DeleteArea")]
        [ResponseType(typeof(JDE_Areas))]
        public IHttpActionResult DeleteArea(string token, int id, int UserId)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = db.JDE_Areas.Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.AreaId == id);
                    if (items.Any())
                    {
                        JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Usunięcie obszaru", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(items.FirstOrDefault()) };
                        db.JDE_Areas.Remove(items.FirstOrDefault());
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

        private bool JDE_AreasExists(int id)
        {
            return db.JDE_Areas.Count(e => e.AreaId == id) > 0;
        }
    }
}