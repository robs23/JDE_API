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
    public class ProcessActionController : ApiController
    {
        private Models.DbModel db = new Models.DbModel();
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [HttpGet]
        [Route("GetProcessActions")]
        public IHttpActionResult GetProcessActions(string token, int page = 0, int total = 0, string query = null)
        {

            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {

                    var items = (from pa in db.JDE_ProcessActions
                                 join p in db.JDE_Processes on pa.ProcessId equals p.ProcessId into Processes
                                 from prs in Processes.DefaultIfEmpty()
                                 join a in db.JDE_Actions on pa.ActionId equals a.ActionId into Actions
                                 from acs in Actions.DefaultIfEmpty()
                                 join pl in db.JDE_Places on prs.PlaceId equals pl.PlaceId
                                 join u in db.JDE_Users on pa.CreatedBy equals u.UserId
                                 join u2 in db.JDE_Users on pa.LmBy equals u2.UserId into LmByNames
                                 from lms in LmByNames.DefaultIfEmpty()
                                 join t in db.JDE_Tenants on pa.TenantId equals t.TenantId
                                 where pa.TenantId == tenants.FirstOrDefault().TenantId
                                 orderby pa.CreatedOn descending
                                 select new
                                 {
                                     ProcessActionId = pa.ProcessActionId,
                                     ProcessId = pa.ProcessId,
                                     PlannedStart = prs.PlannedStart,
                                     PlannedFinish = prs.PlannedFinish,
                                     StartedOn = prs.StartedOn,
                                     FinishedOn = prs.FinishedOn,
                                     PlaceId = prs.PlaceId,
                                     PlaceName = pl.Name,
                                     ActionId = pa.ActionId,
                                     ActionName = acs.Name,
                                     GivenTime = acs.GivenTime,
                                     Type = acs.Type,
                                     IsChecked = pa.IsChecked,
                                     CreatedBy = u.UserId,
                                     CreatedByName = u.Name + " " + u.Surname,
                                     CreatedOn = pa.CreatedOn,
                                     LmBy = pa.LmBy,
                                     LmByName = lms.Name + " " + lms.Surname,
                                     TenantId = pa.TenantId,
                                     TenantName = t.TenantName,
                                     AssignedUsers = (from pras in db.JDE_ProcessAssigns
                                                        join uu in db.JDE_Users on pras.UserId equals uu.UserId
                                                        where pras.ProcessId == pa.ProcessId
                                                        select uu.Name + " " + uu.Surname),
                                     Handlings = (from hans in db.JDE_Handlings
                                                  join uh in db.JDE_Users on hans.UserId equals uh.UserId
                                                  where hans.HandlingId == pa.HandlingId
                                                  select new
                                                  {
                                                      HandlingId = hans.HandlingId,
                                                      UserName = uh.Name + " " + uh.Surname,
                                                      StartedOn = hans.StartedOn,
                                                      FinishedOn = hans.FinishedOn
                                                  }),
                                      LastChecks = (from pact in db.JDE_ProcessActions
                                                   join h in db.JDE_Handlings on pact.HandlingId equals h.HandlingId into Handlings
                                                   from hs in Handlings.DefaultIfEmpty()
                                                   where pact.ActionId==pa.ActionId && pact.IsChecked==true
                                                   orderby hs.FinishedOn descending
                                                   select hs.FinishedOn).Take(1)
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
        [Route("GetProcessAction")]
        [ResponseType(typeof(JDE_ProcessActions))]
        public IHttpActionResult GetProcessAction(string token, int id)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = (from pa in db.JDE_ProcessActions
                                 join p in db.JDE_Processes on pa.ProcessId equals p.ProcessId into Processes
                                 from prs in Processes.DefaultIfEmpty()
                                 join a in db.JDE_Actions on pa.ActionId equals a.ActionId into Actions
                                 from acs in Actions.DefaultIfEmpty()
                                 join h in db.JDE_Handlings on pa.HandlingId equals h.HandlingId into Handlings
                                 from hs in Handlings.DefaultIfEmpty()
                                 join u in db.JDE_Users on pa.CreatedBy equals u.UserId
                                 join u2 in db.JDE_Users on pa.LmBy equals u2.UserId into LmByNames
                                 from lms in LmByNames.DefaultIfEmpty()
                                 join t in db.JDE_Tenants on pa.TenantId equals t.TenantId
                                 where pa.TenantId == tenants.FirstOrDefault().TenantId && pa.ProcessActionId==id
                                 orderby pa.CreatedOn descending
                                 select new
                                 {
                                     ProcessActionId = pa.ProcessActionId,
                                     ProcessId = pa.ProcessId,
                                     ActionId = pa.ActionId,
                                     ActionName = acs.Name,
                                     PlaceId = prs.PlaceId,
                                     IsChecked = pa.IsChecked,
                                     CreatedBy = u.UserId,
                                     CreatedByName = u.Name + " " + u.Surname,
                                     CreatedOn = pa.CreatedOn,
                                     LmBy = pa.LmBy,
                                     LmByName = lms.Name + " " + lms.Surname,
                                     TenantId = pa.TenantId,
                                     TenantName = t.TenantName,
                                     LastChecks = (from pact in db.JDE_ProcessActions
                                                   join h in db.JDE_Handlings on pact.HandlingId equals h.HandlingId into Handlings
                                                   from hs in Handlings.DefaultIfEmpty()
                                                   where pact.ActionId == pa.ActionId && pact.IsChecked == true
                                                   orderby hs.FinishedOn descending
                                                   select hs.FinishedOn).Take(1)
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
        [Route("EditProcessAction")]
        [ResponseType(typeof(void))]

        public IHttpActionResult EditProcessAction(string token, int id, int UserId, JDE_ProcessActions item)
        {
            Logger.Info("Start EditProcessAction. Id={id}, UserId={UserId}",id, UserId);
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = db.JDE_ProcessActions.AsNoTracking().Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.ProcessActionId == id);
                    if (items.Any())
                    {
                        try
                        {
                            string descr = "Edycja przypisania czynności do zgłoszenia";
                            item.LmOn = DateTime.Now;
                            item.LmBy = UserId;
                            JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = descr, TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(items.FirstOrDefault()), NewValue = new JavaScriptSerializer().Serialize(item) };
                            db.JDE_Logs.Add(Log);
                            db.Entry(item).State = EntityState.Modified;
                            try
                            {
                                db.SaveChanges();
                            }
                            catch (DbUpdateConcurrencyException DbEx)
                            {
                                Logger.Error("Błąd DbUpdateConcurrencyException w EditProcessAction. Id={id}, UserId={UserId}. Wiadomość: {Message}", id, UserId, DbEx.Message);
                                if (!JDE_ProcessActionExists(id))
                                {
                                    return NotFound();
                                }
                                else
                                {
                                    return InternalServerError();
                                }
                            }
                        }
                        catch(Exception ex)
                        {
                            Logger.Error("Błąd w EditProcessAction. Id={id}, UserId={UserId}. Wiadomość: {Message}", id, UserId, ex.Message);
                        }

                    }
                    else
                    {
                        Logger.Info("ProcessAction Id={id} nie zostało znalezione..", id);
                    }
                }
            }
            Logger.Info("Koniec EditProcessAction. Id={id}, UserId={UserId}", id, UserId);
            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpPost]
        [Route("CreateProcessAction")]
        [ResponseType(typeof(JDE_ProcessActions))]
        public IHttpActionResult CreateProcessAction(string token, JDE_ProcessActions item, int UserId)
        {
            Logger.Info("Start CreateProcessAction. UserId={UserId}", UserId);
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    try
                    {
                        item.TenantId = tenants.FirstOrDefault().TenantId;
                        item.CreatedOn = DateTime.Now;
                        db.JDE_ProcessActions.Add(item);
                        db.SaveChanges();
                        JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Utworzenie przypisania czynności do zgłoszenia", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, NewValue = new JavaScriptSerializer().Serialize(item) };
                        db.JDE_Logs.Add(Log);
                        db.SaveChanges();
                        Logger.Info("Koniec CreateProcessAction. UserId={UserId}", UserId);
                        return Ok(item);
                    }catch(Exception ex)
                    {
                        Logger.Error("Błąd w CreateProcessAction. UserId={UserId}. Wiadomość: {Message}", UserId, ex.Message);
                        return InternalServerError();
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
        [Route("DeleteProcessAction")]
        [ResponseType(typeof(JDE_ProcessActions))]
        public IHttpActionResult DeleteProcessAction(string token, int id, int UserId)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = db.JDE_ProcessActions.Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.ProcessActionId == id);
                    if (items.Any())
                    {
                        JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Usunięcie przypisania czynności do zgłoszenia", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(items.FirstOrDefault()) };
                        db.JDE_ProcessActions.Remove(items.FirstOrDefault());
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

        private bool JDE_ProcessActionExists(int id)
        {
            return db.JDE_ProcessActions.Count(e => e.ProcessActionId == id) > 0;
        }
    }
}
