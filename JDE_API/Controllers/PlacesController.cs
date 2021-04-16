using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web;
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
        public IHttpActionResult GetPlaces(string token, int page = 0, int total=0, string query = null)
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
                                    join u2 in db.JDE_Users on pl.LmBy equals u2.UserId into modifiedBy
                                    from mb in modifiedBy.DefaultIfEmpty()
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
                                      Image = pl.Image,
                                      CreatedOn = pl.CreatedOn,
                                      CreatedBy = us.UserId,
                                      CreatedByName = us.Name + " " + us.Surname,
                                      TenantId = t.TenantId,
                                      TenantName = t.TenantName,
                                      PlaceToken = pl.PlaceToken,
                                      IsArchived = pl.IsArchived,
                                      LmBy = pl.LmBy,
                                      LmByName = mb.Name + " " + mb.Surname,
                                      LmOn = pl.LmOn,
                                      HasAttachments = db.JDE_FileAssigns.Any(f=>f.PlaceId==pl.PlaceId)
                                  }
                          );
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
                                 join u2 in db.JDE_Users on pl.LmBy equals u2.UserId into modifiedBy
                                 from mb in modifiedBy.DefaultIfEmpty()
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
                                     Image = pl.Image,
                                     CreatedOn = pl.CreatedOn,
                                     CreatedBy = us.UserId,
                                     CreatedByName = us.Name + " " + us.Surname,
                                     TenantId = t.TenantId,
                                     TenantName = t.TenantName,
                                     PlaceToken = pl.PlaceToken,
                                     LmBy = pl.LmBy,
                                     LmByName = mb.Name + " " + mb.Surname,
                                     LmOn = pl.LmOn,
                                     IsArchived = pl.IsArchived
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
                                 join u2 in db.JDE_Users on pl.LmBy equals u2.UserId into modifiedBy
                                 from mb in modifiedBy.DefaultIfEmpty()
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
                                     Image = pl.Image,
                                     CreatedOn = pl.CreatedOn,
                                     CreatedBy = us.UserId,
                                     CreatedByName = us.Name + " " + us.Surname,
                                     TenantId = t.TenantId,
                                     TenantName = t.TenantName,
                                     PlaceToken = pl.PlaceToken,
                                     LmBy = pl.LmBy,
                                     LmByName = mb.Name + " " + mb.Surname,
                                     LmOn = pl.LmOn,
                                     IsArchived = pl.IsArchived
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
        [Route("GetUsersLastPlaces")]
        public IHttpActionResult GetUsersLastPlaces(string token, int UserId, bool distinct = false)
        {
            if (token != null && token.Length > 0)
            {
                DateTime lastDate = DateTime.Now.AddDays(-1);

                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = (from h in db.JDE_Handlings
                                 join p in db.JDE_Processes on h.ProcessId equals p.ProcessId
                                 join pl in db.JDE_Places on p.PlaceId equals pl.PlaceId
                                 join st in db.JDE_Sets on pl.SetId equals st.SetId
                                 join ar in db.JDE_Areas on pl.AreaId equals ar.AreaId
                                 join us in db.JDE_Users on pl.CreatedBy equals us.UserId
                                 join t in db.JDE_Tenants on pl.TenantId equals t.TenantId
                                 where t.TenantId == tenants.FirstOrDefault().TenantId && h.UserId == UserId && (h.StartedOn >= lastDate || h.FinishedOn >= lastDate)
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
                                     Image = pl.Image,
                                     CreatedOn = pl.CreatedOn,
                                     CreatedBy = us.UserId,
                                     CreatedByName = us.Name + " " + us.Surname,
                                     TenantId = t.TenantId,
                                     TenantName = t.TenantName,
                                     PlaceToken = pl.PlaceToken,
                                     VisitedAt = h.FinishedOn == null ? h.StartedOn : h.FinishedOn,
                                     HandlingId = h.HandlingId
                                 }
                          );
                    if (!items.Any())
                    {
                        return NotFound();
                    }
                    else
                    {
                        items = items.OrderByDescending(i => i.VisitedAt);
                        if (distinct)
                        {
                            items = items.GroupBy(i => i.PlaceId).Select(i => i.FirstOrDefault());
                        }
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

        [HttpPut]
        [Route("EditPlace")]
        [ResponseType(typeof(void))]

        public IHttpActionResult EditPlace(string token, int id, JDE_Places item, int UserId, bool DeleteImage = false)
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
                        item.LmBy = UserId;
                        item.LmOn = DateTime.Now;
                        // Check if to remove image
                        if (DeleteImage)
                        {
                            string oFileName = item.Image;
                            if (!string.IsNullOrEmpty(oFileName))
                            {
                                // There was a file, must delete it first
                                System.IO.File.Delete(Path.Combine(RuntimeSettings.Path2Files, oFileName));
                                System.IO.File.Delete(Path.Combine(RuntimeSettings.Path2Thumbs, oFileName));
                            }
                            item.Image = null;
                        }
                        
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

        [HttpPut]
        [Route("EditPlace")]
        [ResponseType(typeof(void))]
        public HttpResponseMessage EditPlace(string token, int id, int UserId, string PlaceJson)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = db.JDE_Places.Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.PlaceId == id);
                    if (items.Any())
                    {
                        JavaScriptSerializer jss = new JavaScriptSerializer();
                        JDE_Places item = jss.Deserialize<JDE_Places>(PlaceJson);
                        JDE_Places orgItem = items.FirstOrDefault();

                        //handle image

                        var httpRequest = HttpContext.Current.Request;
                        if (httpRequest.ContentLength > 0)
                        {
                            //there's a new content
                            if (httpRequest.ContentLength > Static.RuntimeSettings.MaxFileContentLength)
                            {
                                return Request.CreateResponse(HttpStatusCode.BadRequest, $"{item.Name} przekracza dopuszczalną wielość pliku ({Static.RuntimeSettings.MaxFileContentLength} MB) i został odrzucony");
                            }

                            item.TenantId = tenants.FirstOrDefault().TenantId;
                            var postedFile = httpRequest.Files[0];
                            string filePath = "";
                            if (postedFile != null && postedFile.ContentLength > 0)
                            {
                                var ext = postedFile.FileName.Substring(postedFile.FileName.LastIndexOf('.'));

                                filePath = $"{Static.RuntimeSettings.Path2Files}{item.PlaceToken + ext.ToLower()}";

                                string oFileName = db.JDE_Places.Where(p => p.PlaceId == id).FirstOrDefault().Image;
                                if (!string.IsNullOrEmpty(oFileName))
                                {
                                    // There was a file, must delete it first
                                    System.IO.File.Delete(Path.Combine(RuntimeSettings.Path2Files, oFileName));
                                    System.IO.File.Delete(Path.Combine(RuntimeSettings.Path2Thumbs, oFileName));
                                }

                                postedFile.SaveAs(filePath);
                                Static.Utilities.ProduceThumbnail(filePath);
                                item.Image = item.PlaceToken + ext.ToLower();
                            }

                        }

                        JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Edycja części", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(items.FirstOrDefault()), NewValue = PlaceJson };
                        db.JDE_Logs.Add(Log);
                        item.LmBy = UserId;
                        item.LmOn = DateTime.Now;


                        try
                        {
                            db.Entry(orgItem).CurrentValues.SetValues(item);
                            db.Entry(orgItem).State = EntityState.Modified;
                            db.SaveChanges();

                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            if (!JDE_PlacesExists(id))
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

        [HttpGet]
        [Route("ArchivePlace")]
        [ResponseType(typeof(void))]
        public HttpResponseMessage ArchivePlace(string token, int id, int UserId)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = db.JDE_Places.AsNoTracking().Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.PlaceId == id);
                    if (items.Any())
                    {
                        JDE_Places orgItem = items.FirstOrDefault();

                        orgItem.IsArchived = true;
                        JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Archiwizacja zasobu", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(items.FirstOrDefault()), NewValue = "" };
                        db.JDE_Logs.Add(Log);

                        try
                        {
                            db.Entry(orgItem).State = EntityState.Modified;
                            db.SaveChanges();

                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            if (!JDE_PlacesExists(id))
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
                    item.PlaceToken = Static.Utilities.GetToken();
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

        [HttpPost]
        [Route("CreatePlace")]
        [ResponseType(typeof(JDE_Places))]
        public HttpResponseMessage CreatePlace(string token, string PlaceJson, int UserId)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    JavaScriptSerializer jss = new JavaScriptSerializer();
                    JDE_Places item = jss.Deserialize<JDE_Places>(PlaceJson);

                    var httpRequest = HttpContext.Current.Request;
                    if (httpRequest.ContentLength > 0)
                    {
                        if (httpRequest.ContentLength > Static.RuntimeSettings.MaxFileContentLength)
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, $"{item.Name} przekracza dopuszczalną wielość pliku ({Static.RuntimeSettings.MaxFileContentLength} MB) i został odrzucony");
                        }

                        //create unique token unless the file already exists
                        item.PlaceToken = Static.Utilities.GetToken();
                        item.TenantId = tenants.FirstOrDefault().TenantId;
                        var postedFile = httpRequest.Files[0];
                        string filePath = "";
                        if (postedFile != null && postedFile.ContentLength > 0)
                        {
                            var ext = postedFile.FileName.Substring(postedFile.FileName.LastIndexOf('.'));

                            filePath = $"{Static.RuntimeSettings.Path2Files}{item.PlaceToken + ext.ToLower()}";

                            postedFile.SaveAs(filePath);
                            Static.Utilities.ProduceThumbnail(filePath);
                            item.Image = item.PlaceToken + ext.ToLower();
                        }

                    }

                    item.TenantId = tenants.FirstOrDefault().TenantId;
                    item.CreatedOn = DateTime.Now;
                    db.JDE_Places.Add(item);
                    db.SaveChanges();
                    JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Utworzenie zasobu", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, NewValue = new JavaScriptSerializer().Serialize(item) };
                    db.JDE_Logs.Add(Log);
                    db.SaveChanges();

                    return Request.CreateResponse(HttpStatusCode.Created, item);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
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
                        string oFileName = items.FirstOrDefault().Image;
                        if (!string.IsNullOrEmpty(oFileName))
                        {
                            // There was a file, must delete it first
                            System.IO.File.Delete(Path.Combine(RuntimeSettings.Path2Files, oFileName));
                            System.IO.File.Delete(Path.Combine(RuntimeSettings.Path2Thumbs, oFileName));
                        }
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
        public string Image { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public string CreatedByName { get; set; }
        public int TenantId { get; set; }
        public string TenantName { get; set; }
        public string PlaceToken { get; set; }

        public DateTime? LmOn { get; set; }
        public int? LmBy { get; set; }
        public string LmByName { get; set; }
    }
}