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
        public IHttpActionResult GetLogs(string token, int page = 0, int total = 0, DateTime? dFrom = null, DateTime? dTo = null, string query = null, string id = null)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    dFrom = dFrom ?? db.JDE_Logs.Min(x => x.Timestamp).Value;
                    dTo = dTo ?? db.JDE_Logs.Max(x => x.Timestamp).Value;

                    var items = (from l in db.JDE_Logs
                                 join u in db.JDE_Users on l.UserId equals u.UserId
                                 join t in db.JDE_Tenants on l.TenantId equals t.TenantId
                                 where l.TenantId == tenants.FirstOrDefault().TenantId && l.Timestamp >= dFrom && l.Timestamp <= dTo
                                 orderby l.Timestamp descending
                                 select new ExtLog
                                 {
                                     LogId = l.LogId,
                                     TimeStamp = l.Timestamp,
                                     TenantId = t.TenantId,
                                     TenantName = t.TenantName,
                                     UserId = l.UserId,
                                     UserName = u.Name + " " + u.Surname,
                                     Description = l.Description,
                                     OldValue = l.OldValue,
                                     NewValue = l.NewValue
                                 });
                    if (items.Any())
                    {
                        if (id != null)
                        {
                            items = items.Where(i => i.NewValue.Contains(id) || i.OldValue.Contains(id));
                        }

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

    public class ExtLog
    {
        public int LogId { get; set; }
        public DateTime? TimeStamp { get; set; }
        public int TenantId { get; set; }
        public string TenantName { get; set; }
        public int? UserId { get; set; }
        public string UserName { get; set; }
        public string Description { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public string ExtDescription { get
            {
                 Models.DbModel db = new Models.DbModel();

                string res = "";

                if (this.Description.Equals("Utworzenie zgłoszenia"))
                {
                    try
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        dynamic p = js.DeserializeObject(this.NewValue);
                        int processId = p["ProcessId"];
                        int at = p["ActionTypeId"];
                        int pl = p["PlaceId"];
                        res = string.Format("Nr zgłoszenia: {0}, Typ: {1}, Zasób: {2}", processId, db.JDE_ActionTypes.Where(t => t.ActionTypeId == at).FirstOrDefault().Name, db.JDE_Places.Where(pla => pla.PlaceId == pl).FirstOrDefault().Name);
                    }
                    catch (Exception ex)
                    {
                        res = "Deserializacja nie powiodła się";
                    }

                }
                else if (this.Description.Equals("Edycja zgłoszenia"))
                {
                    try
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        dynamic nv = js.DeserializeObject(this.NewValue);
                        dynamic ov = js.DeserializeObject(this.OldValue);
                        int processId = nv["ProcessId"];
                        int at = nv["ActionTypeId"];
                        int pl = nv["PlaceId"];
                        string comment = "";
                        if (((bool)nv["IsCompleted"] || (bool)nv["IsSuccessfull"]) && ((bool)ov["IsCompleted"] == false && (bool)ov["IsSuccessfull"] == false))
                        {
                            comment = "Zgłoszenie zostało zamknięte";
                        }
                        else if ((bool)nv["IsFrozen"] && (bool)ov["IsFrozen"] == false)
                        {
                            comment = "Zgłoszenie zostało wstrzymane";
                        }
                        else if ((bool)nv["IsFrozen"] == false && (bool)ov["IsFrozen"])
                        {
                            comment = "Zgłoszenie zostało wznowione";
                        }
                        string initDiag = "";
                        string ar = "";
                        try
                        {
                            initDiag = nv["InitialDiagnosis"];
                            ar = nv["RepairActions"];
                        }
                        catch (Exception ex)
                        {

                        }
                        if (string.IsNullOrEmpty(initDiag) || string.IsNullOrEmpty(ar))
                        {
                            res = string.Format("Nr zgłoszenia: {0}, Typ: {1}, Zasób: {2}, Wstępne rozpoznanie: {3}, Czynności naprawcze: {4}, {5}", processId, db.JDE_ActionTypes.Where(t => t.ActionTypeId == at).FirstOrDefault().Name, db.JDE_Places.Where(pla => pla.PlaceId == pl).FirstOrDefault().Name, initDiag, ar, comment);
                        }
                        else
                        {
                            res = string.Format("Nr zgłoszenia: {0}, Typ: {1}, Zasób: {2}, Rezultat: {3}, {4}", processId, db.JDE_ActionTypes.Where(t => t.ActionTypeId == at).FirstOrDefault().Name, db.JDE_Places.Where(pla => pla.PlaceId == pl).FirstOrDefault().Name, nv["Output"], comment);
                        }
                    }
                    catch (Exception ex)
                    {
                        res = "Deserializacja nie powiodła się";
                    }
                }
                else if (this.Description.Equals("Zamknięcie zgłoszenia"))
                {
                    try
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        dynamic nv = js.DeserializeObject(this.NewValue);
                        dynamic ov = js.DeserializeObject(this.OldValue);
                        int processId = nv["ProcessId"];
                        int at = nv["ActionTypeId"];
                        int pl = nv["PlaceId"];
                        string initDiag = "";
                        string ar = "";
                        try
                        {
                            initDiag = nv["InitialDiagnosis"];
                            ar = nv["RepairActions"];
                        }catch(Exception ex)
                        {

                        }
                        if(initDiag.Length > 0 || ar.Length > 0)
                        {
                            res = string.Format("Nr zgłoszenia: {0}, Typ: {1}, Zasób: {2}, Wstępne rozpoznanie: {3}, Czynności naprawcze: {4}", processId, db.JDE_ActionTypes.Where(t => t.ActionTypeId == at).FirstOrDefault().Name, db.JDE_Places.Where(pla => pla.PlaceId == pl).FirstOrDefault().Name, initDiag, ar);
                        }
                        else
                        {
                            res = string.Format("Nr zgłoszenia: {0}, Typ: {1}, Zasób: {2}, Rezultat: {3}", processId, db.JDE_ActionTypes.Where(t => t.ActionTypeId == at).FirstOrDefault().Name, db.JDE_Places.Where(pla => pla.PlaceId == pl).FirstOrDefault().Name, nv["Output"]);
                        }
                    }
                    catch (Exception ex)
                    {
                        res = "Deserializacja nie powiodła się";
                    }
                }
                else if (this.Description.Equals("Usunięcie zgłoszenia"))
                {
                    try
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        dynamic ov = js.DeserializeObject(this.OldValue);
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
                else if (this.Description.Equals("Utworzenie użytkownika") || this.Description.Equals("Edycja użytkownika"))
                {
                    try
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        dynamic nv = js.DeserializeObject(this.NewValue);
                        res = "ID użytkownika: " + nv["UserId"] + ", imię i nazwisko: " + nv["Name"] + " " + nv["Surname"];
                    }
                    catch (Exception ex)
                    {
                        res = "Deserializacja nie powiodła się";
                    }
                }
                else if (this.Description.Equals("Usunięcie użytkownika"))
                {
                    try
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        dynamic nv = js.DeserializeObject(this.OldValue);
                        res = "ID użytkownika: " + nv["UserId"] + ", imię i nazwisko: " + nv["Name"] + " " + nv["Surname"];
                    }
                    catch (Exception ex)
                    {
                        res = "Deserializacja nie powiodła się";
                    }
                }
                else if (this.Description.Equals("Utworzenie zasobu") || this.Description.Equals("Edycja zasobu"))
                {
                    try
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        dynamic nv = js.DeserializeObject(this.NewValue);
                        int setId = nv["SetId"];
                        int areaId = nv["AreaId"];
                        res = "ID zasobu: " + nv["PlaceId"] + ", nazwa: " + nv["Name"] + ", instalacja: " + db.JDE_Sets.Where(s => s.SetId == setId).FirstOrDefault().Name + ", obszar: " + db.JDE_Areas.Where(a => a.AreaId == areaId).FirstOrDefault().Name;
                    }
                    catch (Exception ex)
                    {
                        res = "Deserializacja nie powiodła się";
                    }
                }
                else if (this.Description.Equals("Usunięcie zasobu"))
                {
                    try
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        dynamic nv = js.DeserializeObject(this.OldValue);
                        int setId = nv["SetId"];
                        int areaId = nv["AreaId"];
                        res = "ID zasobu: " + nv["PlaceId"] + ", nazwa: " + nv["Name"] + ", instalacja: " + db.JDE_Sets.Where(s => s.SetId == setId).FirstOrDefault().Name + ", obszar: " + db.JDE_Areas.Where(a => a.AreaId == areaId).FirstOrDefault().Name;
                    }
                    catch (Exception ex)
                    {
                        res = "Deserializacja nie powiodła się";
                    }
                }


                return res;
            } }
    }
}