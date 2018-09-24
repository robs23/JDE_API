using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Script.Serialization;
using JDE_API.Models;
using JDE_API.Static;
using System.Linq.Dynamic;

namespace JDE_API.Controllers
{
    public class LogController : ApiController
    {
        private Models.DbModel db = new Models.DbModel();

        [HttpGet]
        [Route("GetLogs")]
        public IHttpActionResult GetLogs(string token, int page=0, int total=0)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = (from l in db.JDE_Logs
                                 join u in db.JDE_Users on l.UserId equals u.UserId
                                 join t in db.JDE_Tenants on l.TenantId equals t.TenantId
                                 where l.TenantId == tenants.FirstOrDefault().TenantId
                                 orderby l.Timestamp descending
                                 select new
                                 {
                                     LogId = l.LogId,
                                     Timestamp = l.Timestamp,
                                     TenantId = t.TenantId,
                                     TenantName = t.TenantName,
                                     UserId = l.UserId,
                                     UserName = u.Name + " " + u.Surname,
                                     Descripiton = l.Description,
                                     OldValue = l.OldValue,
                                     NewValue = l.NewValue
                                 });
                    if (items.Any())
                    {

                        if(total==0 && page > 0)
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
                        }else if(total>0 && page == 0)
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
        [Route("GetLogsExt")]
        public IHttpActionResult GetLogsExt(string token, int page = 0, int total = 0, DateTime? dFrom = null, DateTime? dTo = null, string query = null)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = (from l in db.JDE_Logs
                                 join u in db.JDE_Users on l.UserId equals u.UserId
                                 join t in db.JDE_Tenants on l.TenantId equals t.TenantId
                                 where l.TenantId == tenants.FirstOrDefault().TenantId && l.Timestamp >= dFrom && l.Timestamp <= dTo
                                 orderby l.Timestamp descending
                                 select new
                                 {
                                     LogId = l.LogId,
                                     Timestamp = l.Timestamp,
                                     TenantId = t.TenantId,
                                     TenantName = t.TenantName,
                                     UserId = l.UserId,
                                     UserName = u.Name + " " + u.Surname,
                                     Descripiton = l.Description,
                                     OldValue = l.OldValue,
                                     NewValue = l.NewValue
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
                                List<object> nItems = new List<object>();
                                int ind = 0;
                                //try to get more details
                                foreach(var i in items)
                                {
                                    string desc = GetLogDescription(i.NewValue, i.OldValue, i.Descripiton);
                                    var nItem = new
                                    {
                                        LogId = i.LogId,
                                        Timestamp = i.Timestamp,
                                        TenantId = i.TenantId,
                                        TenantName = i.TenantName,
                                        UserId = i.UserId,
                                        UserName = i.UserName,
                                        Descripiton = i.Descripiton,
                                        OldValue = i.OldValue,
                                        NewValue = i.NewValue,
                                        ExtDescription = desc
                                    };
                                    nItems.Add(nItem);
                                }
                                return Ok(nItems);
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

        private string GetLogDescription(string newValue, string oldValue, string type)
        {
            string res = "";

            if (type.Equals("Utworzenie zgłoszenia"))
            {
                try
                {
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    dynamic p = js.DeserializeObject(newValue);
                    int processId = p["ProcessId"];
                    int at = p["ActionTypeId"];
                    int pl = p["PlaceId"];
                    res = string.Format("Nr zgłoszenia: {0}, Typ: {1}, Zasób: {2}", processId, db.JDE_ActionTypes.Where(t => t.ActionTypeId == at).FirstOrDefault().Name, db.JDE_Places.Where(pla => pla.PlaceId == pl).FirstOrDefault().Name);
                }
                catch (Exception ex)
                {
                    res = "Deserializacja nie powiodła się";
                }

            }else if(type.Equals("Edycja zgłoszenia"))
            {
                try
                {
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    dynamic nv = js.DeserializeObject(newValue);
                    dynamic ov = js.DeserializeObject(oldValue);
                    int processId = nv["ProcessId"];
                    int at = nv["ActionTypeId"];
                    int pl = nv["PlaceId"];
                    string comment = "";
                    if(((bool)nv["IsCompleted"] || (bool)nv["IsSuccessfull"]) && ((bool)ov["IsCompleted"]==false && (bool)ov["IsSuccessfull"]==false))
                    {
                        comment = "Zgłoszenie zostało zamknięte";
                    }else if((bool)nv["IsFrozen"] && (bool)ov["IsFrozen"] == false)
                    {
                        comment = "Zgłoszenie zostało wstrzymane";
                    }
                    else if ((bool)nv["IsFrozen"]==false && (bool)ov["IsFrozen"])
                    {
                        comment = "Zgłoszenie zostało wznowione";
                    }
                    res = string.Format("Nr zgłoszenia: {0}, Typ: {1}, Zasób: {2}, Rezultat: {3}, {4}",processId, db.JDE_ActionTypes.Where(t => t.ActionTypeId == at).FirstOrDefault().Name, db.JDE_Places.Where(pla => pla.PlaceId == pl).FirstOrDefault().Name,nv["Output"], comment);
                }
                catch (Exception ex)
                {
                    res = "Deserializacja nie powiodła się";
                }
            }
            else if (type.Equals("Zamknięcie zgłoszenia"))
            {
                try
                {
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    dynamic nv = js.DeserializeObject(newValue);
                    dynamic ov = js.DeserializeObject(oldValue);
                    int processId = nv["ProcessId"];
                    int at = nv["ActionTypeId"];
                    int pl = nv["PlaceId"];
                    res = string.Format("Nr zgłoszenia: {0}, Typ: {1}, Zasób: {2}, Rezultat: {3}", processId, db.JDE_ActionTypes.Where(t => t.ActionTypeId == at).FirstOrDefault().Name, db.JDE_Places.Where(pla => pla.PlaceId == pl).FirstOrDefault().Name, nv["Output"]);
                }
                catch (Exception ex)
                {
                    res = "Deserializacja nie powiodła się";
                }
            }
            else if (type.Equals("Usunięcie zgłoszenia"))
            {
                try
                {
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    dynamic ov = js.DeserializeObject(oldValue);
                    int processId = ov["ProcessId"];
                    int at = ov["ActionTypeId"];
                    int pl = ov["PlaceId"];
                    res = string.Format("Nr zgłoszenia: {0}, Typ: {1}, Zasób: {2}, Rezultat: {3}", processId, db.JDE_ActionTypes.Where(t => t.ActionTypeId == at).FirstOrDefault().Name, db.JDE_Places.Where(pla => pla.PlaceId == pl).FirstOrDefault().Name, ov["Output"]);
                }
                catch (Exception ex)
                {
                    res = "Deserializacja nie powiodła się";
                }
            }

            return res;
        }

        [HttpGet]
        [Route("GetLog")]
        public IHttpActionResult GetLog(string token, int id)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = (from l in db.JDE_Logs
                                 join u in db.JDE_Users on l.UserId equals u.UserId
                                 join t in db.JDE_Tenants on l.TenantId equals t.TenantId
                                 where l.TenantId == tenants.FirstOrDefault().TenantId && l.LogId==id
                                 select new
                                 {
                                     LogId = l.LogId,
                                     Timestamp = l.Timestamp,
                                     TenantId = t.TenantId,
                                     TenantName = t.TenantName,
                                     UserId = l.UserId,
                                     UserName = u.Name + " " + u.Surname,
                                     Descripiton = l.Description,
                                     OldValue = l.OldValue,
                                     NewValue = l.NewValue
                                 });

                    if (items.Any())
                    {
                        return Ok(items);
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

        [HttpPost]
        [Route("CreateLog")]
        [ResponseType(typeof(JDE_Logs))]
        public IHttpActionResult CreateLog(string token, JDE_Logs item)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    item.TenantId = tenants.FirstOrDefault().TenantId;
                    db.JDE_Logs.Add(item);
                    db.SaveChanges();
                    return Ok(item);
                    //return CreatedAtRoute("DefaultApi", new { id = item.ProcessId }, item);
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
        [Route("DeleteLog")]
        [ResponseType(typeof(JDE_Logs))]
        public IHttpActionResult DeleteLog(string token, int id)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = db.JDE_Logs.Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.LogId == id);
                    if (items.Any())
                    {
                        db.JDE_Logs.Remove(items.FirstOrDefault());
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

        private bool JDE_LogsExists(int id)
        {
            return db.JDE_Logs.Count(e => e.LogId == id) > 0;
        }
    }
}