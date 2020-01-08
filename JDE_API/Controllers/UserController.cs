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
using JDE_API.Models;
using System.Web.Script.Serialization;
using System.Diagnostics;
using JDE_API.Static;
using System.Linq.Dynamic;

namespace JDE_API.Controllers
{
    public class UserController : ApiController
    {
        private Models.DbModel db = new Models.DbModel();

        [HttpGet]
        [Route("GetUsers")]
        public IHttpActionResult GetUsers(string token, string query = null)
        {
            if(token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var users = (from us in db.JDE_Users
                                 join t in db.JDE_Tenants on us.TenantId equals t.TenantId
                                 join u in db.JDE_Users on us.CreatedBy equals u.UserId
                                 where us.TenantId == tenants.FirstOrDefault().TenantId
                                 orderby us.CreatedOn descending
                                 select new
                                 {
                                     UserId = us.UserId,
                                     Name = us.Name,
                                     Surname = us.Surname,
                                     Password = us.Password,
                                     isMechanic = us.isMechanic,
                                     TenantId = us.TenantId,
                                     TenantName = t.TenantName,
                                     CreatedOn = us.CreatedOn,
                                     CreatedBy = us.CreatedBy,
                                     CreatedByName = u.Name + " " + u.Surname,
                                     LastLoggedOn = us.lastLoggedOn,
                                     MesLogin = us.MesLogin,
                                     IsArchived = us.IsArchived
                                 });
                    if (users.Any())
                    {
                        if (query != null)
                        {
                            users = users.Where(query);
                        }
                        return Ok(users);
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
        [Route("GetUsers")]
        public IHttpActionResult GetUsers(string token, int page, string query = null)
        {
            int pageSize = RuntimeSettings.PageSize;
            var skip = pageSize * (page - 1);

            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var users = (from us in db.JDE_Users
                                 join t in db.JDE_Tenants on us.TenantId equals t.TenantId
                                 join u in db.JDE_Users on us.CreatedBy equals u.UserId
                                 where us.TenantId == tenants.FirstOrDefault().TenantId
                                 orderby us.CreatedOn descending
                                 select new
                                 {
                                     UserId = us.UserId,
                                     Name = us.Name,
                                     Surname = us.Surname,
                                     Password = us.Password,
                                     isMechanic = us.isMechanic,
                                     TenantId = us.TenantId,
                                     TenantName = t.TenantName,
                                     CreatedOn = us.CreatedOn,
                                     CreatedBy = us.CreatedBy,
                                     CreatedByName = u.Name + " " + u.Surname,
                                     LastLoggedOn = us.lastLoggedOn,
                                     MesLogin = us.MesLogin,
                                     IsActive = us.IsArchived
                                 });
                    if (users.Any())
                    {
                        if (query != null)
                        {
                            users = users.Where(query);
                        }

                        if (skip < users.Count())
                        {
                            users = users.Skip(skip).Take(pageSize);
                            return Ok(users);
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
            else
            {
                return NotFound();
            }

        }

        [HttpGet]
        [Route("IsUserWorking")]
        public IHttpActionResult IsUserWorking(string token, int UserId)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {                    
                    var handlings = (from h in db.JDE_Handlings
                                 join t in db.JDE_Tenants on h.TenantId equals t.TenantId
                                 join u in db.JDE_Users on h.UserId equals u.UserId
                                 where h.TenantId == tenants.FirstOrDefault().TenantId && u.UserId==UserId && (h.IsCompleted==false || h.IsCompleted==null)
                                 select new
                                 {
                                     HandlingId = h.HandlingId,

                                 });
                    if (handlings.Any())
                    {
                        return Ok(true);
                    }
                    else
                    {
                        return Ok(false);
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
        [Route("GetMechanics")]
        public IHttpActionResult GetMechanics(string token)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var users = (from us in db.JDE_Users
                                 join t in db.JDE_Tenants on us.TenantId equals t.TenantId
                                 join u in db.JDE_Users on us.CreatedBy equals u.UserId
                                 where us.TenantId == tenants.FirstOrDefault().TenantId && us.isMechanic==true
                                 select new
                                 {
                                     UserId = us.UserId,
                                     Name = us.Name,
                                     Surname = us.Surname,
                                     Password = us.Password,
                                     isMechanic = us.isMechanic,
                                     TenantId = us.TenantId,
                                     TenantName = t.TenantName,
                                     CreatedOn = us.CreatedOn,
                                     CreatedBy = us.CreatedBy,
                                     CreatedByName = u.Name + " " + u.Surname,
                                     LastLoggedOn = us.lastLoggedOn,
                                     MesLogin = us.MesLogin
                                 });
                    if (users.Any())
                    {
                        return Ok(users);
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
        [Route("GetUser")]
        [ResponseType(typeof(JDE_Users))]
        public IHttpActionResult GetUser(string token, int id)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var users = (from us in db.JDE_Users
                                 join t in db.JDE_Tenants on us.TenantId equals t.TenantId
                                 join u in db.JDE_Users on us.CreatedBy equals u.UserId
                                 where us.TenantId == tenants.FirstOrDefault().TenantId && us.UserId == id
                                 select new
                                 {
                                     UserId = us.UserId,
                                     Name = us.Name,
                                     Surname = us.Surname,
                                     Password = us.Password,
                                     isMechanic = us.isMechanic,
                                     TenantId = us.TenantId,
                                     TenantName = t.TenantName,
                                     CreatedOn = us.CreatedOn,
                                     CreatedBy = us.CreatedBy,
                                     CreatedByName = u.Name + " " + u.Surname,
                                     LastLoggedOn = us.lastLoggedOn,
                                     MesLogin = us.MesLogin
                                 });
                    if (users.Any())
                    {
                        return Ok(users.FirstOrDefault());
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
        [Route("EditUser")]
        [ResponseType(typeof(void))]

        public IHttpActionResult EditUser(string token, int id,  int UserId, JDE_Users jDE_Users)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var users = db.JDE_Users.AsNoTracking().Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.UserId == id);
                    if (users.Any())
                    {
                        JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Edycja użytkownika", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(users.FirstOrDefault()), NewValue=new JavaScriptSerializer().Serialize(jDE_Users) };
                        db.JDE_Logs.Add(Log);
                        db.Entry(jDE_Users).State = EntityState.Modified;

                        try
                        {
                            db.SaveChanges();
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            if (!JDE_UsersExists(id))
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
        [Route("CreateUser")]
        [ResponseType(typeof(JDE_Users))]
        public IHttpActionResult CreateUser(string token, JDE_Users jDE_Users, int UserId)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    jDE_Users.TenantId = tenants.FirstOrDefault().TenantId;
                    db.JDE_Users.Add(jDE_Users);
                    db.SaveChanges();
                    JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Utworzenie użytkownika", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, NewValue = new JavaScriptSerializer().Serialize(jDE_Users) };
                    db.JDE_Logs.Add(Log);
                    db.SaveChanges();
                    return Ok(jDE_Users);
                    //return CreatedAtRoute("DefaultApi", new { id = jDE_Users.UserId }, jDE_Users);
                }
                else
                {
                    return NotFound();
                }
            }else
            {
                return NotFound();
            }
        }

        [HttpDelete]
        [Route("DeleteUser")]
        [ResponseType(typeof(JDE_Users))]
        public IHttpActionResult DeleteUser(string token, int id, int UserId)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var users = db.JDE_Users.Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.UserId == id);
                    if (users.Any())
                    {
                        JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Usunięcie użytkownika", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(users.FirstOrDefault()) };
                        db.JDE_Users.Remove(users.FirstOrDefault());  
                        db.JDE_Logs.Add(Log);
                        db.SaveChanges();

                        return Ok(users.FirstOrDefault());
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

        [HttpPut]
        [Route("LogIn")]
        [ResponseType(typeof(void))]
        public IHttpActionResult LogIn(string token, int id, JDE_Users jDE_Users)
        {
            if (token != null && token.Length > 0)
            {
                var tenants = db.JDE_Tenants.Where(t => t.TenantToken == token.Trim());
                if (tenants.Any())
                {
                    var users = db.JDE_Users.Where(u => u.TenantId == tenants.FirstOrDefault().TenantId && u.UserId == id);
                    if (users.Any())
                    {
                        JDE_Logs Log = new JDE_Logs { UserId = id, Description = "Logowanie", TenantId = tenants.FirstOrDefault().TenantId, Timestamp = DateTime.Now };
                        jDE_Users.lastLoggedOn = DateTime.Now;
                        db.Entry(jDE_Users).State = EntityState.Modified;
                        db.JDE_Logs.Add(Log);
                        try
                        {
                            db.SaveChanges();
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            if (!JDE_UsersExists(id))
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool JDE_UsersExists(int id)
        {
            return db.JDE_Users.Count(e => e.UserId == id) > 0;
        }
    }
}