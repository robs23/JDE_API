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
    public class CompanyController : ApiController
    {
        private Models.DbModel db = new Models.DbModel();

        [HttpGet]
        [Route("GetCompanies")]
        public IHttpActionResult GetCompanies(string token, int page = 0, int pageSize = 0, int total = 0, DateTime? dFrom = null, DateTime? dTo = null, string query = null)
        {

            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    if (dFrom == null)
                    {
                        if (db.JDE_Handlings.Any())
                        {
                            dFrom = db.JDE_Companies.Min(x => x.CreatedOn).Value.AddDays(-1);
                        }
                        else
                        {
                            dFrom = new DateTime(2018, 1, 1);
                        }
                    }
                    if (dTo == null)
                    {
                        if (db.JDE_Companies.Any())
                        {
                            dTo = db.JDE_Companies.Max(x => x.CreatedOn).Value.AddDays(1);

                        }
                        else
                        {
                            dTo = new DateTime(2030, 12, 31);
                        }
                    }

                    var items = (from c in db.JDE_Companies
                                 join ct in db.JDE_CompanyTypes on c.TypeId equals ct.CompanyTypeId
                                 join u in db.JDE_Users on c.CreatedBy equals u.UserId
                                 join u2 in db.JDE_Users on c.LmBy equals u2.UserId into modifiedBy
                                 from mb in modifiedBy.DefaultIfEmpty()
                                 join t in db.JDE_Tenants on c.TenantId equals t.TenantId
                                 where c.TenantId == tenants.FirstOrDefault().TenantId && c.CreatedOn >= dFrom && c.CreatedOn <= dTo
                                 orderby c.CreatedOn descending
                                 select new
                                 {
                                     CompanyId = c.CompanyId,
                                     Name = c.Name,
                                     CreatedOn = c.CreatedOn,
                                     CreatedBy = c.CreatedBy,
                                     CreatedByName = u.Name + " " + u.Surname,
                                     LmOn = c.LmOn,
                                     LmBy = c.LmBy,
                                     LmByName = mb.Name + " " + mb.Surname,
                                     Street = c.Street,
                                     Street2 = c.Street2,
                                     BuildingNo = c.BuildingNo,
                                     LocalNo = c.LocalNo,
                                     ZipCode = c.ZipCode,
                                     City = c.City,
                                     Country = c.Country,
                                     TypeId = c.TypeId,
                                     CompanyTypeName = ct.Name,
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
        [Route("GetCompany")]
        [ResponseType(typeof(JDE_Companies))]
        public IHttpActionResult GetCompany(string token, int id)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = (from c in db.JDE_Companies
                                 join ct in db.JDE_CompanyTypes on c.TypeId equals ct.CompanyTypeId
                                 join u in db.JDE_Users on c.CreatedBy equals u.UserId
                                 join u2 in db.JDE_Users on c.LmBy equals u2.UserId into modifiedBy
                                 from mb in modifiedBy.DefaultIfEmpty()
                                 join t in db.JDE_Tenants on c.TenantId equals t.TenantId
                                 where c.TenantId == tenants.FirstOrDefault().TenantId && c.CompanyId ==id
                                 orderby c.CreatedOn descending
                                 select new
                                 {
                                     CompanyId = c.CompanyId,
                                     Name = c.Name,
                                     CreatedOn = c.CreatedOn,
                                     CreatedBy = c.CreatedBy,
                                     CreatedByName = u.Name + " " + u.Surname,
                                     LmOn = c.LmOn,
                                     LmBy = c.LmBy,
                                     LmByName = mb.Name + " " + mb.Surname,
                                     Street = c.Street,
                                     Street2 = c.Street2,
                                     BuildingNo = c.BuildingNo,
                                     LocalNo = c.LocalNo,
                                     ZipCode = c.ZipCode,
                                     City = c.City,
                                     Country = c.Country,
                                     TypeId = c.TypeId,
                                     CompanyTypeName = ct.Name,
                                     TenantId = c.TenantId,
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
        [Route("EditCompany")]
        [ResponseType(typeof(void))]

        public IHttpActionResult EditCompany(string token, int id, int UserId, JDE_Companies item)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = db.JDE_Companies.AsNoTracking().Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.CompanyId == id);
                    if (items.Any())
                    {
                        
                        JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Edycja firmy", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(items.FirstOrDefault()), NewValue = new JavaScriptSerializer().Serialize(item) };
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
                            if (!JDE_CompaniesExists(id))
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

        [HttpGet]
        [Route("ArchiveCompany")]
        [ResponseType(typeof(void))]
        public HttpResponseMessage ArchiveCompany(string token, int id, int UserId)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = db.JDE_Companies.AsNoTracking().Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.CompanyId == id);
                    if (items.Any())
                    {
                        JDE_Companies orgItem = items.FirstOrDefault();

                        orgItem.IsArchived = true;
                        JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Archiwizacja firmy", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(items.FirstOrDefault()), NewValue = "" };
                        db.JDE_Logs.Add(Log);

                        try
                        {
                            db.Entry(orgItem).State = EntityState.Modified;
                            db.SaveChanges();

                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            if (!JDE_CompaniesExists(id))
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

        [HttpPost]
        [Route("CreateCompany")]
        [ResponseType(typeof(JDE_Companies))]
        public IHttpActionResult CreateCompany(string token, JDE_Companies item, int UserId)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    item.TenantId = tenants.FirstOrDefault().TenantId;
                    item.CreatedOn = DateTime.Now;
                    db.JDE_Companies.Add(item);
                    db.SaveChanges();
                    JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Utworzenie firmy", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, NewValue = new JavaScriptSerializer().Serialize(item) };
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
        [Route("DeleteCompany")]
        [ResponseType(typeof(JDE_Companies))]
        public IHttpActionResult DeleteCompany(string token, int id, int UserId)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = db.JDE_Companies.Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.CompanyId == id);
                    if (items.Any())
                    {
                        JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Usunięcie firmy", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(items.FirstOrDefault()) };
                        db.JDE_Companies.Remove(items.FirstOrDefault());
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

        private bool JDE_CompaniesExists(int id)
        {
            return db.JDE_Companies.Count(e => e.CompanyId == id) > 0;
        }
    }
}
