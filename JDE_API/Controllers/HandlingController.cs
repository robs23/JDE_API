﻿using JDE_API.Models;
using JDE_API.Static;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Script.Serialization;

namespace JDE_API.Controllers
{
    public class HandlingController : ApiController
    {
        private Models.DbModel db = new Models.DbModel();

        [HttpGet]
        [Route("GetHandlings")]
        public IHttpActionResult GetHandlings(string token, int page = 0, int total = 0, DateTime? dFrom = null, DateTime? dTo = null, string query = null, string length = null)
        {

            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    if(dFrom == null)
                    {
                        if (db.JDE_Handlings.Any())
                        {
                            dFrom = db.JDE_Handlings.Min(x => x.StartedOn).Value;
                        }
                        else
                        {
                            dFrom = new DateTime(2018, 1, 1);
                        }
                    }
                    if (dTo == null)
                    {
                        if (db.JDE_Handlings.Any())
                        {
                            dTo = db.JDE_Handlings.Max(x => x.StartedOn).Value;
                            
                        }
                        else
                        {
                            dTo = new DateTime(2030, 12, 31);
                        }
                    }

                    var items = (from h in db.JDE_Handlings
                                 join u in db.JDE_Users on h.UserId equals u.UserId
                                 join t in db.JDE_Tenants on h.TenantId equals t.TenantId
                                 join p in db.JDE_Processes on h.ProcessId equals p.ProcessId
                                 join at in db.JDE_ActionTypes on p.ActionTypeId equals at.ActionTypeId
                                 join pl in db.JDE_Places on p.PlaceId equals pl.PlaceId
                                 join s in db.JDE_Sets on pl.SetId equals s.SetId
                                 join a in db.JDE_Areas on pl.AreaId equals a.AreaId
                                 where h.TenantId == tenants.FirstOrDefault().TenantId && h.StartedOn >= dFrom && h.StartedOn <= dTo
                                 orderby h.StartedOn descending
                                 select new Handling
                                 {
                                     HandlingId = h.HandlingId,
                                     ProcessId = p.ProcessId,
                                     StartedOn = h.StartedOn,
                                     FinishedOn = h.FinishedOn,
                                     UserId = u.UserId,
                                     UserName = u.Name + " " + u.Surname,
                                     ActionTypeId = p.ActionTypeId,
                                     ActionTypeName = at.Name,
                                     IsActive = h.IsActive,
                                     IsFrozen = h.IsFrozen,
                                     IsCompleted = h.IsCompleted,
                                     PlaceId = p.PlaceId,
                                     PlaceName = pl.Name,
                                     SetId = pl.SetId,
                                     SetName = s.Name,
                                     AreaId = pl.AreaId,
                                     AreaName = a.Name,
                                     Output = p.Output,
                                     TenantId = p.TenantId,
                                     TenantName = t.TenantName
                                 });
                    if (items.Any())
                    {
                        if (query != null)
                        {
                            items = items.Where(query);
                        }

                        if (length != null)
                        {
                            var nItems = items.ToList();
                            nItems = FilterByLength(nItems, length);
                            if (total == 0 && page > 0)
                            {
                                int pageSize = RuntimeSettings.PageSize;
                                var skip = pageSize * (page - 1);
                                if (skip < nItems.Count())
                                {
                                    nItems = nItems.Skip(skip).Take(pageSize).ToList();
                                    return Ok(nItems);
                                }
                                else
                                {
                                    return NotFound();
                                }
                            }
                            else if (total > 0 && page == 0)
                            {
                                nItems = nItems.Take(total).ToList();
                                return Ok(nItems);
                            }
                            else
                            {
                                return Ok(nItems);
                            }
                        }
                        else
                        {
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
        [Route("GetHandling")]
        [ResponseType(typeof(JDE_Handlings))]
        public IHttpActionResult GetHandling(string token, int id)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = (from h in db.JDE_Handlings
                                 join u in db.JDE_Users on h.UserId equals u.UserId
                                 join t in db.JDE_Tenants on h.TenantId equals t.TenantId
                                 join p in db.JDE_Processes on h.ProcessId equals p.ProcessId
                                 join at in db.JDE_ActionTypes on p.ActionTypeId equals at.ActionTypeId
                                 join pl in db.JDE_Places on p.PlaceId equals pl.PlaceId
                                 join s in db.JDE_Sets on pl.SetId equals s.SetId
                                 join a in db.JDE_Areas on pl.AreaId equals a.AreaId
                                 where h.TenantId == tenants.FirstOrDefault().TenantId && h.HandlingId == id
                                 select new Handling
                                 {
                                     HandlingId = h.HandlingId,
                                     ProcessId = p.ProcessId,
                                     StartedOn = h.StartedOn,
                                     FinishedOn = h.FinishedOn,
                                     UserId = u.UserId,
                                     UserName = u.Name + " " + u.Surname,
                                     ActionTypeId = p.ActionTypeId,
                                     ActionTypeName = at.Name,
                                     IsActive = h.IsActive,
                                     IsFrozen = h.IsFrozen,
                                     IsCompleted = h.IsCompleted,
                                     PlaceId = p.PlaceId,
                                     PlaceName = pl.Name,
                                     SetId = pl.SetId,
                                     SetName = s.Name,
                                     AreaId = pl.AreaId,
                                     AreaName = a.Name,
                                     Output = p.Output,
                                     TenantId = p.TenantId,
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
        [Route("EditHandling")]
        [ResponseType(typeof(void))]

        public IHttpActionResult EditHandling(string token, int id, int UserId, JDE_Handlings item)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = db.JDE_Handlings.AsNoTracking().Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.HandlingId == id);
                    if (items.Any())
                    {
                        if (items.FirstOrDefault().StartedOn != null)
                        {
                            item.StartedOn = items.FirstOrDefault().StartedOn;
                        }
                        string descr = "Edycja obsługi";
                        if (items.FirstOrDefault().FinishedOn == null && item.FinishedOn != null)
                        {
                            //this has just been finished. Replace user's finish time with server time
                            item.FinishedOn = DateTime.Now;
                            descr = "Zakończenie obsługi";
                        }
                        JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = descr, TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(items.FirstOrDefault()), NewValue = new JavaScriptSerializer().Serialize(item) };
                        db.JDE_Logs.Add(Log);
                        db.Entry(item).State = EntityState.Modified;
                        try
                        {
                            db.SaveChanges();
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            if (!JDE_HandlingsExists(id))
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
        [Route("CreateHandling")]
        [ResponseType(typeof(JDE_Processes))]
        public IHttpActionResult CreateHandling(string token, JDE_Handlings item, int UserId)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    item.TenantId = tenants.FirstOrDefault().TenantId;
                    item.StartedOn = DateTime.Now;
                    db.JDE_Handlings.Add(item);
                    db.SaveChanges();
                    JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Rozpoczęcie obsługi", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, NewValue = new JavaScriptSerializer().Serialize(item) };
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
        [Route("DeleteHandling")]
        [ResponseType(typeof(JDE_Handlings))]
        public IHttpActionResult DeleteHandling(string token, int id, int UserId)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = db.JDE_Handlings.Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.HandlingId == id);
                    if (items.Any())
                    {
                        JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Usunięcie obsługi", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(items.FirstOrDefault()) };
                        db.JDE_Handlings.Remove(items.FirstOrDefault());
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

        private List<Handling> FilterByLength(List<Handling> nItems, string length)
        {
            var min = Regex.Match(length, @"\d+").Value;
            int mins = 0;
            int.TryParse(min, out mins);
            var sign = length.Substring(0, length.Length - min.Length);
            if ((sign.Equals(">") || sign.Equals("<") || sign.Equals("=<") || sign.Equals("<=") || sign.Equals("=>") || sign.Equals(">=") || sign.Equals("=")) && mins >= 0)
            {
                // don't do anything unless you've got both min and sign
                if (sign.Equals("="))
                {
                    nItems = nItems.Where(i => i.Length == mins).ToList();
                }
                else if (sign.Equals("<=") || sign.Equals("=<"))
                {
                    nItems = nItems.Where(i => i.Length <= mins).ToList();
                }
                else if (sign.Equals(">=") || sign.Equals("=>"))
                {
                    nItems = nItems.Where(i => i.Length >= mins).ToList();
                }
                else if (sign.Equals(">"))
                {
                    nItems = nItems.Where(i => i.Length > mins).ToList();
                }
                else if (sign.Equals("<"))
                {
                    nItems = nItems.Where(i => i.Length < mins).ToList();
                }

            }
            return nItems;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool JDE_HandlingsExists(int id)
        {
            return db.JDE_Handlings.Count(e => e.HandlingId == id) > 0;
        }
    }

    public class Handling
    {
        public int HandlingId { get; set; }
        public int ProcessId { get; set; }
        public int? PlaceId { get; set; }
        public string PlaceName { get; set; }
        public int? SetId { get; set; }
        public string SetName { get; set; }
        public int? AreaId { get; set; }
        public string AreaName { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public DateTime? StartedOn { get; set; }
        public DateTime? FinishedOn { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsFrozen { get; set; }
        public bool? IsCompleted { get; set; }
        public string Output { get; set; }
        public int? TenantId { get; set; }
        public string TenantName { get; set; }
        public int? ActionTypeId { get; set; }
        public string ActionTypeName { get; set; }
        public int? Length
        {
            get
            {
                if (StartedOn == null)
                {
                    return null;
                }
                else
                {
                    if (FinishedOn == null)
                    {
                        return (int)DateTime.Now.Subtract((DateTime)StartedOn).TotalMinutes;
                    }
                    else
                    {
                        return (int)((DateTime)FinishedOn).Subtract((DateTime)StartedOn).TotalMinutes;
                    }
                }
            }
        }
    }
}