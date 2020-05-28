using JDE_API.Models;
using JDE_API.Static;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Script.Serialization;

namespace JDE_API.Controllers
{
    public class ComponentController : ApiController
    {
        private Models.DbModel db = new Models.DbModel();

        [HttpGet]
        [Route("GetComponents")]
        public IHttpActionResult GetComponents(string token, int page = 0, int pageSize = 0, int total = 0, string query = null)
        {

            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = (from c in db.JDE_Components
                                 join pl in db.JDE_Places on c.PlaceId equals pl.PlaceId into places
                                 from pls in places.DefaultIfEmpty()
                                 join u in db.JDE_Users on c.CreatedBy equals u.UserId
                                 join u2 in db.JDE_Users on c.LmBy equals u2.UserId into modifiedBy
                                 from mb in modifiedBy.DefaultIfEmpty()
                                 join t in db.JDE_Tenants on c.TenantId equals t.TenantId
                                 where c.TenantId == tenants.FirstOrDefault().TenantId
                                 orderby c.CreatedOn descending
                                 select new
                                 {
                                     ComponentId = c.ComponentId,
                                     Name = c.Name,
                                     Description = c.Description,
                                     PlaceId = c.PlaceId,
                                     PlaceName = pls.Name,
                                     CreatedOn = c.CreatedOn,
                                     CreatedBy = c.CreatedBy,
                                     CreatedByName = u.Name + " " + u.Surname,
                                     LmOn = c.LmOn,
                                     LmBy = c.LmBy,
                                     LmByName = mb.Name + " " + mb.Surname,
                                     TenantId = c.TenantId,
                                     TenantName = t.TenantName,
                                     IsArchived = c.IsArchived
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
        [Route("GetComponent")]
        [ResponseType(typeof(JDE_Components))]
        public IHttpActionResult GetComponent(string token, int id)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = (from c in db.JDE_Components
                                 join pl in db.JDE_Places on c.PlaceId equals pl.PlaceId into places
                                 from pls in places.DefaultIfEmpty()
                                 join u in db.JDE_Users on c.CreatedBy equals u.UserId
                                 join u2 in db.JDE_Users on c.LmBy equals u2.UserId into modifiedBy
                                 from mb in modifiedBy.DefaultIfEmpty()
                                 join t in db.JDE_Tenants on c.TenantId equals t.TenantId
                                 where c.TenantId == tenants.FirstOrDefault().TenantId && c.ComponentId==id
                                 orderby c.CreatedOn descending
                                 select new
                                 {
                                     ComponentId = c.ComponentId,
                                     Name = c.Name,
                                     Description = c.Description,
                                     PlaceId = c.PlaceId,
                                     PlaceName = pls.Name,
                                     CreatedOn = c.CreatedOn,
                                     CreatedBy = c.CreatedBy,
                                     CreatedByName = u.Name + " " + u.Surname,
                                     LmOn = c.LmOn,
                                     LmBy = c.LmBy,
                                     LmByName = mb.Name + " " + mb.Surname,
                                     TenantId = c.TenantId,
                                     TenantName = t.TenantName,
                                     IsArchived = c.IsArchived
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
        [Route("ArchiveComponent")]
        [ResponseType(typeof(void))]
        public HttpResponseMessage ArchiveComponent(string token, int id, int UserId)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = db.JDE_Components.AsNoTracking().Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.ComponentId == id);
                    if (items.Any())
                    {
                        JDE_Components orgItem = items.FirstOrDefault();

                        orgItem.IsArchived = true;
                        JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Archiwizacja komponentu", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(items.FirstOrDefault()), NewValue = "" };
                        db.JDE_Logs.Add(Log);

                        try
                        {
                            db.Entry(orgItem).State = EntityState.Modified;
                            db.SaveChanges();

                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            if (!JDE_ComponentExists(id))
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound);
                            }
                            else
                            {
                                throw;
                            }
                        }
                        catch (Exception ex)
                        {
                            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
                        }
                    }
                }
            }
            return Request.CreateResponse(HttpStatusCode.NoContent);
        }

        [HttpPut]
        [Route("EditComponent")]
        [ResponseType(typeof(void))]

        public IHttpActionResult EditComponent(string token, int id, int UserId, JDE_Components item)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = db.JDE_Components.AsNoTracking().Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.ComponentId == id);
                    if (items.Any())
                    {

                        JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Edycja komponentu", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(items.FirstOrDefault()), NewValue = new JavaScriptSerializer().Serialize(item) };
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
                            if (!JDE_ComponentExists(id))
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
        [Route("CreateComponent")]
        [ResponseType(typeof(JDE_Components))]
        public IHttpActionResult CreateComponent(string token, JDE_Components item, int UserId)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    item.TenantId = tenants.FirstOrDefault().TenantId;
                    item.CreatedOn = DateTime.Now;
                    db.JDE_Components.Add(item);
                    db.SaveChanges();
                    JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Utworzenie komponentu", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, NewValue = new JavaScriptSerializer().Serialize(item) };
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
        [Route("DeleteComponent")]
        [ResponseType(typeof(JDE_Components))]
        public IHttpActionResult DeleteComponent(string token, int id, int UserId)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = db.JDE_Components.Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.ComponentId == id);
                    if (items.Any())
                    {
                        JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Usunięcie komponentu", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(items.FirstOrDefault()) };
                        db.JDE_Components.Remove(items.FirstOrDefault());
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

        private bool JDE_ComponentExists(int id)
        {
            return db.JDE_Components.Count(e => e.ComponentId == id) > 0;
        }
    }
}

