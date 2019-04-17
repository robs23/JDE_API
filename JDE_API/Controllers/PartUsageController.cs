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
    public class PartUsageController : ApiController
    {
        private Models.DbModel db = new Models.DbModel();

        [HttpGet]
        [Route("GetPartUsages")]
        public IHttpActionResult GetPartUsages(string token, int page = 0, int pageSize = 0, int total = 0, string query = null)
        {

            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = (from pu in db.JDE_PartUsages
                                 join p in db.JDE_Parts on pu.PartId equals p.PartId
                                 join pl in db.JDE_Places on pu.PlaceId equals pl.PlaceId
                                 join u in db.JDE_Users on pu.CreatedBy equals u.UserId
                                 join u2 in db.JDE_Users on pu.LmBy equals u2.UserId into modifiedBy
                                 from mb in modifiedBy.DefaultIfEmpty()
                                 join t in db.JDE_Tenants on pu.TenantId equals t.TenantId
                                 where pu.TenantId == tenants.FirstOrDefault().TenantId
                                 orderby pu.CreatedOn descending
                                 select new
                                 {
                                     PartUsageId = pu.PartUsageId,
                                     PartId = p.PartId,
                                     PartName = p.Name,
                                     PlaceId = pu.PlaceId,
                                     PlaceName = pl.Name,
                                     HandlingId = pu.HandlingId,
                                     Amount = pu.Amount,
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
        [Route("GetPartUsage")]
        [ResponseType(typeof(JDE_PartUsages))]
        public IHttpActionResult GetPartUsage(string token, int id)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = (from pu in db.JDE_PartUsages
                                 join p in db.JDE_Parts on pu.PartId equals p.PartId
                                 join pl in db.JDE_Places on pu.PlaceId equals pl.PlaceId
                                 join u in db.JDE_Users on pu.CreatedBy equals u.UserId
                                 join u2 in db.JDE_Users on pu.LmBy equals u2.UserId into modifiedBy
                                 from mb in modifiedBy.DefaultIfEmpty()
                                 join t in db.JDE_Tenants on pu.TenantId equals t.TenantId
                                 where pu.TenantId == tenants.FirstOrDefault().TenantId && pu.PartUsageId == id
                                 orderby pu.CreatedOn descending
                                 select new
                                 {
                                     PartUsageId = pu.PartUsageId,
                                     PartId = p.PartId,
                                     PartName = p.Name,
                                     PlaceId = pu.PlaceId,
                                     PlaceName = pl.Name,
                                     HandlingId = pu.HandlingId,
                                     Amount = pu.Amount,
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
        [Route("EditPartUsage")]
        [ResponseType(typeof(void))]

        public IHttpActionResult EditPartUsage(string token, int id, int UserId, JDE_PartUsages item)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = db.JDE_PartUsages.AsNoTracking().Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.PartUsageId == id);
                    if (items.Any())
                    {

                        JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Edycja zużycia części", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(items.FirstOrDefault()), NewValue = new JavaScriptSerializer().Serialize(item) };
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
                            if (!JDE_PartUsageExists(id))
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
        [Route("CreatePartUsage")]
        [ResponseType(typeof(JDE_PartUsages))]
        public IHttpActionResult CreatePartUsage(string token, JDE_PartUsages item, int UserId)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    item.TenantId = tenants.FirstOrDefault().TenantId;
                    item.CreatedOn = DateTime.Now;
                    db.JDE_PartUsages.Add(item);
                    db.SaveChanges();
                    JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Zużycie części", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, NewValue = new JavaScriptSerializer().Serialize(item) };
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
        [Route("DeletePartUsage")]
        [ResponseType(typeof(JDE_PartUsages))]
        public IHttpActionResult DeletePartUsage(string token, int id, int UserId)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = db.JDE_PartUsages.Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.PartUsageId == id);
                    if (items.Any())
                    {
                        JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Usunięcie zużycia częśći", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(items.FirstOrDefault()) };
                        db.JDE_PartUsages.Remove(items.FirstOrDefault());
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

        private bool JDE_PartUsageExists(int id)
        {
            return db.JDE_PartUsages.Count(e => e.PartUsageId == id) > 0;
        }
    }
}
