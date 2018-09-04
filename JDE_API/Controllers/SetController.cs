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
    public class SetController : ApiController
    {
        private Models.DbModel db = new Models.DbModel();

        [HttpGet]
        [Route("GetSets")]
        public IHttpActionResult GetSets(string token, int page=0, int total=0)
        {

            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = (from s in db.JDE_Sets
                                 join t in db.JDE_Tenants on s.TenantId equals t.TenantId
                                 join u in db.JDE_Users on s.CreatedBy equals u.UserId
                                 where s.TenantId == tenants.FirstOrDefault().TenantId
                                 orderby s.CreatedOn descending
                                 select new
                                 {
                                     SetId = s.SetId,
                                     Number = s.Number,
                                     Description = s.Description,
                                     Name = s.Name,
                                     TenantId = s.TenantId,
                                     TenantName = t.TenantName,
                                     CreatedOn = s.CreatedOn,
                                     CreatedBy = s.CreatedBy,
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
        [Route("GetSet")]
        [ResponseType(typeof(JDE_Sets))]
        public IHttpActionResult GetSet(string token, int id)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = (from s in db.JDE_Sets
                                 join t in db.JDE_Tenants on s.TenantId equals t.TenantId
                                 join u in db.JDE_Users on s.CreatedBy equals u.UserId
                                 where s.TenantId == tenants.FirstOrDefault().TenantId && s.SetId == id
                                 select new
                                 {
                                     SetId = s.SetId,
                                     Number = s.Number,
                                     Description = s.Description,
                                     Name = s.Name,
                                     TenantId = s.TenantId,
                                     TenantName = t.TenantName,
                                     CreatedOn = s.CreatedOn,
                                     CreatedBy = s.CreatedBy,
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
        [Route("EditSet")]
        [ResponseType(typeof(void))]

        public IHttpActionResult EditSet(string token, int id, JDE_Sets item, int UserId)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = db.JDE_Sets.Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.SetId == id);
                    if (items.Any())
                    {
                        JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Edycja instalacji", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(items.FirstOrDefault()), NewValue = new JavaScriptSerializer().Serialize(item) };
                        db.JDE_Logs.Add(Log);
                        db.Entry(item).State = EntityState.Modified;
                        try
                        {
                            db.SaveChanges();
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            if (!JDE_SetsExists(id))
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
        [Route("CreateSet")]
        [ResponseType(typeof(JDE_Sets))]
        public IHttpActionResult CreateSet(string token, JDE_Sets item, int UserId)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    item.TenantId = tenants.FirstOrDefault().TenantId;
                    db.JDE_Sets.Add(item);
                    db.SaveChanges();
                    JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Utworzenie instalacji", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, NewValue = new JavaScriptSerializer().Serialize(item) };
                    db.JDE_Logs.Add(Log);
                    db.SaveChanges();
                    return Ok(item);
                    //return CreatedAtRoute("JDEApi", new {token = token, id = item.SetId }, item);
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
        [Route("DeleteSet")]
        [ResponseType(typeof(JDE_Sets))]
        public IHttpActionResult DeleteSet(string token, int id, int UserId)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = db.JDE_Sets.Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.SetId == id);
                    if (items.Any())
                    {
                        JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Usunięcie instalacji", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(items.FirstOrDefault()) };
                        db.JDE_Sets.Remove(items.FirstOrDefault());
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

        private bool JDE_SetsExists(int id)
        {
            return db.JDE_Sets.Count(e => e.SetId == id) > 0;
        }
    }
}