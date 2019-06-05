using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace JDE_API.Static
{
    public static class RuntimeSettings
    {
        public static int PageSize {
            get
            {
                return 200;
            }
        }

        public static string Path2Files
        {
            get
            {
                return $@"{HttpContext.Current.Server.MapPath("~")}\Files\";
            }
        }

        public static int MaxFileContentLength
        {
            get
            {
                return 1024 * 1024 * 5; //Size = 5 MB;
            }
        }
    }
}