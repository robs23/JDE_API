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
    public class OrderController : ApiController
    {
        private Models.DbModel db = new Models.DbModel();

        [HttpGet]
        [Route("GetOrders")]
        public IHttpActionResult GetOrders(string token, int page = 0, int pageSize = 0, int total = 0, string query = null)
        {

            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = (from x in db.JDE_Orders
                                 join s in db.JDE_Companies on x.SupplierId equals s.CompanyId
                                 join u in db.JDE_Users on x.CreatedBy equals u.UserId
                                 join u2 in db.JDE_Users on x.LmBy equals u2.UserId into modifiedBy
                                 from mb in modifiedBy.DefaultIfEmpty()
                                 join t in db.JDE_Tenants on x.TenantId equals t.TenantId
                                 where x.TenantId == tenants.FirstOrDefault().TenantId
                                 orderby x.CreatedOn descending
                                 select new
                                 {
                                     OrderId = x.OrderId,
                                     OrderNo = x.OrderNo,
                                     SupplierOrderNo = x.SuppliersOrderNo,
                                     DeliveryOn = x.DeliveryOn,
                                     SupplierId = x.SupplierId,
                                     SupplierName = s.Name,
                                     CreatedOn = x.CreatedOn,
                                     CreatedBy = x.CreatedBy,
                                     CreatedByName = u.Name + " " + u.Surname,
                                     LmOn = x.LmOn,
                                     LmBy = x.LmBy,
                                     LmByName = mb.Name + " " + mb.Surname,
                                     TenantId = x.TenantId,
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
        [Route("GetOrder")]
        [ResponseType(typeof(JDE_Orders))]
        public IHttpActionResult GetOrder(string token, int id)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = (from x in db.JDE_Orders
                                 join s in db.JDE_Companies on x.SupplierId equals s.CompanyId
                                 join u in db.JDE_Users on x.CreatedBy equals u.UserId
                                 join u2 in db.JDE_Users on x.LmBy equals u2.UserId into modifiedBy
                                 from mb in modifiedBy.DefaultIfEmpty()
                                 join t in db.JDE_Tenants on x.TenantId equals t.TenantId
                                 where x.TenantId == tenants.FirstOrDefault().TenantId && x.OrderId==id
                                 orderby x.CreatedOn descending
                                 select new
                                 {
                                     OrderId = x.OrderId,
                                     OrderNo = x.OrderNo,
                                     SupplierOrderNo = x.SuppliersOrderNo,
                                     DeliveryOn = x.DeliveryOn,
                                     SupplierId = x.SupplierId,
                                     SupplierName = s.Name,
                                     CreatedOn = x.CreatedOn,
                                     CreatedBy = x.CreatedBy,
                                     CreatedByName = u.Name + " " + u.Surname,
                                     LmOn = x.LmOn,
                                     LmBy = x.LmBy,
                                     LmByName = mb.Name + " " + mb.Surname,
                                     TenantId = x.TenantId,
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
        [Route("EditOrder")]
        [ResponseType(typeof(void))]

        public IHttpActionResult EditOrder(string token, int id, int UserId, JDE_Orders item)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = db.JDE_Orders.AsNoTracking().Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.OrderId == id);
                    if (items.Any())
                    {

                        JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Edycja zamówienia", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(items.FirstOrDefault()), NewValue = new JavaScriptSerializer().Serialize(item) };
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
                            if (!JDE_OrderExists(id))
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
        [Route("CreateOrder")]
        [ResponseType(typeof(JDE_Orders))]
        public IHttpActionResult CreateOrder(string token, JDE_Orders item, int UserId)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    item.TenantId = tenants.FirstOrDefault().TenantId;
                    item.CreatedOn = DateTime.Now;
                    db.JDE_Orders.Add(item);
                    db.SaveChanges();
                    JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Utworzenie zamówienia", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, NewValue = new JavaScriptSerializer().Serialize(item) };
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
        [Route("DeleteOrder")]
        [ResponseType(typeof(JDE_Orders))]
        public IHttpActionResult DeleteOrder(string token, int id, int UserId)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = db.JDE_Orders.Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.OrderId == id);
                    if (items.Any())
                    {
                        JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Usunięcie zamówienia", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(items.FirstOrDefault()) };
                        db.JDE_Orders.Remove(items.FirstOrDefault());
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

        private bool JDE_OrderExists(int id)
        {
            return db.JDE_Orders.Count(e => e.OrderId == id) > 0;
        }
    }
}
