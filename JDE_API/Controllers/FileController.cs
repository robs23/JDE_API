using JDE_API.Models;
using JDE_API.Static;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Script.Serialization;

namespace JDE_API.Controllers
{
    public class FileController : ApiController
    {
        private Models.DbModel db = new Models.DbModel();

        [HttpGet]
        [Route("GetFiles")]
        public IHttpActionResult GetFiles(string token, int page = 0, int pageSize = 0, int total = 0, string query = null)
        {

            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = (from f in db.JDE_Files
                                 join u in db.JDE_Users on f.CreatedBy equals u.UserId
                                 join t in db.JDE_Tenants on f.TenantId equals t.TenantId
                                 where f.TenantId == tenants.FirstOrDefault().TenantId
                                 orderby f.CreatedOn descending
                                 select new
                                 {
                                     FileId = f.FileId,
                                     Name = f.Name,
                                     Description = f.Description,
                                     CreatedOn = f.CreatedOn,
                                     CreatedBy = f.CreatedBy,
                                     CreatedByName = u.Name + " " + u.Surname,
                                     TenantId = f.TenantId,
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
        [Route("GetFileAssigns")]
        public IHttpActionResult GetFileAssigns(string token, int page = 0, int pageSize = 0, int total = 0, string query = null)
        {

            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = (from fa in db.JDE_FileAssigns
                                 join f in db.JDE_Files on fa.FileId equals f.FileId
                                 join pt in db.JDE_Parts on fa.PartId equals pt.PartId into parts
                                 from prts in parts.DefaultIfEmpty()
                                 join pl in db.JDE_Places on fa.PlaceId equals pl.PlaceId into places
                                 from plcs in places.DefaultIfEmpty()
                                 join u in db.JDE_Users on fa.CreatedBy equals u.UserId
                                 join t in db.JDE_Tenants on fa.TenantId equals t.TenantId
                                 where fa.TenantId == tenants.FirstOrDefault().TenantId
                                 orderby f.CreatedOn descending
                                 select new
                                 {
                                     FileId = f.FileId,
                                     Name = f.Name,
                                     Token = f.Token,
                                     PartId = prts.PartId,
                                     PartName = prts.Name,
                                     PlaceId = plcs.PlaceId,
                                     PlaceName = plcs.Name,
                                     ProcessId = fa.ProcessId,
                                     CreatedOn = fa.CreatedOn,
                                     CreatedBy = fa.CreatedBy,
                                     CreatedByName = u.Name + " " + u.Surname,
                                     TenantId = fa.TenantId,
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
        [Route("GetFile")]
        [ResponseType(typeof(JDE_Files))]
        public IHttpActionResult GetFile(string token, int id)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = (from f in db.JDE_Files
                                 join u in db.JDE_Users on f.CreatedBy equals u.UserId
                                 join t in db.JDE_Tenants on f.TenantId equals t.TenantId
                                 where f.TenantId == tenants.FirstOrDefault().TenantId && f.FileId==id
                                 orderby f.CreatedOn descending
                                 select new
                                 {
                                     FileId = f.FileId,
                                     Name = f.Name,
                                     Description = f.Description,
                                     CreatedOn = f.CreatedOn,
                                     CreatedBy = f.CreatedBy,
                                     CreatedByName = u.Name + " " + u.Surname,
                                     TenantId = f.TenantId,
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
        [Route("EditFile")]
        [ResponseType(typeof(void))]

        public IHttpActionResult EditFile(string token, int id, int UserId, JDE_Files item)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = db.JDE_Files.AsNoTracking().Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.FileId == id);
                    if (items.Any())
                    {

                        JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Edycja pliku", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(items.FirstOrDefault()), NewValue = new JavaScriptSerializer().Serialize(item) };
                        db.JDE_Logs.Add(Log);
                        db.Entry(item).State = EntityState.Modified;
                        try
                        {
                            db.SaveChanges();
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            if (!JDE_FileExists(id))
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
        [Route("CreateFile")]
        public HttpResponseMessage CreateFile(string token, JDE_Files item, int UserId, int? PlaceId=null, int? PartId=null, int? ProcessId=null)
        {           
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var httpRequest = HttpContext.Current.Request;
                    if (httpRequest.Files.Any())
                    {
                        if (httpRequest.Files[0].ContentLength > Static.RuntimeSettings.MaxFileContentLength)
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, $"{item.Name} przekracza dopuszczalną wielość pliku ({Static.RuntimeSettings.MaxFileContentLength} MB) i został odrzucony");
                        }
                        if (string.IsNullOrEmpty(item.Token))
                        {
                            //create unique token unless the file already exists
                            item.Token = Static.Utilities.GetToken();
                            item.TenantId = tenants.FirstOrDefault().TenantId;
                            var postedFile = httpRequest.Files[0];
                            string filePath = "";
                            if (postedFile != null && postedFile.ContentLength > 0)
                            {
                                var ext = postedFile.FileName.Substring(postedFile.FileName.LastIndexOf('.'));
                                var extension = ext.ToLower();

                                filePath = HttpContext.Current.Server.MapPath($"{Static.RuntimeSettings.Path2Files}{item.Token +  "."}");

                                postedFile.SaveAs(filePath);
                                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, item);
                            }
                            item.CreatedOn = DateTime.Now;
                            item.Link = filePath;
                            db.JDE_Files.Add(item);
                            db.SaveChanges();

                            JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Utworzenie pliku", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, NewValue = new JavaScriptSerializer().Serialize(item) };
                            db.JDE_Logs.Add(Log);
                            db.SaveChanges();
                        }
                    }

                    JDE_FileAssigns FileAssing = new JDE_FileAssigns { FileId = item.FileId, CreatedBy = UserId, CreatedOn = DateTime.Now, TenantId = tenants.FirstOrDefault().TenantId, PartId = PartId, PlaceId = PlaceId, ProcessId = ProcessId };
                    string word = "";
                    if (PlaceId != null)
                    {
                        word = "zasobu";
                    }else if(PartId != null)
                    {
                        word = "części";
                    }
                    else
                    {
                        word = "zgłoszenia";
                    }
                    db.JDE_FileAssigns.Add(FileAssing);
                    db.SaveChanges();
                    JDE_Logs Log2 = new JDE_Logs { UserId = UserId, Description = $"Przypisanie pliku do {word}", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, NewValue = new JavaScriptSerializer().Serialize(item) };
                    db.JDE_Logs.Add(Log2);
                    db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, item);
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

        //private Task<string> SaveFile()
        //{

        //}

        [HttpDelete]
        [Route("DeleteFile")]
        [ResponseType(typeof(JDE_Files))]
        public IHttpActionResult DeleteFile(string token, int id, int UserId)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = db.JDE_Files.Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.FileId == id);
                    if (items.Any())
                    {
                        JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Usunięcie pliku", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(items.FirstOrDefault()) };
                        db.JDE_Files.Remove(items.FirstOrDefault());
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

        private bool JDE_FileExists(int id)
        {
            return db.JDE_Files.Count(e => e.FileId == id) > 0;
        }
    }
}
