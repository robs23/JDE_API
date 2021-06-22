using JDE_API.Models;
using JDE_API.Static;
using NLog;
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
    public class StorageBinController : ApiController
    {
        private Models.DbModel db = new Models.DbModel();
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [HttpGet]
        [Route("GetStorageBins")]
        public IHttpActionResult GetStorageBins(string token, int page = 0, int pageSize = 0, int total = 0, string query = null)
        {

            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = (from st in db.JDE_StorageBins
                                 join u in db.JDE_Users on st.CreatedBy equals u.UserId
                                 join u2 in db.JDE_Users on st.LmBy equals u2.UserId into modifiedBy
                                 from mb in modifiedBy.DefaultIfEmpty()
                                 join t in db.JDE_Tenants on st.TenantId equals t.TenantId
                                 where st.TenantId == tenants.FirstOrDefault().TenantId
                                 orderby st.CreatedOn descending
                                 select new
                                 {
                                     StorageBinId = st.StorageBinId,
                                     Number = st.Number,
                                     Name = st.Name,
                                     Token = st.Token,
                                     CreatedOn = st.CreatedOn,
                                     CreatedBy = st.CreatedBy,
                                     CreatedByName = u.Name + " " + u.Surname,
                                     LmOn = st.LmOn,
                                     LmBy = st.LmBy,
                                     LmByName = mb.Name + " " + mb.Surname,
                                     TenantId = st.TenantId,
                                     TenantName = t.TenantName,
                                     IsArchived = st.IsArchived
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
        [Route("GetStorageBin")]
        [ResponseType(typeof(JDE_StorageBins))]
        public IHttpActionResult GetStorageBin(string token, int id)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = (from st in db.JDE_StorageBins
                                 join u in db.JDE_Users on st.CreatedBy equals u.UserId
                                 join u2 in db.JDE_Users on st.LmBy equals u2.UserId into modifiedBy
                                 from mb in modifiedBy.DefaultIfEmpty()
                                 join t in db.JDE_Tenants on st.TenantId equals t.TenantId
                                 where st.TenantId == tenants.FirstOrDefault().TenantId && st.StorageBinId == id
                                 orderby st.CreatedOn descending
                                 select new
                                 {
                                     StorageBinId = st.StorageBinId,
                                     Number = st.Number,
                                     Name = st.Name,
                                     Token = st.Token,
                                     CreatedOn = st.CreatedOn,
                                     CreatedBy = st.CreatedBy,
                                     CreatedByName = u.Name + " " + u.Surname,
                                     LmOn = st.LmOn,
                                     LmBy = st.LmBy,
                                     LmByName = mb.Name + " " + mb.Surname,
                                     TenantId = st.TenantId,
                                     TenantName = t.TenantName,
                                     IsArchived = st.IsArchived
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
        [Route("EditStorageBin")]
        [ResponseType(typeof(void))]

        public IHttpActionResult EditStorageBin(string token, int id, int UserId, JDE_StorageBins item)
        {
            string methodName = System.Reflection.MethodInfo.GetCurrentMethod().Name;
            Logger.Info("Start {methodName}. Id={id}, UserId={UserId}", methodName, id, UserId);
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    string newItem = "";
                    try
                    {
                        var items = db.JDE_StorageBins.AsNoTracking().Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.StorageBinId == id);
                        if (items.Any())
                        {
                            Logger.Info("{methodName}: Znalazłem odpowiedni StorageBin. Przystępuję do edycji Id={id}, UserId={UserId}", methodName, id, UserId);
                            item.CreatedOn = items.FirstOrDefault().CreatedOn; //switch back to original createdOn date
                            item.LmBy = UserId;
                            item.LmOn = DateTime.Now;
                            newItem = new JavaScriptSerializer().Serialize(item);
                            JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Edycja regału", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(items.FirstOrDefault()), NewValue = new JavaScriptSerializer().Serialize(item) };
                            db.JDE_Logs.Add(Log);
                            db.Entry(item).State = EntityState.Modified;
                            try
                            {
                                db.SaveChanges();
                                Logger.Info("{methodName}: Edycja zakończona powodzeniem. Przystępuję do edycji Id={id}, UserId={UserId}", methodName, id, UserId);
                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                if (!JDE_StorageBinExists(id))
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
                    catch (Exception ex)
                    {
                        Logger.Error("Błąd w {methodName}. Id={id}, UserId={UserId}. Szczegóły: {Message}, nowa wartość: {newItem}", methodName, id, UserId, ex.ToString(), newItem);
                        return StatusCode(HttpStatusCode.InternalServerError);
                    }
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpGet]
        [Route("ArchiveStorageBin")]
        [ResponseType(typeof(void))]
        public HttpResponseMessage ArchiveStorageBin(string token, int id, int UserId)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = db.JDE_StorageBins.AsNoTracking().Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.StorageBinId == id);
                    if (items.Any())
                    {
                        JDE_StorageBins orgItem = items.FirstOrDefault();

                        orgItem.IsArchived = true;
                        JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Archiwizacja regału", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(items.FirstOrDefault()), NewValue = "" };
                        db.JDE_Logs.Add(Log);

                        try
                        {
                            db.Entry(orgItem).State = EntityState.Modified;
                            db.SaveChanges();

                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            if (!JDE_StorageBinExists(id))
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
        [Route("CreateStorageBin")]
        [ResponseType(typeof(JDE_StorageBins))]
        public IHttpActionResult CreateStorageBin(string token, JDE_StorageBins item, int UserId)
        {
            string methodName = System.Reflection.MethodInfo.GetCurrentMethod().Name;
            Logger.Info("Start {methodName}. Name={Name}, UserId={UserId}", methodName, item.Name, UserId);
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    try
                    {
                        item.TenantId = tenants.FirstOrDefault().TenantId;
                        item.CreatedOn = DateTime.Now;
                        item.Token = Static.Utilities.GetToken();
                        db.JDE_StorageBins.Add(item);
                        db.SaveChanges();
                        JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Utworzenie regału", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, NewValue = new JavaScriptSerializer().Serialize(item) };
                        db.JDE_Logs.Add(Log);
                        db.SaveChanges();
                        Logger.Info("{methodName}: Zapis zakończony powodzeniem. ID={ID}, UserId={UserId}", methodName, item.StorageBinId, UserId);
                        return Ok(item);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Błąd w {methodName}. ID={ID}, UserId={UserId}. Szczegóły: {Message}", methodName, item.StorageBinId, UserId, ex.ToString());
                        return StatusCode(HttpStatusCode.InternalServerError);
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

        [HttpDelete]
        [Route("DeleteStorageBin")]
        [ResponseType(typeof(JDE_StorageBins))]
        public IHttpActionResult DeleteStorageBin(string token, int id, int UserId)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = db.JDE_StorageBins.Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.StorageBinId == id);
                    if (items.Any())
                    {
                        JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Usunięcie regału", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(items.FirstOrDefault()) };
                        db.JDE_StorageBins.Remove(items.FirstOrDefault());
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

        private bool JDE_StorageBinExists(int id)
        {
            return db.JDE_StorageBins.Count(e => e.StorageBinId == id) > 0;
        }
    }
}