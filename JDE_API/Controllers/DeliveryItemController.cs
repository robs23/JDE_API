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
    public class DeliveryItemController : ApiController
    {
        private Models.DbModel db = new Models.DbModel();

        [HttpGet]
        [Route("GetDeliveryItems")]
        public IHttpActionResult GetDeliverItems(string token, int page = 0, int pageSize = 0, int total = 0, string query = null)
        {

            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = (from di in db.JDE_DeliveryItems
                                 join d in db.JDE_Deliveries on di.DeliveryId equals d.DeliveryId
                                 join p in db.JDE_Parts on di.PartId equals p.PartId
                                 join t in db.JDE_Tenants on d.TenantId equals t.TenantId
                                 where d.TenantId == tenants.FirstOrDefault().TenantId
                                 orderby d.CreatedOn descending
                                 select new
                                 {
                                     DeliveryItemId = di.DeliveryItemId,
                                     DeliveryId = d.DeliveryId,
                                     DeliveredOn = d.DeliveredOn,
                                     PartId = di.PartId,
                                     PartName = p.Name,
                                     OrderId = d.OrderId,
                                     Amount = di.Amount,
                                     StorageBinId = di.StorageBinId,
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
        [Route("GetDeliveryItem")]
        [ResponseType(typeof(JDE_DeliveryItems))]
        public IHttpActionResult GetDeliveryItem(string token, int id)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = (from di in db.JDE_DeliveryItems
                                 join d in db.JDE_Deliveries on di.DeliveryId equals d.DeliveryId
                                 join p in db.JDE_Parts on di.PartId equals p.PartId
                                 join t in db.JDE_Tenants on d.TenantId equals t.TenantId
                                 where d.TenantId == tenants.FirstOrDefault().TenantId && di.DeliveryItemId == id
                                 orderby d.CreatedOn descending
                                 select new
                                 {
                                     DeliveryItemId = di.DeliveryItemId,
                                     DeliveryId = d.DeliveryId,
                                     DeliveredOn = d.DeliveredOn,
                                     PartId = di.PartId,
                                     PartName = p.Name,
                                     OrderId = d.OrderId,
                                     Amount = di.Amount,
                                     StorageBinId = di.StorageBinId,
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
        [Route("EditDeliveryItem")]
        [ResponseType(typeof(void))]

        public IHttpActionResult EditDeliveryItem(string token, int id, int UserId, JDE_DeliveryItems item)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = db.JDE_DeliveryItems.AsNoTracking().Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.DeliveryItemId == id);
                    if (items.Any())
                    {

                        JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Edycja pozycji w dostawie", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(items.FirstOrDefault()), NewValue = new JavaScriptSerializer().Serialize(item) };
                        db.JDE_Logs.Add(Log);
                        db.Entry(item).State = EntityState.Modified;
                        try
                        {
                            db.SaveChanges();
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            if (!JDE_DeliveryItemExists(id))
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
        [Route("CreateDeliveryItem")]
        [ResponseType(typeof(JDE_DeliveryItems))]
        public IHttpActionResult CreateDeliveryItem(string token, JDE_DeliveryItems item, int UserId)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    item.TenantId = tenants.FirstOrDefault().TenantId;
                    db.JDE_DeliveryItems.Add(item);
                    db.SaveChanges();
                    JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Utworzenie pozycji w dostawie", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, NewValue = new JavaScriptSerializer().Serialize(item) };
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
        [Route("DeleteDeliveryItem")]
        [ResponseType(typeof(JDE_DeliveryItems))]
        public IHttpActionResult DeleteDeliveryItem(string token, int id, int UserId)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = db.JDE_DeliveryItems.Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.DeliveryItemId == id);
                    if (items.Any())
                    {
                        JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Usunięcie pozycji w dostawie", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(items.FirstOrDefault()) };
                        db.JDE_DeliveryItems.Remove(items.FirstOrDefault());
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

        private bool JDE_DeliveryItemExists(int id)
        {
            return db.JDE_DeliveryItems.Count(e => e.DeliveryItemId == id) > 0;
        }
    }
}
