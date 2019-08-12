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
    public class PartController : ApiController
    {
        private Models.DbModel db = new Models.DbModel();

        [HttpGet]
        [Route("GetParts")]
        public IHttpActionResult GetParts(string token, int page = 0, int pageSize = 0, int total = 0, string query = null)
        {

            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = (from p in db.JDE_Parts
                                 join pr in db.JDE_Companies on p.ProducerId equals pr.CompanyId into producers
                                 from prs in producers.DefaultIfEmpty()
                                 join s in db.JDE_Companies on p.SupplierId equals s.CompanyId into suppliers
                                 from sups in suppliers.DefaultIfEmpty()
                                 join u in db.JDE_Users on p.CreatedBy equals u.UserId
                                 join u2 in db.JDE_Users on p.LmBy equals u2.UserId into modifiedBy
                                 from mb in modifiedBy.DefaultIfEmpty()
                                 join t in db.JDE_Tenants on p.TenantId equals t.TenantId
                                 where p.TenantId == tenants.FirstOrDefault().TenantId
                                 orderby p.CreatedOn descending
                                 select new
                                 {
                                     PartId = p.PartId,
                                     Name = p.Name,
                                     Description = p.Description,
                                     EAM = p.EAN,
                                     ProducerId = p.ProducerId,
                                     ProducerName = prs.Name,
                                     SupplierId = p.SupplierId,
                                     SupplierName = sups.Name,
                                     Symbol = p.Symbol,
                                     Destination = p.Destination,
                                     Appliance = p.Appliance,
                                     UsedOn = p.UsedOn,
                                     Token = p.Token,
                                     CreatedOn = p.CreatedOn,
                                     CreatedBy = p.CreatedBy,
                                     CreatedByName = u.Name + " " + u.Surname,
                                     LmOn = p.LmOn,
                                     LmBy = p.LmBy,
                                     LmByName = mb.Name + " " + mb.Surname,
                                     TenantId = p.TenantId,
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
        [Route("GetPart")]
        [ResponseType(typeof(JDE_Parts))]
        public IHttpActionResult GetPart(string token, int id)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = (from p in db.JDE_Parts
                                 join pr in db.JDE_Companies on p.ProducerId equals pr.CompanyId
                                 join s in db.JDE_Companies on p.SupplierId equals s.CompanyId
                                 join u in db.JDE_Users on p.CreatedBy equals u.UserId
                                 join u2 in db.JDE_Users on p.LmBy equals u2.UserId into modifiedBy
                                 from mb in modifiedBy.DefaultIfEmpty()
                                 join t in db.JDE_Tenants on p.TenantId equals t.TenantId
                                 where p.TenantId == tenants.FirstOrDefault().TenantId && p.PartId ==id
                                 orderby p.CreatedOn descending
                                 select new
                                 {
                                     PartId = p.PartId,
                                     Name = p.Name,
                                     Description = p.Description,
                                     EAM = p.EAN,
                                     ProducerId = p.ProducerId,
                                     ProducerName = pr.Name,
                                     SupplierId = p.SupplierId,
                                     SupplierName = s.Name,
                                     Symbol = p.Symbol,
                                     Destination = p.Destination,
                                     Appliance = p.Appliance,
                                     UsedOn = p.UsedOn,
                                     Token = p.Token,
                                     CreatedOn = p.CreatedOn,
                                     CreatedBy = p.CreatedBy,
                                     CreatedByName = u.Name + " " + u.Surname,
                                     LmOn = p.LmOn,
                                     LmBy = p.LmBy,
                                     LmByName = mb.Name + " " + mb.Surname,
                                     TenantId = p.TenantId,
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
        [Route("EditPart")]
        [ResponseType(typeof(void))]

        public IHttpActionResult EditPart(string token, int id, int UserId, JDE_Parts item)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = db.JDE_Parts.AsNoTracking().Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.PartId == id);
                    if (items.Any())
                    {

                        JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Edycja części", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(items.FirstOrDefault()), NewValue = new JavaScriptSerializer().Serialize(item) };
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
                            if (!JDE_PartExists(id))
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
        [Route("CreatePart")]
        [ResponseType(typeof(JDE_Parts))]
        public IHttpActionResult CreatePart(string token, JDE_Parts item, int UserId)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    item.TenantId = tenants.FirstOrDefault().TenantId;
                    item.CreatedOn = DateTime.Now;
                    item.Token = Static.Utilities.GetToken();
                    db.JDE_Parts.Add(item);
                    db.SaveChanges();
                    JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Utworzenie części", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, NewValue = new JavaScriptSerializer().Serialize(item) };
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
        [Route("DeletePart")]
        [ResponseType(typeof(JDE_Parts))]
        public IHttpActionResult DeletePart(string token, int id, int UserId)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = db.JDE_Parts.Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.PartId == id);
                    if (items.Any())
                    {
                        JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Usunięcie części", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(items.FirstOrDefault()) };
                        db.JDE_Parts.Remove(items.FirstOrDefault());
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

        private bool JDE_PartExists(int id)
        {
            return db.JDE_Parts.Count(e => e.PartId == id) > 0;
        }
    }
}
