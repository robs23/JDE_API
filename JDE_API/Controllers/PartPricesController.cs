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
    public class PartPricesController : ApiController
    {
        private Models.DbModel db = new Models.DbModel();
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [HttpGet]
        [Route("GetPartPrices")]
        public IHttpActionResult GetPartPrices(string token, int page = 0, int pageSize = 0, int total = 0, string query = null)
        {

            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = (from pp in db.JDE_PartPrices
                                 join p in db.JDE_Parts on pp.PartId equals p.PartId
                                 join u in db.JDE_Users on pp.CreatedBy equals u.UserId
                                 join u2 in db.JDE_Users on pp.LmBy equals u2.UserId into modifiedBy
                                 from mb in modifiedBy.DefaultIfEmpty()
                                 join comp in db.JDE_Companies on p.ProducerId equals comp.CompanyId into producer
                                 from pr in producer.DefaultIfEmpty()
                                 join t in db.JDE_Tenants on pp.TenantId equals t.TenantId
                                 where pp.TenantId == tenants.FirstOrDefault().TenantId
                                 orderby pp.ValidFrom descending
                                 select new
                                 {
                                     PartPriceId = pp.PartPriceId,
                                     PartId = p.PartId,
                                     Name = p.Name,
                                     ProducerId = p.ProducerId,
                                     ProducerName = pr.Name,
                                     Image = p.Image,
                                     Price = pp.Price,
                                     ValidFrom = pp.ValidFrom,
                                     Currency = pp.Currency,
                                     CreatedOn = pp.CreatedOn,
                                     CreatedBy = pp.CreatedBy,
                                     CreatedByName = u.Name + " " + u.Surname,
                                     LmOn = pp.LmOn,
                                     LmBy = pp.LmBy,
                                     LmByName = mb.Name + " " + mb.Surname,
                                     TenantId = pp.TenantId,
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
        [Route("GetPartPrice")]
        [ResponseType(typeof(JDE_PartPrices))]
        public IHttpActionResult GetPartPrice(string token, int id)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = (from pp in db.JDE_PartPrices
                                 join p in db.JDE_Parts on pp.PartId equals p.PartId
                                 join u in db.JDE_Users on pp.CreatedBy equals u.UserId
                                 join u2 in db.JDE_Users on pp.LmBy equals u2.UserId into modifiedBy
                                 from mb in modifiedBy.DefaultIfEmpty()
                                 join comp in db.JDE_Companies on p.ProducerId equals comp.CompanyId into producer
                                 from pr in producer.DefaultIfEmpty()
                                 join t in db.JDE_Tenants on pp.TenantId equals t.TenantId
                                 where pp.TenantId == tenants.FirstOrDefault().TenantId && pp.PartPriceId==id
                                 orderby pp.CreatedOn descending
                                 select new
                                 {
                                     PartPriceId = pp.PartPriceId,
                                     PartId = p.PartId,
                                     Name = p.Name,
                                     ProducerId = p.ProducerId,
                                     ProducerName = pr.Name,
                                     Image = p.Image,
                                     Price = pp.Price,
                                     Currency = pp.Currency,
                                     ValidFrom = pp.ValidFrom,
                                     CreatedOn = pp.CreatedOn,
                                     CreatedBy = pp.CreatedBy,
                                     CreatedByName = u.Name + " " + u.Surname,
                                     LmOn = pp.LmOn,
                                     LmBy = pp.LmBy,
                                     LmByName = mb.Name + " " + mb.Surname,
                                     TenantId = pp.TenantId,
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
        [Route("EditPartPrice")]
        [ResponseType(typeof(void))]

        public IHttpActionResult EditPartPrice(string token, int id, int UserId, JDE_PartPrices item)
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
                        var items = db.JDE_PartPrices.AsNoTracking().Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.PartPriceId == id);
                        if (items.Any())
                        {
                            Logger.Info("{methodName}: Znalazłem odpowiednie PartPrice. Przystępuję do edycji Id={id}, UserId={UserId}", methodName, id, UserId);
                            item.CreatedOn = items.FirstOrDefault().CreatedOn; //switch back to original createdOn date
                            item.LmBy = UserId;
                            item.LmOn = DateTime.Now;
                            newItem = new JavaScriptSerializer().Serialize(item);
                            JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Edycja ceny części", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(items.FirstOrDefault()), NewValue = new JavaScriptSerializer().Serialize(item) };
                            db.JDE_Logs.Add(Log);
                            db.Entry(item).State = EntityState.Modified;
                            try
                            {
                                db.SaveChanges();
                                Logger.Info("{methodName}: Edycja zakończona powodzeniem. Przystępuję do edycji Id={id}, UserId={UserId}", methodName, id, UserId);
                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                if (!JDE_PartPriceExists(id))
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
        [Route("CreatePartPrice")]
        [ResponseType(typeof(JDE_PartPrices))]
        public IHttpActionResult CreatePartPrice(string token, JDE_PartPrices item, int UserId)
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
                        db.JDE_PartPrices.Add(item);
                        db.SaveChanges();
                        JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Nowa cena części", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, NewValue = new JavaScriptSerializer().Serialize(item) };
                        db.JDE_Logs.Add(Log);
                        db.SaveChanges();
                        Logger.Info("{methodName}: Zapis zakończony powodzeniem. PartPriceId={PartPriceId}, UserId={UserId}", methodName, item.PartPriceId, UserId);
                        return Ok(item);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Błąd w {methodName}. PartPriceId={PartPriceId}, UserId={UserId}. Szczegóły: {Message}", methodName, item.PartPriceId, UserId, ex.ToString());
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
        [Route("DeletePartPrice")]
        [ResponseType(typeof(JDE_PartPrices))]
        public IHttpActionResult DeletePartPrice(string token, int id, int UserId)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = db.JDE_PartPrices.Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.PartPriceId == id);
                    if (items.Any())
                    {
                        JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Usunięcie ceny", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(items.FirstOrDefault()) };
                        db.JDE_PartPrices.Remove(items.FirstOrDefault());
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

        private bool JDE_PartPriceExists(int id)
        {
            return db.JDE_PartPrices.Count(e => e.PartPriceId == id) > 0;
        }
    }
}
