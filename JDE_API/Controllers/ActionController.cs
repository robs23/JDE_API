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
    public class ActionController : ApiController
    {
        private Models.DbModel db = new Models.DbModel();

        [HttpGet]
        [Route("GetActions")]
        public IHttpActionResult GetActions(string token, int page = 0, int total = 0, string query = null)
        {

            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {

                    var items = (from a in db.JDE_Actions
                                 join u in db.JDE_Users on a.CreatedBy equals u.UserId
                                 join u2 in db.JDE_Users on a.LmBy equals u2.UserId into LmByNames
                                 from lms in LmByNames.DefaultIfEmpty()
                                 join t in db.JDE_Tenants on a.TenantId equals t.TenantId
                                 where a.TenantId == tenants.FirstOrDefault().TenantId
                                 orderby a.CreatedOn descending
                                 select new
                                 {
                                     ActionId = a.ActionId,
                                     Name = a.Name,
                                     CreatedBy = u.UserId,
                                     CreatedByName = u.Name + " " + u.Surname,
                                     CreatedOn = a.CreatedOn,
                                     LmBy = a.LmBy,
                                     LmByName = lms.Name + " " + lms.Surname,
                                     TenantId = a.TenantId,
                                     TenantName = t.TenantName,
                                     ActionTypeId = a.ActionTypeId
                                 });
                    if (items.Any())
                    {
                        if (query != null)
                        {

                            items = items.Where(query);

                        }

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
        [Route("GetAction")]
        [ResponseType(typeof(JDE_Actions))]
        public IHttpActionResult GetAction(string token, int id)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = (from a in db.JDE_Actions
                                 join u in db.JDE_Users on a.CreatedBy equals u.UserId
                                 join u2 in db.JDE_Users on a.LmBy equals u2.UserId into LmByNames
                                 from lms in LmByNames.DefaultIfEmpty()
                                 join t in db.JDE_Tenants on a.TenantId equals t.TenantId
                                 where a.TenantId == tenants.FirstOrDefault().TenantId && a.ActionId==id
                                 orderby a.CreatedOn descending
                                 select new
                                 {
                                     ActionId = a.ActionId,
                                     Name = a.Name,
                                     CreatedBy = u.UserId,
                                     CreatedByName = u.Name + " " + u.Surname,
                                     CreatedOn = a.CreatedOn,
                                     LmBy = a.LmBy,
                                     LmByName = lms.Name + " " + lms.Surname,
                                     TenantId = a.TenantId,
                                     TenantName = t.TenantName,
                                     ActionTypeId = a.ActionTypeId
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
        [Route("EditAction")]
        [ResponseType(typeof(void))]

        public IHttpActionResult EditAction(string token, int id, int UserId, JDE_Actions item)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = db.JDE_Actions.AsNoTracking().Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.ActionId == id);
                    if (items.Any())
                    {
                        string descr = "Edycja czynności";
                        item.LmOn = DateTime.Now;
                        JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = descr, TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(items.FirstOrDefault()), NewValue = new JavaScriptSerializer().Serialize(item) };
                        db.JDE_Logs.Add(Log);
                        db.Entry(item).State = EntityState.Modified;
                        try
                        {
                            db.SaveChanges();
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            if (!JDE_ActionsExists(id))
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
        [Route("CreateAction")]
        [ResponseType(typeof(JDE_Actions))]
        public IHttpActionResult CreateAction(string token, JDE_Actions item, int UserId)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    item.TenantId = tenants.FirstOrDefault().TenantId;
                    item.CreatedOn = DateTime.Now;
                    db.JDE_Actions.Add(item);
                    db.SaveChanges();
                    JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Utworzenie czynności", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, NewValue = new JavaScriptSerializer().Serialize(item) };
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
        [Route("DeleteAction")]
        [ResponseType(typeof(JDE_Actions))]
        public IHttpActionResult DeleteAction(string token, int id, int UserId)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = db.JDE_Actions.Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.ActionId == id);
                    if (items.Any())
                    {
                        JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Usunięcie czynności", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(items.FirstOrDefault()) };
                        db.JDE_Actions.Remove(items.FirstOrDefault());
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

        private bool JDE_ActionsExists(int id)
        {
            return db.JDE_Actions.Count(e => e.ActionId == id) > 0;
        }
    }
}
