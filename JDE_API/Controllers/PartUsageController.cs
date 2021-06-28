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
    public class PartUsageController : ApiController
    {
        private Models.DbModel db = new Models.DbModel();
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

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
                                 join pl in db.JDE_Places on pu.PlaceId equals pl.PlaceId into place
                                 from pla in place.DefaultIfEmpty()
                                 join u in db.JDE_Users on pu.CreatedBy equals u.UserId
                                 join u2 in db.JDE_Users on pu.LmBy equals u2.UserId into modifiedBy
                                 from mb in modifiedBy.DefaultIfEmpty()
                                 join comp in db.JDE_Companies on p.ProducerId equals comp.CompanyId into producer
                                 from pr in producer.DefaultIfEmpty()
                                 join stbin in db.JDE_StorageBins on pu.StorageBinId equals stbin.StorageBinId into storagebin
                                 from sbin in storagebin.DefaultIfEmpty()
                                 join t in db.JDE_Tenants on pu.TenantId equals t.TenantId
                                 where pu.TenantId == tenants.FirstOrDefault().TenantId
                                 orderby pu.CreatedOn descending
                                 select new
                                 {
                                     PartUsageId = pu.PartUsageId,
                                     PartId = p.PartId,
                                     Name = p.Name,
                                     PlaceId = pu.PlaceId,
                                     PlaceName = pla.Name,
                                     ProcessId = pu.ProcessId,
                                     ProducerId = pu.ProcessId,
                                     ProducerName = pr.Name,
                                     Symbol = p.Symbol,
                                     Description = p.Description,
                                     Comment = pu.Comment,
                                     Image = p.Image,
                                     Amount = pu.Amount,
                                     StorageBinId = pu.StorageBinId,
                                     StorageBinNumber = sbin.Number,
                                     CreatedOn = pu.CreatedOn,
                                     CreatedBy = pu.CreatedBy,
                                     CreatedByName = u.Name + " " + u.Surname,
                                     LmOn = pu.LmOn,
                                     LmBy = pu.LmBy,
                                     LmByName = mb.Name + " " + mb.Surname,
                                     TenantId = pu.TenantId,
                                     TenantName = t.TenantName,
                                     Cost = db.JDE_PartPrices.Where(pp => pp.ValidFrom <= pu.CreatedOn && pp.PartId == pu.PartId).OrderByDescending(pp => pp.ValidFrom).FirstOrDefault() == null ? null : (double)db.JDE_PartPrices.Where(pp => pp.ValidFrom <= pu.CreatedOn && pp.PartId == pu.PartId).OrderByDescending(pp => pp.ValidFrom).FirstOrDefault().Price * pu.Amount,
                                     CostCurrency = db.JDE_PartPrices.Where(pp => pp.ValidFrom <= pu.CreatedOn && pp.PartId == pu.PartId).OrderByDescending(pp => pp.ValidFrom).FirstOrDefault() == null ? null : db.JDE_PartPrices.Where(pp => pp.ValidFrom <= pu.CreatedOn && pp.PartId == pu.PartId).OrderByDescending(pp => pp.ValidFrom).FirstOrDefault().Currency
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
                                 join pl in db.JDE_Places on pu.PlaceId equals pl.PlaceId into place
                                 from pla in place.DefaultIfEmpty()
                                 join u in db.JDE_Users on pu.CreatedBy equals u.UserId
                                 join u2 in db.JDE_Users on pu.LmBy equals u2.UserId into modifiedBy
                                 from mb in modifiedBy.DefaultIfEmpty()
                                 join comp in db.JDE_Companies on p.ProducerId equals comp.CompanyId into producer
                                 from pr in producer.DefaultIfEmpty()
                                 join stbin in db.JDE_StorageBins on pu.StorageBinId equals stbin.StorageBinId into storagebin
                                 from sbin in storagebin.DefaultIfEmpty()
                                 join t in db.JDE_Tenants on pu.TenantId equals t.TenantId
                                 where pu.TenantId == tenants.FirstOrDefault().TenantId && pu.PartUsageId == id
                                 orderby pu.CreatedOn descending
                                 select new
                                 {
                                     PartUsageId = pu.PartUsageId,
                                     PartId = p.PartId,
                                     Name = p.Name,
                                     PlaceId = pu.PlaceId,
                                     PlaceName = pla.Name,
                                     ProcessId = pu.ProcessId,
                                     ProducerId = pu.ProcessId,
                                     ProducerName = pr.Name,
                                     Symbol = p.Symbol,
                                     Description = p.Description,
                                     Commnent = pu.Comment,
                                     Image = p.Image,
                                     Amount = pu.Amount,
                                     StorageBinId = pu.StorageBinId,
                                     StorageBinNumber = sbin.Number,
                                     CreatedOn = pu.CreatedOn,
                                     CreatedBy = pu.CreatedBy,
                                     CreatedByName = u.Name + " " + u.Surname,
                                     LmOn = pu.LmOn,
                                     LmBy = pu.LmBy,
                                     LmByName = mb.Name + " " + mb.Surname,
                                     TenantId = pu.TenantId,
                                     TenantName = t.TenantName,
                                     Cost = db.JDE_PartPrices.Where(pp => pp.ValidFrom <= pu.CreatedOn &&  pp.PartId == pu.PartId).OrderByDescending(pp => pp.ValidFrom).FirstOrDefault() == null ? null : (double)db.JDE_PartPrices.Where(pp => pp.ValidFrom <= pu.CreatedOn && pp.PartId == pu.PartId).OrderByDescending(pp => pp.ValidFrom).FirstOrDefault().Price * pu.Amount,
                                     CostCurrency = db.JDE_PartPrices.Where(pp => pp.ValidFrom <= pu.CreatedOn && pp.PartId == pu.PartId).OrderByDescending(pp => pp.ValidFrom).FirstOrDefault() == null ? null : db.JDE_PartPrices.Where(pp => pp.ValidFrom <= pu.CreatedOn && pp.PartId == pu.PartId).OrderByDescending(pp => pp.ValidFrom).FirstOrDefault().Currency
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
            Logger.Info("Start EditPartUsage. Id={id}, UserId={UserId}", id, UserId);
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    string newItem = "";
                    try
                    {
                        var items = db.JDE_PartUsages.AsNoTracking().Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.PartUsageId == id);
                        if (items.Any())
                        {
                            Logger.Info("EditPartUsage: Znalazłem odpowiednie PartUsage. Przystępuję do edycji Id={id}, UserId={UserId}", id, UserId);
                            item.CreatedOn = items.FirstOrDefault().CreatedOn; //switch back to original createdOn date
                            item.LmBy = UserId;
                            item.LmOn = DateTime.Now;
                            newItem = new JavaScriptSerializer().Serialize(item);
                            JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Edycja zużycia części", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(items.FirstOrDefault()), NewValue = new JavaScriptSerializer().Serialize(item) };
                            db.JDE_Logs.Add(Log);
                            db.Entry(item).State = EntityState.Modified;
                            try
                            {
                                db.SaveChanges();
                                Logger.Info("EditPartUsage: Edycja zakończona powodzeniem. Przystępuję do edycji Id={id}, UserId={UserId}", id, UserId);
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
                    catch (Exception ex)
                    {
                        Logger.Error("Błąd w EditPartUsage. Id={id}, UserId={UserId}. Szczegóły: {Message}, nowa wartość: {newItem}", id, UserId, ex.ToString(), newItem);
                        return StatusCode(HttpStatusCode.InternalServerError);
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
            Logger.Info("Start CreatePartUsage. PartId={PartId}, UserId={UserId}", item.PartId, UserId);
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    try
                    {
                        item.TenantId = tenants.FirstOrDefault().TenantId;
                        item.CreatedOn = DateTime.Now;
                        db.JDE_PartUsages.Add(item);
                        db.SaveChanges();
                        JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Zużycie części", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, NewValue = new JavaScriptSerializer().Serialize(item) };
                        db.JDE_Logs.Add(Log);
                        db.SaveChanges();
                        Logger.Info("CreatePartUsage: Zapis zakończony powodzeniem. PartId={PartId}, UserId={UserId}", item.PartId, UserId);
                        return Ok(item);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Błąd w CreatePartUsage. PartId={PartId}, UserId={UserId}. Szczegóły: {Message}", item.PartId, UserId, ex.ToString());
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
