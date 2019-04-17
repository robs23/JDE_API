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
    public class BomController : ApiController
    {
        private Models.DbModel db = new Models.DbModel();

        [HttpGet]
        [Route("GetBoms")]
        public IHttpActionResult GetBoms(string token, int page = 0, int pageSize = 0, int total = 0, string query = null)
        {

            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = (from b in db.JDE_Boms
                                 join p in db.JDE_Parts on b.PartId equals p.PartId
                                 join pl in db.JDE_Places on b.PlaceId equals pl.PlaceId
                                 join u in db.JDE_Users on b.CreatedBy equals u.UserId
                                 join u2 in db.JDE_Users on b.LmBy equals u2.UserId into modifiedBy
                                 from mb in modifiedBy.DefaultIfEmpty()
                                 join t in db.JDE_Tenants on b.TenantId equals t.TenantId
                                 where b.TenantId == tenants.FirstOrDefault().TenantId
                                 orderby b.CreatedOn descending
                                 select new
                                 {
                                     BomId = b.BomId,
                                     PartId = b.PartId,
                                     PartName = p.Name,
                                     PlaceId = b.PlaceId,
                                     PlaceName = pl.Name,
                                     Amount = b.Amount,
                                     Unit = b.Unit,
                                     ValidFrom = b.ValidFrom,
                                     ValidTo = b.ValidTo,
                                     CreatedOn = b.CreatedOn,
                                     CreatedBy = b.CreatedBy,
                                     CreatedByName = u.Name + " " + u.Surname,
                                     LmOn = b.LmOn,
                                     LmBy = b.LmBy,
                                     LmByName = mb.Name + " " + mb.Surname,
                                     TenantId = b.TenantId,
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
        [Route("GetBom")]
        [ResponseType(typeof(JDE_Boms))]
        public IHttpActionResult GetBom(string token, int id)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = (from b in db.JDE_Boms
                                 join p in db.JDE_Parts on b.PartId equals p.PartId
                                 join pl in db.JDE_Places on b.PlaceId equals pl.PlaceId
                                 join u in db.JDE_Users on b.CreatedBy equals u.UserId
                                 join u2 in db.JDE_Users on b.LmBy equals u2.UserId into modifiedBy
                                 from mb in modifiedBy.DefaultIfEmpty()
                                 join t in db.JDE_Tenants on b.TenantId equals t.TenantId
                                 where b.TenantId == tenants.FirstOrDefault().TenantId && b.BomId==id
                                 orderby b.CreatedOn descending
                                 select new
                                 {
                                     BomId = b.BomId,
                                     PartId = b.PartId,
                                     PartName = p.Name,
                                     PlaceId = b.PlaceId,
                                     PlaceName = pl.Name,
                                     Amount = b.Amount,
                                     Unit = b.Unit,
                                     ValidFrom = b.ValidFrom,
                                     ValidTo = b.ValidTo,
                                     CreatedOn = b.CreatedOn,
                                     CreatedBy = b.CreatedBy,
                                     CreatedByName = u.Name + " " + u.Surname,
                                     LmOn = b.LmOn,
                                     LmBy = b.LmBy,
                                     LmByName = mb.Name + " " + mb.Surname,
                                     TenantId = b.TenantId,
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
        [Route("EditBom")]
        [ResponseType(typeof(void))]

        public IHttpActionResult EditBom(string token, int id, int UserId, JDE_Boms item)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = db.JDE_Boms.AsNoTracking().Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.BomId == id);
                    if (items.Any())
                    {

                        JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Edycja BOMu", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(items.FirstOrDefault()), NewValue = new JavaScriptSerializer().Serialize(item) };
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
                            if (!JDE_BomExists(id))
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
        [Route("CreateBom")]
        [ResponseType(typeof(JDE_Boms))]
        public IHttpActionResult CreateBom(string token, JDE_Boms item, int UserId)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    item.TenantId = tenants.FirstOrDefault().TenantId;
                    item.CreatedOn = DateTime.Now;
                    db.JDE_Boms.Add(item);
                    db.SaveChanges();
                    JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Dodanie do BOMu", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, NewValue = new JavaScriptSerializer().Serialize(item) };
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
        [Route("DeleteBom")]
        [ResponseType(typeof(JDE_Boms))]
        public IHttpActionResult DeleteBom(string token, int id, int UserId)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = db.JDE_Boms.Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.BomId == id);
                    if (items.Any())
                    {
                        JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Usunięcie z BOMu", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(items.FirstOrDefault()) };
                        db.JDE_Boms.Remove(items.FirstOrDefault());
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

        private bool JDE_BomExists(int id)
        {
            return db.JDE_Boms.Count(e => e.BomId == id) > 0;
        }
    }
}
