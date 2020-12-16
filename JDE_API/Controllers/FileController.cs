using JDE_API.Models;
using JDE_API.Static;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Script.Serialization;
using File = JDE_API.Models.File;

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
                                 join fa in db.JDE_FileAssigns on f.FileId equals fa.FileId
                                 join u in db.JDE_Users on fa.CreatedBy equals u.UserId
                                 join t in db.JDE_Tenants on f.TenantId equals t.TenantId
                                 where f.TenantId == tenants.FirstOrDefault().TenantId
                                 orderby f.CreatedOn descending
                                 select new
                                 {
                                     FileId = f.FileId,
                                     Name = f.Name,
                                     Description = f.Description,
                                     Token = f.Token,
                                     Link = f.Link,
                                     PartId = fa.PartId,
                                     PlaceId = fa.PlaceId,
                                     ProcessId = fa.ProcessId,
                                     CreatedOn = fa.CreatedOn,
                                     CreatedBy = fa.CreatedBy,
                                     CreatedByName = u.Name + " " + u.Surname,
                                     TenantId = f.TenantId,
                                     TenantName = t.TenantName,
                                     Type = f.Type,
                                     Size = f.Size
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
                                     TenantName = t.TenantName,
                                     Type = f.Type,
                                     Size = f.Size
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
        public HttpResponseMessage GetFile(string token, int id, bool min)
        {
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);

            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = (from f in db.JDE_Files
                                 join fa in db.JDE_FileAssigns on f.FileId equals fa.FileId
                                 join u in db.JDE_Users on fa.CreatedBy equals u.UserId
                                 join t in db.JDE_Tenants on f.TenantId equals t.TenantId
                                 where f.TenantId == tenants.FirstOrDefault().TenantId && f.FileId == id
                                 orderby f.CreatedOn descending
                                 select new File
                                 {
                                     FileId = f.FileId,
                                     Name = f.Name,
                                     Description = f.Description,
                                     Token = f.Token,
                                     Link = f.Link,
                                     CreatedOn = fa.CreatedOn,
                                     CreatedBy = fa.CreatedBy,
                                     CreatedByName = u.Name + " " + u.Surname,
                                     TenantId = f.TenantId,
                                     TenantName = t.TenantName,
                                     Type = f.Type,
                                     Size = (long)f.Size
                                 });

                    if (items.Any())
                    {
                        ByteArrayContent content = PackAttachment(items.FirstOrDefault().Name,min);
                        response.Content = content;
                        return response;
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.NoContent);
                    }

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

        private ByteArrayContent PackAttachment(string fileName, bool min)
        {
            ByteArrayContent content = null;
            string filePath = null;
            
            if (min)
            {
                filePath = RuntimeSettings.Path2Thumbs + fileName;
            }
            else
            {
                filePath = RuntimeSettings.Path2Files + fileName;
            }

            string name = Path.GetFileNameWithoutExtension(filePath);

                if (System.IO.File.Exists(filePath))
                {
                    byte[] bytes = System.IO.File.ReadAllBytes(filePath);
                    content = new ByteArrayContent(bytes);
                    content.Headers.ContentLength = bytes.Length;
                    content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
                    content.Headers.ContentDisposition.FileName = name;
                    content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(MimeMapping.GetMimeMapping(name));
                }

            return content;

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
        public HttpResponseMessage CreateFile(string token, string FileJson, int UserId, int? PlaceId=null, int? PartId=null, int? ProcessId=null)
        {           
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    JavaScriptSerializer jss = new JavaScriptSerializer();
                    JDE_Files item = jss.Deserialize<JDE_Files>(FileJson);
                    var httpRequest = HttpContext.Current.Request;

                    if (httpRequest.ContentLength > 0)
                    {
                        if (httpRequest.ContentLength > Static.RuntimeSettings.MaxFileContentLength)
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

                                filePath = $"{Static.RuntimeSettings.Path2Files}{item.Token +  ext.ToLower()}";

                                postedFile.SaveAs(filePath);
                                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, item);
                            }
                            item.CreatedOn = DateTime.Now;
                            item.CreatedBy = UserId;
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

        [HttpPost]
        [Route("CreateFile")]
        [ResponseType(typeof(JDE_Files))]
        public IHttpActionResult CreateFile(string token, JDE_Files item, int UserId, int? PlaceId = null, int? PartId = null, int? ProcessId = null)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    try
                    {
                        item.TenantId = tenants.FirstOrDefault().TenantId;
                        item.CreatedOn = DateTime.Now;
                        item.CreatedBy = UserId;
                        item.Token = Static.Utilities.GetToken();
                        db.JDE_Files.Add(item);
                        db.SaveChanges();
                        JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Utworzenie pliku", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, NewValue = new JavaScriptSerializer().Serialize(item) };
                        db.JDE_Logs.Add(Log);
                        db.SaveChanges();

                        JDE_FileAssigns FileAssing = new JDE_FileAssigns { FileId = item.FileId, CreatedBy = UserId, CreatedOn = DateTime.Now, TenantId = tenants.FirstOrDefault().TenantId, PartId = PartId, PlaceId = PlaceId, ProcessId = ProcessId };
                        string word = "";
                        if (PlaceId != null)
                        {
                            word = "zasobu";
                        }
                        else if (PartId != null)
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
                        return Ok(item);
                    }catch(Exception ex)
                    {
                        return InternalServerError(ex);
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
        [Route("UploadFile")]
        public HttpResponseMessage UploadFile(string token, string fileToken)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var httpRequest = HttpContext.Current.Request;

                    if (httpRequest.ContentLength > 0)
                    {
                        if (httpRequest.ContentLength > Static.RuntimeSettings.MaxFileContentLength)
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, $"Przekroczono dopuszczalną wielość pliku ({Static.RuntimeSettings.MaxFileContentLength} MB)");
                        }

                        var postedFile = httpRequest.Files[0];
                        string filePath = "";
                        if (postedFile != null && postedFile.ContentLength > 0)
                        {
                            var ext = postedFile.FileName.Substring(postedFile.FileName.LastIndexOf('.'));

                            filePath = $"{Static.RuntimeSettings.Path2Files}{fileToken + ext.ToLower()}";

                            postedFile.SaveAs(filePath);
                            if (postedFile.IsImage())
                            {
                                Static.Utilities.ProduceThumbnail(filePath);
                            }
                            //edit file data to mark IsUploaded as true

                            try
                            {
                                JDE_Files file = db.JDE_Files.Where(f => f.Token == fileToken).FirstOrDefault();
                                if (file != null)
                                {
                                    file.IsUploaded = true;
                                    file.Link = RuntimeSettings.Path2Files;
                                    db.Entry(file).State = EntityState.Modified;
                                    db.SaveChanges();
                                }
                            }
                            catch (Exception ex)
                            {

                            }

                            return Request.CreateResponse(HttpStatusCode.OK);
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Brak pliku");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Plik nieprawidłowy (plik zawiera 0 bajtów)");

                    }
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

        [HttpGet]
        [Route("GetAttachment")]
        public HttpResponseMessage GetAttachment(string token, string name, bool min)
        {
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);

            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {

                        ByteArrayContent content = PackAttachment(name, min);
                    if (content == null)
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound);
                    }
                    else
                    {
                        response.Content = content;
                        return response;
                    }
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
        [Route("DeleteFile")]
        public IHttpActionResult DeleteFile(string token, int id, int UserId, int? PlaceId = null, int? PartId = null, int? ProcessId = null)
        {
            //File should only be deleted if we're deleting last fileAssign
            //otherwise we're deleting only fileAssign
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = db.JDE_Files.Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.FileId == id);
                    if (items.Any())
                    {
                        JDE_Files item = items.FirstOrDefault();
                        var fileAssigns = db.JDE_FileAssigns.Where(fi => fi.FileId == id);
                        if (fileAssigns.Any())
                        {
                            //at least 1 fileAssign
                            //otherwise it would be weird
                            _DeleteFileAssigns(id, UserId, (int)fileAssigns.FirstOrDefault().TenantId, PlaceId, PartId, ProcessId);
                            if (fileAssigns.Count() == 1)
                            {
                                //there's only 1 assigns so more it's the last one
                                _DeleteFile(items.FirstOrDefault().Token, items.FirstOrDefault().Type, UserId, (int)items.FirstOrDefault().TenantId);

                            }
                        }
                        else
                        {
                            //there's no fileAssign
                            //existing file should be deleted
                            //should never happen in the first place
                            _DeleteFile(items.FirstOrDefault().Token, items.FirstOrDefault().Type, UserId, (int)items.FirstOrDefault().TenantId);
                        }


                        return Ok();
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

        private void _DeleteFile(string token, string type, int UserId, int tenantId)
        {
            var items = db.JDE_Files.Where(f=>f.Token==token);
            if (items.Any())
            {
                JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Usunięcie pliku", TenantId = tenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(items.FirstOrDefault()) };
                db.JDE_Files.Remove(items.FirstOrDefault());
                db.JDE_Logs.Add(Log);
                db.SaveChanges();
                //delete physical file
                if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(type))
                {
                    // There was a file, must delete it first
                    System.IO.File.Delete(Path.Combine(RuntimeSettings.Path2Files, $"{token}.{type}"));
                    System.IO.File.Delete(Path.Combine(RuntimeSettings.Path2Thumbs, $"{token}.{type}"));
                }
            }
        }

        private void _DeleteFileAssigns(int fileId, int UserId, int tenantId, int? PlaceId = null, int? PartId = null, int? ProcessId = null)
        {
            var fileAssigns = db.JDE_FileAssigns.Where(f => f.FileId == fileId && ((f.PlaceId==PlaceId && PlaceId!=null) || (f.PartId==PartId && PartId!=null) || (f.ProcessId==ProcessId && ProcessId!=null)));
            List<JDE_Logs> logs = new List<JDE_Logs>();
            if (fileAssigns.Any())
            {
                foreach(var i in fileAssigns)
                {
                    logs.Add(new JDE_Logs { UserId = UserId, Description = "Usunięcie przypisania pliku", TenantId = tenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(i)});
                    db.JDE_FileAssigns.Remove(i);
                }
                
                db.JDE_Logs.AddRange(logs);
                db.SaveChanges();
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
