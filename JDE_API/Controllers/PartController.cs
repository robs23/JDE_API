using JDE_API.Models;
using JDE_API.Static;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Script.Serialization;

namespace JDE_API.Controllers
{
    public class PartController : ApiController
    {
        private Models.DbModel db = new Models.DbModel();
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [HttpGet]
        public HttpResponseMessage GetThumb(string Photo)
        {
            var path = Path.Combine(RuntimeSettings.Path2Thumbs, Photo);
            var theFile = new FileInfo(path);
            if (theFile.Exists)
            {
                var stream = new MemoryStream();
                var result = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(stream.ToArray())
                };
                result.Content.Headers.ContentDisposition =
                new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
                {
                    FileName = theFile.Name
                };
                result.Content.Headers.ContentType =
                new MediaTypeHeaderValue("application/octet-stream");

                return result;
            }
            return null;
        }

        [HttpGet]
        [Route("GetParts")]
        public IHttpActionResult GetParts(string token, int page = 0, int pageSize = 0, int total = 0, string query = null)
        {

            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = (from p in db.JDE_Parts
                                 join pr in db.JDE_Companies on p.ProducerId equals pr.CompanyId into producers
                                 from prs in producers.DefaultIfEmpty()
                                 join s in db.JDE_Companies on p.SupplierId equals s.CompanyId into suppliers
                                 from sups in suppliers.DefaultIfEmpty()
                                 join u in db.JDE_Users on p.CreatedBy equals u.UserId
                                 join u2 in db.JDE_Users on p.LmBy equals u2.UserId into modifiedBy
                                 from mb in modifiedBy.DefaultIfEmpty()
                                 join t in db.JDE_Tenants on p.TenantId equals t.TenantId
                                 where p.TenantId == tenants.FirstOrDefault().TenantId
                                 orderby p.CreatedOn descending
                                 select new
                                 {
                                     PartId = p.PartId,
                                     Name = p.Name,
                                     Description = p.Description,
                                     EAN = p.EAN,
                                     ProducerId = p.ProducerId,
                                     ProducerName = prs.Name,
                                     ProducentsCode = p.ProducentsCode,
                                     SupplierId = p.SupplierId,
                                     SupplierName = sups.Name,
                                     Symbol = p.Symbol,
                                     Destination = p.Destination,
                                     Appliance = p.Appliance,
                                     UsedOn = p.UsedOn,
                                     Token = p.Token,
                                     Image = p.Image,
                                     CreatedOn = p.CreatedOn,
                                     CreatedBy = p.CreatedBy,
                                     CreatedByName = u.Name + " " + u.Surname,
                                     LmOn = p.LmOn,
                                     LmBy = p.LmBy,
                                     LmByName = mb.Name + " " + mb.Surname,
                                     TenantId = p.TenantId,
                                     TenantName = t.TenantName,
                                     IsArchived = p.IsArchived
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
        [Route("GetPart")]
        [ResponseType(typeof(JDE_Parts))]
        public IHttpActionResult GetPart(string token, int id)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = (from p in db.JDE_Parts
                                 join pr in db.JDE_Companies on p.ProducerId equals pr.CompanyId into producers
                                 from prs in producers.DefaultIfEmpty()
                                 join s in db.JDE_Companies on p.SupplierId equals s.CompanyId into suppliers
                                 from sups in suppliers.DefaultIfEmpty()
                                 join u in db.JDE_Users on p.CreatedBy equals u.UserId
                                 join u2 in db.JDE_Users on p.LmBy equals u2.UserId into modifiedBy
                                 from mb in modifiedBy.DefaultIfEmpty()
                                 join t in db.JDE_Tenants on p.TenantId equals t.TenantId
                                 where p.TenantId == tenants.FirstOrDefault().TenantId && p.PartId ==id
                                 orderby p.CreatedOn descending
                                 select new
                                 {
                                     PartId = p.PartId,
                                     Name = p.Name,
                                     Description = p.Description,
                                     EAN = p.EAN,
                                     ProducerId = p.ProducerId,
                                     ProducerName = prs.Name,
                                     ProducentsCode = p.ProducentsCode,
                                     SupplierId = p.SupplierId,
                                     SupplierName = sups.Name,
                                     Symbol = p.Symbol,
                                     Destination = p.Destination,
                                     Appliance = p.Appliance,
                                     UsedOn = p.UsedOn,
                                     Token = p.Token,
                                     Image = p.Image,
                                     CreatedOn = p.CreatedOn,
                                     CreatedBy = p.CreatedBy,
                                     CreatedByName = u.Name + " " + u.Surname,
                                     LmOn = p.LmOn,
                                     LmBy = p.LmBy,
                                     LmByName = mb.Name + " " + mb.Surname,
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

        [HttpGet]
        [Route("GetPart")]
        [ResponseType(typeof(JDE_Parts))]
        public IHttpActionResult GetPart(string token, string partsToken)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = (from p in db.JDE_Parts
                                 join pr in db.JDE_Companies on p.ProducerId equals pr.CompanyId into producers
                                 from prs in producers.DefaultIfEmpty()
                                 join s in db.JDE_Companies on p.SupplierId equals s.CompanyId into suppliers
                                 from sups in suppliers.DefaultIfEmpty()
                                 join u in db.JDE_Users on p.CreatedBy equals u.UserId
                                 join u2 in db.JDE_Users on p.LmBy equals u2.UserId into modifiedBy
                                 from mb in modifiedBy.DefaultIfEmpty()
                                 join t in db.JDE_Tenants on p.TenantId equals t.TenantId
                                 where p.TenantId == tenants.FirstOrDefault().TenantId && p.Token.Equals(partsToken.Trim())
                                 orderby p.CreatedOn descending
                                 select new
                                 {
                                     PartId = p.PartId,
                                     Name = p.Name,
                                     Description = p.Description,
                                     EAN = p.EAN,
                                     ProducerId = p.ProducerId,
                                     ProducerName = prs.Name,
                                     ProducentsCode = p.ProducentsCode,
                                     SupplierId = p.SupplierId,
                                     SupplierName = sups.Name,
                                     Symbol = p.Symbol,
                                     Destination = p.Destination,
                                     Appliance = p.Appliance,
                                     UsedOn = p.UsedOn,
                                     Token = p.Token,
                                     Image = p.Image,
                                     CreatedOn = p.CreatedOn,
                                     CreatedBy = p.CreatedBy,
                                     CreatedByName = u.Name + " " + u.Surname,
                                     LmOn = p.LmOn,
                                     LmBy = p.LmBy,
                                     LmByName = mb.Name + " " + mb.Surname,
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

        [HttpGet]
        [Route("ArchivePart")]
        [ResponseType(typeof(void))]
        public HttpResponseMessage ArchivePart(string token, int id, int UserId)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = db.JDE_Parts.AsNoTracking().Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.PartId == id);
                    if (items.Any())
                    {
                        JDE_Parts orgItem = items.FirstOrDefault();

                        orgItem.IsArchived = true;
                        JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Archiwizacja części", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(items.FirstOrDefault()), NewValue = "" };
                        db.JDE_Logs.Add(Log);

                        try
                        {
                            db.Entry(orgItem).State = EntityState.Modified;
                            db.SaveChanges();

                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            if (!JDE_PartExists(id))
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

        [HttpPut]
        [Route("EditPart")]
        [ResponseType(typeof(void))]

        public IHttpActionResult EditPart(string token, int id, int UserId, JDE_Parts item)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = db.JDE_Parts.AsNoTracking().Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.PartId == id);
                    if (items.Any())
                    {

                        JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Edycja części", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(items.FirstOrDefault()), NewValue = new JavaScriptSerializer().Serialize(item) };
                        db.JDE_Logs.Add(Log);
                        item.LmBy = UserId;
                        item.LmOn = DateTime.Now;
                        // Not any image was sent so I'm removing it
                        string oFileName = item.Image;
                        if (!string.IsNullOrEmpty(oFileName))
                        {
                            // There was a file, must delete it first
                            System.IO.File.Delete(Path.Combine(RuntimeSettings.Path2Files, oFileName));
                            System.IO.File.Delete(Path.Combine(RuntimeSettings.Path2Thumbs, oFileName));
                        }
                        item.Image = null;

                        db.Entry(item).State = EntityState.Modified;
                        try
                        {
                            db.SaveChanges();
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            if (!JDE_PartExists(id))
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
        [Route("EditPart")]
        [ResponseType(typeof(void))]

        public HttpResponseMessage EditPart(string token, int id, int UserId, string PartJson)
        {
            try
            {
                if (token != null && token.Length > 0)
                {
                    var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                    if (tenants.Any())
                    {
                        JavaScriptSerializer jss = new JavaScriptSerializer();
                        JDE_Parts item = jss.Deserialize<JDE_Parts>(PartJson);

                        try
                        {
                            var items = db.JDE_Parts.Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.PartId == id);
                            if (items.Any())
                            {
                                Logger.Info("EditPart: Znalazłem odpowiednią część. Przystępuję do edycji Id={id}, UserId={UserId}", id, UserId);
                                JDE_Parts orgItem = items.FirstOrDefault();

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
                                        Logger.Info("EditPart: Znaleziono nowe pliki. Przystępuję do zapisu na dysku. Id={id}, UserId={UserId}", id, UserId);
                                        var ext = postedFile.FileName.Substring(postedFile.FileName.LastIndexOf('.'));

                                        filePath = $"{Static.RuntimeSettings.Path2Files}{item.Token + ext.ToLower()}";

                                        string oFileName = db.JDE_Parts.Where(p => p.PartId == id).FirstOrDefault().Image;
                                        if (!string.IsNullOrEmpty(oFileName))
                                        {
                                            // There was a file, must delete it first
                                            Logger.Info("EditPart: Istnieją poprzednie pliki pod tą nazwą. Przystępuję do usuwania. Id={id}, UserId={UserId}", id, UserId);
                                            System.IO.File.Delete(Path.Combine(RuntimeSettings.Path2Files, oFileName));
                                            System.IO.File.Delete(Path.Combine(RuntimeSettings.Path2Thumbs, oFileName));
                                        }

                                        postedFile.SaveAs(filePath);
                                        Logger.Info("EditPart: Zapisano pliki. Przystępuję do utworzenia miniatury.. Id={id}, UserId={UserId}", id, UserId);
                                        Static.Utilities.ProduceThumbnail(filePath);
                                        item.Image = item.Token + ext.ToLower();
                                    }

                                }

                                JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Edycja części", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(items.FirstOrDefault()), NewValue = PartJson };
                                db.JDE_Logs.Add(Log);
                                item.LmBy = UserId;
                                item.LmOn = DateTime.Now;


                                try
                                {
                                    Logger.Info("EditPart: Przystępuję do zapisu zmian w bazie danych. Id={id}, UserId={UserId}", id, UserId);
                                    db.Entry(orgItem).CurrentValues.SetValues(item);
                                    db.Entry(orgItem).State = EntityState.Modified;
                                    db.SaveChanges();
                                    Logger.Info("EditPart: Zapisano zmiany w bazie. Id={id}, UserId={UserId}", id, UserId);
                                }
                                catch (DbUpdateConcurrencyException)
                                {
                                    if (!JDE_PartExists(id))
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
                                    Logger.Error("Błąd w EditPart. Id={id}, UserId={UserId}. Szczegóły: {Message}, nowa wartość: {item}", id, UserId, ex.ToString(), item);
                                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
                                }
                            }
                        }
                        catch (Exception ex)
                        {

                            Logger.Error("Błąd w EditPart. Id={id}, UserId={UserId}. Szczegóły: {Message}, nowa wartość: {item}", id, UserId, ex.ToString(), item);
                            return Request.CreateResponse(HttpStatusCode.NoContent);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Błąd w EditPart. Id={id}, UserId={UserId}. Szczegóły: {Message}", id, UserId, ex.ToString());
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            return Request.CreateResponse(HttpStatusCode.NoContent);
        }

        [HttpPost]
        [Route("CreatePart")]
        [ResponseType(typeof(JDE_Parts))]
        public IHttpActionResult CreatePart(string token, JDE_Parts item, int UserId)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    item.TenantId = tenants.FirstOrDefault().TenantId;
                    item.CreatedOn = DateTime.Now;
                    item.Token = Static.Utilities.GetToken();
                    db.JDE_Parts.Add(item);
                    db.SaveChanges();
                    JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Utworzenie części", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, NewValue = new JavaScriptSerializer().Serialize(item) };
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

        [HttpPost]
        [Route("CreatePart")]
        [ResponseType(typeof(JDE_Parts))]
        public HttpResponseMessage CreatePart(string token, string PartJson, int UserId)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    JavaScriptSerializer jss = new JavaScriptSerializer();
                    JDE_Parts item = jss.Deserialize<JDE_Parts>(PartJson);

                    var httpRequest = HttpContext.Current.Request;
                    if (httpRequest.ContentLength > 0)
                    {
                        if (httpRequest.ContentLength > Static.RuntimeSettings.MaxFileContentLength)
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, $"{item.Name} przekracza dopuszczalną wielość pliku ({Static.RuntimeSettings.MaxFileContentLength} MB) i został odrzucony");
                        }
                        
                        //create unique token unless the file already exists
                        item.Token = Static.Utilities.GetToken();
                        item.TenantId = tenants.FirstOrDefault().TenantId;
                        var postedFile = httpRequest.Files[0];
                        string filePath = "";
                        if (postedFile != null && postedFile.ContentLength > 0)
                        {
                            var ext = postedFile.FileName.Substring(postedFile.FileName.LastIndexOf('.'));

                            filePath = $"{Static.RuntimeSettings.Path2Files}{item.Token + ext.ToLower()}";

                            postedFile.SaveAs(filePath);
                            Static.Utilities.ProduceThumbnail(filePath);
                            item.Image = item.Token + ext.ToLower();
                        }
                        
                    }

                    item.TenantId = tenants.FirstOrDefault().TenantId;
                    item.CreatedOn = DateTime.Now;
                    db.JDE_Parts.Add(item);
                    db.SaveChanges();
                    JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Utworzenie części", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, NewValue = new JavaScriptSerializer().Serialize(item) };
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
        [Route("DeletePart")]
        [ResponseType(typeof(JDE_Parts))]
        public IHttpActionResult DeletePart(string token, int id, int UserId)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = db.JDE_Parts.Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.PartId == id);
                    if (items.Any())
                    {
                        string oFileName = items.FirstOrDefault().Image;
                        if (!string.IsNullOrEmpty(oFileName))
                        {
                            // There was a file, must delete it first
                            System.IO.File.Delete(Path.Combine(RuntimeSettings.Path2Files, oFileName));
                            System.IO.File.Delete(Path.Combine(RuntimeSettings.Path2Thumbs, oFileName));
                        }
                        JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Usunięcie części", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(items.FirstOrDefault()) };
                        db.JDE_Parts.Remove(items.FirstOrDefault());
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

        private bool JDE_PartExists(int id)
        {
            return db.JDE_Parts.Count(e => e.PartId == id) > 0;
        }
    }
}
