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

namespace JDE_API.Controllers
{
    public class TenantController : ApiController
    {
        private Models.DbModel db = new Models.DbModel();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool JDE_TenantsExists(int id)
        {
            return db.JDE_Tenants.Count(e => e.TenantId == id) > 0;
        }
    }
}