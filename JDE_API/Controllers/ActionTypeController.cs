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
    public class ActionTypeController : ApiController
    {
        private Models.DbModel db = new Models.DbModel();

        [HttpGet]
        [Route("GetActionTypes")]
        public IHttpActionResult GetActionTypes(string token, int page=0, int total = 0)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = (from at in db.JDE_ActionTypes
                                 join t in db.JDE_Tenants on at.TenantId equals t.TenantId
                                 join u in db.JDE_Users on at.CreatedBy equals u.UserId
                                 where at.TenantId == tenants.FirstOrDefault().TenantId
                                 orderby at.CreatedOn descending
                                 select new
                                 {
                                     ActionTypeId = at.ActionTypeId,
                                     Name = at.Name,
                                     Description = at.Description,
                                     TenantId = at.TenantId,
                                     TenantName = t.TenantName,
                                     CreatedOn = at.CreatedOn,
                                     CreatedBy = at.CreatedBy,
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
        [Route("GetActionType")]
        [ResponseType(typeof(JDE_ActionTypes))]
        public IHttpActionResult GetActionType(string token, int id)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = (from at in db.JDE_ActionTypes
                                 join t in db.JDE_Tenants on at.TenantId equals t.TenantId
                                 join u in db.JDE_Users on at.CreatedBy equals u.UserId
                                 where at.TenantId == tenants.FirstOrDefault().TenantId && at.ActionTypeId == id
                                 select new
                                 {
                                     ActionTypeId = at.ActionTypeId,
                                     Name = at.Name,
                                     Description = at.Description,
                                     TenantId = at.TenantId,
                                     TenantName = t.TenantName,
                                     CreatedOn = at.CreatedOn,
                                     CreatedBy = at.CreatedBy,
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
        [Route("EditActionType")]
        [ResponseType(typeof(void))]

        public IHttpActionResult EditActionType(string token, int id, JDE_ActionTypes item, int UserId)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = db.JDE_ActionTypes.AsNoTracking().Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.ActionTypeId == id);
                    if (items.Any())
                    {
                        JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Edycja typu zgłoszenia", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(items.FirstOrDefault()), NewValue = new JavaScriptSerializer().Serialize(item) };
                        db.JDE_Logs.Add(Log);
                        db.Entry(item).State = EntityState.Modified;
                        try
                        {
                            db.SaveChanges();
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            if (!JDE_ActionTypesExists(id))
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
        [Route("CreateActionType")]
        [ResponseType(typeof(JDE_ActionTypes))]
        public IHttpActionResult CreateActionType(string token, JDE_ActionTypes item, int UserId)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    item.TenantId = tenants.FirstOrDefault().TenantId;
                    db.JDE_ActionTypes.Add(item);
                    db.SaveChanges();
                    JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Utworzenie typu złoszenia", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, NewValue = new JavaScriptSerializer().Serialize(item) };
                    db.JDE_Logs.Add(Log);
                    db.SaveChanges();
                    return Ok(item);
                    //return CreatedAtRoute("DefaultApi", new { id = item.ActionTypeId }, item);
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
        [Route("DeleteActionType")]
        [ResponseType(typeof(JDE_ActionTypes))]
        public IHttpActionResult DeleteActionType(string token, int id, int UserId)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = db.JDE_ActionTypes.Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.ActionTypeId == id);
                    if (items.Any())
                    {
                        JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Usunięcie typu zgłoszenia", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(items.FirstOrDefault()) };
                        db.JDE_ActionTypes.Remove(items.FirstOrDefault());
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

        private bool JDE_ActionTypesExists(int id)
        {
            return db.JDE_ActionTypes.Count(e => e.ActionTypeId == id) > 0;
        }
    }
}