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
    public class CompanyTypeController : ApiController
    {
        private Models.DbModel db = new Models.DbModel();

        [HttpGet]
        [Route("GetCompanyTypes")]
        public IHttpActionResult GetCompanyTypes(string token, int page = 0, int pageSize = 0, int total = 0, string query = null)
        {

            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = (from x in db.JDE_CompanyTypes
                                 join u in db.JDE_Users on x.CreatedBy equals u.UserId
                                 join u2 in db.JDE_Users on x.LmBy equals u2.UserId into modifiedBy
                                 from mb in modifiedBy.DefaultIfEmpty()
                                 join t in db.JDE_Tenants on x.TenantId equals t.TenantId
                                 where x.TenantId == tenants.FirstOrDefault().TenantId
                                 orderby x.CreatedOn descending
                                 select new
                                 {
                                     CompanyTypeId = x.CompanyTypeId,
                                     Name = x.Name,
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
        [Route("GetCompanyType")]
        [ResponseType(typeof(JDE_CompanyTypes))]
        public IHttpActionResult GetCompanyType(string token, int id)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = (from x in db.JDE_CompanyTypes
                                 join u in db.JDE_Users on x.CreatedBy equals u.UserId
                                 join u2 in db.JDE_Users on x.LmBy equals u2.UserId into modifiedBy
                                 from mb in modifiedBy.DefaultIfEmpty()
                                 join t in db.JDE_Tenants on x.TenantId equals t.TenantId
                                 where x.TenantId == tenants.FirstOrDefault().TenantId
                                 orderby x.CreatedOn descending
                                 select new
                                 {
                                     CompanyTypeId = x.CompanyTypeId,
                                     Name = x.Name,
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
        [Route("EditCompanyType")]
        [ResponseType(typeof(void))]

        public IHttpActionResult EditCompanyType(string token, int id, int UserId, JDE_CompanyTypes item)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = db.JDE_CompanyTypes.AsNoTracking().Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.CompanyTypeId == id);
                    if (items.Any())
                    {

                        JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Edycja typu firmy", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(items.FirstOrDefault()), NewValue = new JavaScriptSerializer().Serialize(item) };
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
                            if (!JDE_CompanyTypeExists(id))
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
        [Route("CreateCompanyType")]
        [ResponseType(typeof(JDE_CompanyTypes))]
        public IHttpActionResult CreateCompanyType(string token, JDE_CompanyTypes item, int UserId)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    item.TenantId = tenants.FirstOrDefault().TenantId;
                    item.CreatedOn = DateTime.Now;
                    db.JDE_CompanyTypes.Add(item);
                    db.SaveChanges();
                    JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Utworzenie typu firmy", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, NewValue = new JavaScriptSerializer().Serialize(item) };
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
        [Route("DeleteCompanyType")]
        [ResponseType(typeof(JDE_CompanyTypes))]
        public IHttpActionResult DeleteCompanyType(string token, int id, int UserId)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = db.JDE_CompanyTypes.Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.CompanyTypeId == id);
                    if (items.Any())
                    {
                        JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Usunięcie typu firmy", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(items.FirstOrDefault()) };
                        db.JDE_CompanyTypes.Remove(items.FirstOrDefault());
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

        private bool JDE_CompanyTypeExists(int id)
        {
            return db.JDE_CompanyTypes.Count(e => e.CompanyTypeId == id) > 0;
        }
    }
}
