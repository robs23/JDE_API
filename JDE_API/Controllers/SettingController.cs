using JDE_API.Static;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace JDE_API.Controllers
{
    public class SettingController : ApiController
    {

        

        [HttpGet]
        [Route("PageSize")]
        public IHttpActionResult PageSize()
        {
            return Ok(RuntimeSettings.PageSize);
        }

        [HttpGet]
        [Route("GetUniqueToken")]
        public IHttpActionResult GetUniqueToken()
        {

            return Ok(Static.Utilities.GetToken());
        }

        
    }
}
