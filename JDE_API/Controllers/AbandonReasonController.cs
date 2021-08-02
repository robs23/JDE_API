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
    public class AbandonReasonController : ApiController
    {
        private Models.DbModel db = new Models.DbModel();

        [HttpGet]
        [Route("GetAbandonReasons")]
        public IHttpActionResult GetAbandonReasons(string token, int page = 0, int pageSize = 0, int total = 0, string query = null)
        {

            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = (from ar in db.JDE_AbandonReasons
                                 join u in db.JDE_Users on ar.CreatedBy equals u.UserId
                                 join u2 in db.JDE_Users on ar.LmBy equals u2.UserId into modifiedBy
                                 from mb in modifiedBy.DefaultIfEmpty()
                                 join t in db.JDE_Tenants on ar.TenantId equals t.TenantId
                                 where ar.TenantId == tenants.FirstOrDefault().TenantId
                                 orderby ar.CreatedOn descending
                                 select new
                                 {
                                     AbandonReasonId = ar.AbandonReasonId,
                                     Name = ar.Name,
                                     CreatedOn = ar.CreatedOn,
                                     CreatedBy = ar.CreatedBy,
                                     CreatedByName = u.Name + " " + u.Surname,
                                     LmOn = ar.LmOn,
                                     LmBy = ar.LmBy,
                                     LmByName = mb.Name + " " + mb.Surname,
                                     TenantId = ar.TenantId,
                                     TenantName = t.TenantName,
                                     IsArchived = ar.IsArchived
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
        [Route("GetAbandonReason")]
        [ResponseType(typeof(JDE_AbandonReasons))]
        public IHttpActionResult GetAbandonReason(string token, int id)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = (from ar in db.JDE_AbandonReasons
                                 join u in db.JDE_Users on ar.CreatedBy equals u.UserId
                                 join u2 in db.JDE_Users on ar.LmBy equals u2.UserId into modifiedBy
                                 from mb in modifiedBy.DefaultIfEmpty()
                                 join t in db.JDE_Tenants on ar.TenantId equals t.TenantId
                                 where ar.TenantId == tenants.FirstOrDefault().TenantId && ar.AbandonReasonId == id
                                 orderby ar.CreatedOn descending
                                 select new
                                 {
                                     AbandonReasonId = ar.AbandonReasonId,
                                     Name = ar.Name,
                                     CreatedOn = ar.CreatedOn,
                                     CreatedBy = ar.CreatedBy,
                                     CreatedByName = u.Name + " " + u.Surname,
                                     LmOn = ar.LmOn,
                                     LmBy = ar.LmBy,
                                     LmByName = mb.Name + " " + mb.Surname,
                                     TenantId = ar.TenantId,
                                     TenantName = t.TenantName,
                                     IsArchived = ar.IsArchived
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
        [Route("ArchiveAbandonReason")]
        [ResponseType(typeof(void))]
        public HttpResponseMessage ArchiveAbandonReason(string token, int id, int UserId)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = db.JDE_AbandonReasons.AsNoTracking().Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.AbandonReasonId == id);
                    if (items.Any())
                    {
                        JDE_AbandonReasons orgItem = items.FirstOrDefault();

                        orgItem.IsArchived = true;
                        JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Archiwizacja powodu niewykonania", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(items.FirstOrDefault()), NewValue = "" };
                        db.JDE_Logs.Add(Log);

                        try
                        {
                            db.Entry(orgItem).State = EntityState.Modified;
                            db.SaveChanges();

                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            if (!JDE_AbandonReasonExists(id))
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
        [Route("EditAbandonReason")]
        [ResponseType(typeof(void))]

        public IHttpActionResult EditAbandonReason(string token, int id, int UserId, JDE_AbandonReasons item)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = db.JDE_AbandonReasons.AsNoTracking().Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.AbandonReasonId == id);
                    if (items.Any())
                    {

                        JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Edycja powodu niewykonania", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(items.FirstOrDefault()), NewValue = new JavaScriptSerializer().Serialize(item) };
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
                            if (!JDE_AbandonReasonExists(id))
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
        [Route("CreateAbandonReason")]
        [ResponseType(typeof(JDE_AbandonReasons))]
        public IHttpActionResult CreateComponent(string token, JDE_AbandonReasons item, int UserId)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    item.TenantId = tenants.FirstOrDefault().TenantId;
                    item.CreatedOn = DateTime.Now;
                    db.JDE_AbandonReasons.Add(item);
                    db.SaveChanges();
                    JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Utworzenie powodu niewykonania", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, NewValue = new JavaScriptSerializer().Serialize(item) };
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
        [Route("DeleteAbandonReason")]
        [ResponseType(typeof(JDE_AbandonReasons))]
        public IHttpActionResult DeleteAbandonReason(string token, int id, int UserId)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = db.JDE_AbandonReasons.Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.AbandonReasonId == id);
                    if (items.Any())
                    {
                        JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Usunięcie powodu niewykonania", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(items.FirstOrDefault()) };
                        db.JDE_AbandonReasons.Remove(items.FirstOrDefault());
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

        private bool JDE_AbandonReasonExists(int id)
        {
            return db.JDE_AbandonReasons.Count(e => e.AbandonReasonId == id) > 0;
        }
    }
}