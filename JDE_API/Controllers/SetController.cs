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
    public class SetController : ApiController
    {
        private Models.DbModel db = new Models.DbModel();

        [HttpGet]
        [Route("GetSets")]
        public IHttpActionResult GetSets(string token, int page=0, int total=0)
        {

            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = (from s in db.JDE_Sets
                                 join t in db.JDE_Tenants on s.TenantId equals t.TenantId
                                 join u in db.JDE_Users on s.CreatedBy equals u.UserId
                                 where s.TenantId == tenants.FirstOrDefault().TenantId
                                 orderby s.CreatedOn descending
                                 select new
                                 {
                                     SetId = s.SetId,
                                     Number = s.Number,
                                     Description = s.Description,
                                     Name = s.Name,
                                     TenantId = s.TenantId,
                                     TenantName = t.TenantName,
                                     CreatedOn = s.CreatedOn,
                                     CreatedBy = s.CreatedBy,
                                     CreatedByName = u.Name + " " + u.Surname
                                 });
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
        [Route("GetSet")]
        [ResponseType(typeof(JDE_Sets))]
        public IHttpActionResult GetSet(string token, int id)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = (from s in db.JDE_Sets
                                 join t in db.JDE_Tenants on s.TenantId equals t.TenantId
                                 join u in db.JDE_Users on s.CreatedBy equals u.UserId
                                 where s.TenantId == tenants.FirstOrDefault().TenantId && s.SetId == id
                                 select new
                                 {
                                     SetId = s.SetId,
                                     Number = s.Number,
                                     Description = s.Description,
                                     Name = s.Name,
                                     TenantId = s.TenantId,
                                     TenantName = t.TenantName,
                                     CreatedOn = s.CreatedOn,
                                     CreatedBy = s.CreatedBy,
                                     CreatedByName = u.Name + " " + u.Surname
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
        [Route("EditSet")]
        [ResponseType(typeof(void))]

        public IHttpActionResult EditSet(string token, int id, JDE_Sets item, int UserId)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = db.JDE_Sets.Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.SetId == id);
                    if (items.Any())
                    {
                        JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Edycja instalacji", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(items.FirstOrDefault()), NewValue = new JavaScriptSerializer().Serialize(item) };
                        db.JDE_Logs.Add(Log);
                        db.Entry(item).State = EntityState.Modified;
                        try
                        {
                            db.SaveChanges();
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            if (!JDE_SetsExists(id))
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

        [HttpGet]
        [Route("GetPlacesBySetName")]
        public IHttpActionResult GetPlacesBySetName(string token, string name, int UserId)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    object returned = JDE_SetsExists(name, false);
                    if (Type.GetTypeCode(returned.GetType()) == TypeCode.Int32)
                    {
                        //there's set of given name, return its places
                        var items = (from pl in db.JDE_Places
                                     join st in db.JDE_Sets on pl.SetId equals st.SetId
                                     join ar in db.JDE_Areas on pl.AreaId equals ar.AreaId
                                     join us in db.JDE_Users on pl.CreatedBy equals us.UserId
                                     join t in db.JDE_Tenants on pl.TenantId equals t.TenantId
                                     where t.TenantId == tenants.FirstOrDefault().TenantId && st.SetId == (int)returned
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
                        return Ok(items);
                    }
                    else
                    {
                        //there's NO set of given name, create it
                        //But first you have to assign it to "Nieokreślone" Area (create if have to)
                        JDE_Areas nArea;
                        JDE_Sets nSet;
                        JDE_Logs Log;

                        if (!db.JDE_Areas.Where(a => a.Name.Equals("Nieokreślone")).Any())
                        {
                            //no are like that, let's create it.
                            nArea = new JDE_Areas
                            {
                                Name = "Nieokreślone",
                                Description = "Trafia tu wszystko co zostaje utworzone bez jasno określonego docelowego obszaru",
                                TenantId = tenants.FirstOrDefault().TenantId,
                                CreatedBy = UserId,
                                CreatedOn = DateTime.Now
                            };
                            db.JDE_Areas.Add(nArea);
                            db.SaveChanges();
                            Log = new JDE_Logs { UserId = UserId, Description = "Utworzenie obszaru", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, NewValue = new JavaScriptSerializer().Serialize(nArea) };
                            db.JDE_Logs.Add(Log);
                            db.SaveChanges();
                        }
                        else
                        {
                            //it exists, ret
                            nArea = db.JDE_Areas.Where(a => a.Name.Equals("Nieokreślone")).FirstOrDefault();
                        }

                        nSet = new JDE_Sets
                        {
                            Name = name.Trim(),
                            Description = null,
                            CreatedBy = UserId,
                            CreatedOn = DateTime.Now,
                            TenantId = tenants.FirstOrDefault().TenantId
                        };
                        db.JDE_Sets.Add(nSet);
                        db.SaveChanges();
                        Log = new JDE_Logs { UserId = UserId, Description = "Utworzenie instalacji", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, NewValue = new JavaScriptSerializer().Serialize(nSet) };
                        db.JDE_Logs.Add(Log);
                        db.SaveChanges();


                        JDE_Places nPlace = new JDE_Places
                        {
                            Name = name.Trim(),
                            Description = null,
                            PlaceToken = Utilities.uniqueToken(),
                            AreaId = nArea.AreaId,
                            SetId = nSet.SetId,
                            CreatedBy = UserId,
                            CreatedOn = DateTime.Now,
                            TenantId = tenants.FirstOrDefault().TenantId
                        };
                        db.JDE_Places.Add(nPlace);
                        db.SaveChanges();
                        Log = new JDE_Logs { UserId = UserId, Description = "Utworzenie zasobu", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, NewValue = new JavaScriptSerializer().Serialize(nPlace) };
                        db.JDE_Logs.Add(Log);
                        db.SaveChanges();

                        var items = (from pl in db.JDE_Places
                                     join st in db.JDE_Sets on pl.SetId equals st.SetId
                                     join ar in db.JDE_Areas on pl.AreaId equals ar.AreaId
                                     join us in db.JDE_Users on pl.CreatedBy equals us.UserId
                                     join t in db.JDE_Tenants on pl.TenantId equals t.TenantId
                                     where t.TenantId == tenants.FirstOrDefault().TenantId && st.SetId == nSet.SetId
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


        [HttpPost]
        [Route("CreateSet")]
        [ResponseType(typeof(JDE_Sets))]
        public IHttpActionResult CreateSet(string token, JDE_Sets item, int UserId)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    item.TenantId = tenants.FirstOrDefault().TenantId;
                    db.JDE_Sets.Add(item);
                    db.SaveChanges();
                    JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Utworzenie instalacji", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, NewValue = new JavaScriptSerializer().Serialize(item) };
                    db.JDE_Logs.Add(Log);
                    db.SaveChanges();
                    return Ok(item);
                    //return CreatedAtRoute("JDEApi", new {token = token, id = item.SetId }, item);
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
        [Route("DeleteSet")]
        [ResponseType(typeof(JDE_Sets))]
        public IHttpActionResult DeleteSet(string token, int id, int UserId)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var items = db.JDE_Sets.Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.SetId == id);
                    if (items.Any())
                    {
                        JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Usunięcie instalacji", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(items.FirstOrDefault()) };
                        db.JDE_Sets.Remove(items.FirstOrDefault());
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

        private bool JDE_SetsExists(int id)
        {
            return db.JDE_Sets.Count(e => e.SetId == id) > 0;
        }

        private object JDE_SetsExists(string name, bool exact)
        {
            string aName = Utilities.ToAscii(name.Trim());

            if (exact)
            {
                return db.JDE_Sets.Count(e => e.Name.Equals(name.Trim())) > 0;
            }
            else
            {
                if (db.JDE_Sets.Where(p => p.Name.Equals(name.Trim())).Any())
                {
                    //maybe given name is exact after all?
                    return db.JDE_Sets.Where(p => p.Name.Equals(name.Trim())).FirstOrDefault().SetId;
                }
                if (db.JDE_Sets.Where(p => p.Name.Equals(aName)).Any())
                {
                    //check Ascii version of the name e.g. 'Mlyn 01' instead of 'Młyn 01'
                    return db.JDE_Sets.Where(p => p.Name.Equals(aName)).FirstOrDefault().SetId;
                }
                else
                {
                    //check if name contains / doesn't contain extra 0 e.g. Mlyn 01
                    string[] str = Regex.Split(name, " ");
                    if(str.Length > 1)
                    {
                        if(int.Parse(str[1]) < 10 && int.Parse(str[1]) > 0 && int.Parse(str[1]).ToString() != str[1])
                        {
                            //given name contains extra 0, that set in db might not have. check without the extra 0
                            string nName = str[0] + " " + int.Parse(str[1]).ToString();
                            string nNameA = Utilities.ToAscii(str[0]) + " " + int.Parse(str[1]).ToString(); //Ascii version
                            if (db.JDE_Sets.Where(p => p.Name.Equals(nName)).Any())
                            {
                                return db.JDE_Sets.Where(p => p.Name.Equals(nName)).FirstOrDefault().SetId;
                            }else if(db.JDE_Sets.Where(p => p.Name.Equals(nNameA)).Any())
                            {
                                //check ascii version too
                                return db.JDE_Sets.Where(p => p.Name.Equals(nNameA)).FirstOrDefault().SetId;
                            }
                        }else if(int.Parse(str[1]) < 10 && int.Parse(str[1]) > 0 && int.Parse(str[1]).ToString() == str[1])
                        {
                            //given name doesn't contain extra 0, that set in db might have. check with the extra 0
                            string nName = str[0] + " 0" + str[1];
                            string nNameA = Utilities.ToAscii(str[0]) + " 0" + str[1]; //Ascii version
                            if (db.JDE_Sets.Where(p => p.Name.Equals(nName)).Any())
                            {
                                return db.JDE_Sets.Where(p => p.Name.Equals(nName)).FirstOrDefault().SetId;
                            }
                            else if (db.JDE_Sets.Where(p => p.Name.Equals(nNameA)).Any())
                            {
                                //check ascii version too
                                return db.JDE_Sets.Where(p => p.Name.Equals(nNameA)).FirstOrDefault().SetId;
                            }
                        }
                    }
                }
                return false;
            }

        }
    }

    public class Set
    {
        public int SetId { get; set; }
        public string Number { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public int TenantId { get; set; }
        public string TenantName { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public string CreatedByName { get; set; }
    }
}