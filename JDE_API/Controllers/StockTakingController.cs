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
    public class StockTakingController : ApiController
    {
        private Models.DbModel db = new Models.DbModel();
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [HttpGet]
        [Route("GetStockTakings")]
        public IHttpActionResult GetStockTakings(string token, int page = 0, int pageSize = 0, int total = 0, string query = null)
        {

            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = (from st in db.JDE_StockTakings
                                 join p in db.JDE_Parts on st.PartId equals p.PartId
                                 join u in db.JDE_Users on st.CreatedBy equals u.UserId
                                 join u2 in db.JDE_Users on st.LmBy equals u2.UserId into modifiedBy
                                 from mb in modifiedBy.DefaultIfEmpty()
                                 join comp in db.JDE_Companies on p.ProducerId equals comp.CompanyId into producer
                                 from pr in producer.DefaultIfEmpty()
                                 join stbin in db.JDE_StorageBins on st.StorageBinId equals stbin.StorageBinId into storagebin
                                 from sbin in storagebin.DefaultIfEmpty()
                                 join t in db.JDE_Tenants on st.TenantId equals t.TenantId
                                 where st.TenantId == tenants.FirstOrDefault().TenantId
                                 orderby st.CreatedOn descending
                                 select new
                                 {
                                     StockTakingId = st.StockTakingId,
                                     PartId = p.PartId,
                                     Name = p.Name,
                                     ProducerId = p.ProducerId,
                                     ProducerName = pr.Name,
                                     Image = p.Image,
                                     Amount = st.Amount,
                                     StorageBinId = st.StorageBinId,
                                     StorageBinNumber = sbin.Number,
                                     TakingDate = st.TakingDate,
                                     CreatedOn = st.CreatedOn,
                                     CreatedBy = st.CreatedBy,
                                     CreatedByName = u.Name + " " + u.Surname,
                                     LmOn = st.LmOn,
                                     LmBy = st.LmBy,
                                     LmByName = mb.Name + " " + mb.Surname,
                                     TenantId = st.TenantId,
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
        [Route("GetStockTaking")]
        [ResponseType(typeof(JDE_StockTakings))]
        public IHttpActionResult GetStockTaking(string token, int id)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = (from st in db.JDE_StockTakings
                                 join p in db.JDE_Parts on st.PartId equals p.PartId
                                 join u in db.JDE_Users on st.CreatedBy equals u.UserId
                                 join u2 in db.JDE_Users on st.LmBy equals u2.UserId into modifiedBy
                                 from mb in modifiedBy.DefaultIfEmpty()
                                 join comp in db.JDE_Companies on p.ProducerId equals comp.CompanyId into producer
                                 from pr in producer.DefaultIfEmpty()
                                 join stbin in db.JDE_StorageBins on st.StorageBinId equals stbin.StorageBinId into storagebin
                                 from sbin in storagebin.DefaultIfEmpty()
                                 join t in db.JDE_Tenants on st.TenantId equals t.TenantId
                                 where st.TenantId == tenants.FirstOrDefault().TenantId && st.StockTakingId == id
                                 orderby st.CreatedOn descending
                                 select new
                                 {
                                     StockTakingId = st.StockTakingId,
                                     PartId = p.PartId,
                                     Name = p.Name,
                                     ProducerId = p.ProducerId,
                                     ProducerName = pr.Name,
                                     Image = p.Image,
                                     Amount = st.Amount,
                                     StorageBinId = st.StorageBinId,
                                     StorageBinNumber = sbin.Number,
                                     TakingDate = st.TakingDate,
                                     CreatedOn = st.CreatedOn,
                                     CreatedBy = st.CreatedBy,
                                     CreatedByName = u.Name + " " + u.Surname,
                                     LmOn = st.LmOn,
                                     LmBy = st.LmBy,
                                     LmByName = mb.Name + " " + mb.Surname,
                                     TenantId = st.TenantId,
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
        [Route("EditStockTaking")]
        [ResponseType(typeof(void))]

        public IHttpActionResult EditStockTaking(string token, int id, int UserId, JDE_StockTakings item)
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
                        var items = db.JDE_StockTakings.AsNoTracking().Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.StockTakingId == id);
                        if (items.Any())
                        {
                            Logger.Info("{methodName}: Znalazłem odpowiednie StockTaking. Przystępuję do edycji Id={id}, UserId={UserId}", methodName, id, UserId);
                            item.CreatedOn = items.FirstOrDefault().CreatedOn; //switch back to original createdOn date
                            item.LmBy = UserId;
                            item.LmOn = DateTime.Now;
                            newItem = new JavaScriptSerializer().Serialize(item);
                            JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Edycja inwentaryzacji części", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(items.FirstOrDefault()), NewValue = new JavaScriptSerializer().Serialize(item) };
                            db.JDE_Logs.Add(Log);
                            db.Entry(item).State = EntityState.Modified;
                            try
                            {
                                db.SaveChanges();
                                Logger.Info("{methodName}: Edycja zakończona powodzeniem. Przystępuję do edycji Id={id}, UserId={UserId}", methodName, id, UserId);
                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                if (!JDE_StockTakingExists(id))
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

        [HttpPost]
        [Route("CreateStockTaking")]
        [ResponseType(typeof(JDE_StockTakings))]
        public IHttpActionResult CreateStockTaking(string token, JDE_StockTakings item, int UserId)
        {
            string methodName = System.Reflection.MethodInfo.GetCurrentMethod().Name;
            Logger.Info("Start {methodName}. PartId={PartId}, UserId={UserId}", methodName, item.PartId, UserId);
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    try
                    {
                        item.TenantId = tenants.FirstOrDefault().TenantId;
                        item.CreatedOn = DateTime.Now;
                        if(item.TakingDate == null)
                        {
                            item.TakingDate = DateTime.Now;
                        }
                        db.JDE_StockTakings.Add(item);
                        db.SaveChanges();
                        JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Inwentaryzacja części", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, NewValue = new JavaScriptSerializer().Serialize(item) };
                        db.JDE_Logs.Add(Log);
                        db.SaveChanges();
                        Logger.Info("{methodName}: Zapis zakończony powodzeniem. StockTakingId={StockTakingId}, UserId={UserId}", methodName, item.StockTakingId, UserId);
                        return Ok(item);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Błąd w {methodName}. StockTakingId={StockTakingId}, UserId={UserId}. Szczegóły: {Message}", methodName, item.StockTakingId, UserId, ex.ToString());
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
        [Route("DeleteStockTaking")]
        [ResponseType(typeof(JDE_StockTakings))]
        public IHttpActionResult DeleteStockTaking(string token, int id, int UserId)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = db.JDE_StockTakings.Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.StockTakingId == id);
                    if (items.Any())
                    {
                        JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Usunięcie inwentaryzacji", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(items.FirstOrDefault()) };
                        db.JDE_StockTakings.Remove(items.FirstOrDefault());
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

        private bool JDE_StockTakingExists(int id)
        {
            return db.JDE_StockTakings.Count(e => e.StockTakingId == id) > 0;
        }
    }
}
