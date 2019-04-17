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
    public class DeliveryController : ApiController
    {
        private Models.DbModel db = new Models.DbModel();

        [HttpGet]
        [Route("GetDeliveries")]
        public IHttpActionResult GetDeliveries(string token, int page = 0, int pageSize = 0, int total = 0, DateTime? dFrom = null, DateTime? dTo = null, string query = null)
        {

            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    if (dFrom == null)
                    {
                        if (db.JDE_Deliveries.Any())
                        {
                            dFrom = db.JDE_Deliveries.Min(x => x.CreatedOn).Value.AddDays(-1);
                        }
                        else
                        {
                            dFrom = new DateTime(2018, 1, 1);
                        }
                    }
                    if (dTo == null)
                    {
                        if (db.JDE_Deliveries.Any())
                        {
                            dTo = db.JDE_Deliveries.Max(x => x.CreatedOn).Value.AddDays(1);

                        }
                        else
                        {
                            dTo = new DateTime(2030, 12, 31);
                        }
                    }

                    var items = (from d in db.JDE_Deliveries
                                 join u in db.JDE_Users on d.CreatedBy equals u.UserId
                                 join u2 in db.JDE_Users on d.LmBy equals u2.UserId into modifiedBy
                                 from mb in modifiedBy.DefaultIfEmpty()
                                 join t in db.JDE_Tenants on d.TenantId equals t.TenantId
                                 where d.TenantId == tenants.FirstOrDefault().TenantId && d.CreatedOn >= dFrom && d.CreatedOn <= dTo
                                 orderby d.CreatedOn descending
                                 select new
                                 {
                                     DeliveryId = d.DeliveryId,
                                     DeliveredOn = d.DeliveredOn,
                                     OrderId = d.OrderId,
                                     CreatedOn = d.CreatedOn,
                                     CreatedBy = d.CreatedBy,
                                     CreatedByName = u.Name + " " + u.Surname,
                                     LmOn = d.LmOn,
                                     LmBy = d.LmBy,
                                     LmByName = mb.Name + " " + mb.Surname,
                                     TenantId = d.TenantId,
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
        [Route("GetDelivery")]
        [ResponseType(typeof(JDE_Deliveries))]
        public IHttpActionResult GetDelivery(string token, int id)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = (from d in db.JDE_Deliveries
                                 join u in db.JDE_Users on d.CreatedBy equals u.UserId
                                 join u2 in db.JDE_Users on d.LmBy equals u2.UserId into modifiedBy
                                 from mb in modifiedBy.DefaultIfEmpty()
                                 join t in db.JDE_Tenants on d.TenantId equals t.TenantId
                                 where d.TenantId == tenants.FirstOrDefault().TenantId && d.DeliveryId==id
                                 orderby d.CreatedOn descending
                                 select new
                                 {
                                     DeliveryId = d.DeliveryId,
                                     DeliveredOn = d.DeliveredOn,
                                     OrderId = d.OrderId,
                                     CreatedOn = d.CreatedOn,
                                     CreatedBy = d.CreatedBy,
                                     CreatedByName = u.Name + " " + u.Surname,
                                     LmOn = d.LmOn,
                                     LmBy = d.LmBy,
                                     LmByName = mb.Name + " " + mb.Surname,
                                     TenantId = d.TenantId,
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
        [Route("EditDelivery")]
        [ResponseType(typeof(void))]

        public IHttpActionResult EditDelivery(string token, int id, int UserId, JDE_Deliveries item)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = db.JDE_Deliveries.AsNoTracking().Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.DeliveryId == id);
                    if (items.Any())
                    {

                        JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Edycja dostawy", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(items.FirstOrDefault()), NewValue = new JavaScriptSerializer().Serialize(item) };
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
                            if (!JDE_DeliveryExists(id))
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
        [Route("CreateDelivery")]
        [ResponseType(typeof(JDE_Deliveries))]
        public IHttpActionResult CreateDelivery(string token, JDE_Deliveries item, int UserId)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    item.TenantId = tenants.FirstOrDefault().TenantId;
                    item.CreatedOn = DateTime.Now;
                    db.JDE_Deliveries.Add(item);
                    db.SaveChanges();
                    JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Utworzenie dostawy", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, NewValue = new JavaScriptSerializer().Serialize(item) };
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
        [Route("DeleteDelivery")]
        [ResponseType(typeof(JDE_Deliveries))]
        public IHttpActionResult DeleteDelivery(string token, int id, int UserId)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = db.JDE_Deliveries.Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.DeliveryId == id);
                    if (items.Any())
                    {
                        JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Usunięcie dostawy", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(items.FirstOrDefault()) };
                        db.JDE_Deliveries.Remove(items.FirstOrDefault());
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

        private bool JDE_DeliveryExists(int id)
        {
            return db.JDE_Deliveries.Count(e => e.DeliveryId == id) > 0;
        }
    }
}
