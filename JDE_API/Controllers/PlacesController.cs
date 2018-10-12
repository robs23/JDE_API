using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Script.Serialization;
using JDE_API.Models;
using JDE_API.Static;

namespace JDE_API.Controllers
{
    public class PlacesController : ApiController
    {
        private Models.DbModel db = new Models.DbModel();

        [HttpGet]
        [Route("GetPlaces")]
        public IHttpActionResult GetPlaces(string token, int page = 0, int total=0)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = (from pl in db.JDE_Places
                                  join st in db.JDE_Sets on pl.SetId equals st.SetId
                                  join ar in db.JDE_Areas on pl.AreaId equals ar.AreaId
                                  join us in db.JDE_Users on pl.CreatedBy equals us.UserId
                                  join t in db.JDE_Tenants on pl.TenantId equals t.TenantId
                                  where t.TenantId == tenants.FirstOrDefault().TenantId
                                  orderby pl.CreatedOn descending
                                  select new
                                  {
                                      PlaceId = pl.PlaceId,
                                      Number1 = pl.Number1,
                                      Number2 = pl.Number2,
                                      Name = pl.Name,
                                      Description = pl.Description,
                                      AreaId = ar.AreaId,
                                      AreaName = ar.Name,
                                      SetId = st.SetId,
                                      SetName = st.Name,
                                      Priority = pl.Priority,
                                      CreatedOn = pl.CreatedOn,
                                      CreatedBy = us.UserId,
                                      CreatedByName = us.Name + " " + us.Surname,
                                      TenantId = t.TenantId,
                                      TenantName = t.TenantName,
                                      PlaceToken = pl.PlaceToken
                                  }
                          );
                    if (items.Any())
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
        [Route("GetPlace")]
        public IHttpActionResult GetPlace(string token, int id)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var place = (from pl in db.JDE_Places
                                 join st in db.JDE_Sets on pl.SetId equals st.SetId
                                 join ar in db.JDE_Areas on pl.AreaId equals ar.AreaId
                                 join us in db.JDE_Users on pl.CreatedBy equals us.UserId
                                 join t in db.JDE_Tenants on pl.TenantId equals t.TenantId
                                 where pl.PlaceId == id && t.TenantId == tenants.FirstOrDefault().TenantId
                                 select new
                                 {
                                     PlaceId = pl.PlaceId,
                                     Number1 = pl.Number1,
                                     Number2 = pl.Number2,
                                     Name = pl.Name,
                                     Description = pl.Description,
                                     AreaId = ar.AreaId,
                                     AreaName = ar.Name,
                                     SetId = st.SetId,
                                     SetName = st.Name,
                                     Priority = pl.Priority,
                                     CreatedOn = pl.CreatedOn,
                                     CreatedBy = us.UserId,
                                     CreatedByName = us.Name + " " + us.Surname,
                                     TenantId = t.TenantId,
                                     TenantName = t.TenantName,
                                     PlaceToken = pl.PlaceToken
                                 }
                          ).Take(1);
                    if (!place.Any())
                    {
                        return NotFound();
                    }

                    return Ok(place.FirstOrDefault());
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
        [Route("GetPlace")]
        public IHttpActionResult GetPlace(string token, string placeToken)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var place = (from pl in db.JDE_Places
                                 join st in db.JDE_Sets on pl.SetId equals st.SetId
                                 join ar in db.JDE_Areas on pl.AreaId equals ar.AreaId
                                 join us in db.JDE_Users on pl.CreatedBy equals us.UserId
                                 join t in db.JDE_Tenants on pl.TenantId equals t.TenantId
                                 where pl.PlaceToken == placeToken && t.TenantId == tenants.FirstOrDefault().TenantId
                                 select new
                                 {
                                     PlaceId = pl.PlaceId,
                                     Number1 = pl.Number1,
                                     Number2 = pl.Number2,
                                     Name = pl.Name,
                                     Description = pl.Description,
                                     AreaId = ar.AreaId,
                                     AreaName = ar.Name,
                                     SetId = st.SetId,
                                     SetName = st.Name,
                                     Priority = pl.Priority,
                                     CreatedOn = pl.CreatedOn,
                                     CreatedBy = us.UserId,
                                     CreatedByName = us.Name + " " + us.Surname,
                                     TenantId = t.TenantId,
                                     TenantName = t.TenantName,
                                     PlaceToken = pl.PlaceToken
                                 }
                          ).Take(1);
                    if (!place.Any())
                    {
                        return NotFound();
                    }

                    return Ok(place.FirstOrDefault());
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
        [Route("EditPlace")]
        [ResponseType(typeof(void))]

        public IHttpActionResult EditPlace(string token, int id, JDE_Places item, int UserId)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = db.JDE_Places.AsNoTracking().Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.PlaceId == id);
                    if (items.Any())
                    {
                        JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Edycja zasobu", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(items.FirstOrDefault()), NewValue = new JavaScriptSerializer().Serialize(item) };
                        db.JDE_Logs.Add(Log);
                        db.Entry(item).State = EntityState.Modified;
                        try
                        {
                            db.SaveChanges();
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            if (!JDE_PlacesExists(id))
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
        [Route("CreatePlace")]
        [ResponseType(typeof(JDE_Places))]
        public IHttpActionResult CreatePlace(string token, JDE_Places item, int UserId)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    item.TenantId = tenants.FirstOrDefault().TenantId;
                    db.JDE_Places.Add(item);
                    db.SaveChanges();
                    JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Utworzenie zasobu", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, NewValue = new JavaScriptSerializer().Serialize(item) };
                    db.JDE_Logs.Add(Log);
                    db.SaveChanges();
                    return Ok(item);
                    //return CreatedAtRoute("DefaultApi", new { id = item.PlaceId }, item);
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
        [Route("DeletePlace")]
        [ResponseType(typeof(JDE_Places))]
        public IHttpActionResult DeletePlace(string token, int id, int UserId)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = db.JDE_Places.Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.PlaceId == id);
                    if (items.Any())
                    {
                        JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Usunięcie zasobu", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(items.FirstOrDefault()) };
                        db.JDE_Places.Remove(items.FirstOrDefault());
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

        private bool JDE_PlacesExists(int id)
        {
            return db.JDE_Places.Count(e => e.PlaceId == id) > 0;
        }
    }

    public class Place
    {
        public int PlaceId { get; set; }
        public string Number1 { get; set; }
        public string Number2 { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int AreaId { get; set; }
        public string AreaName { get; set; }
        public int SetId { get; set; }
        public string SetName { get; set; }
        public string Priority { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public string CreatedByName { get; set; }
        public int TenantId { get; set; }
        public string TenantName { get; set; }
        public string PlaceToken { get; set; }
    }
}