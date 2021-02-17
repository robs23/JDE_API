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
                string path = $@"{HttpContext.Current.Server.MapPath("~")}\Files\";
                if (!System.IO.Directory.Exists(path))
                {
                    System.IO.Directory.CreateDirectory(path);
                    System.IO.Directory.CreateDirectory(System.IO.Path.Combine(path, "Thumbnails"));
                }
                return path;
            }
        }

        public static string Path2Thumbs
        {
            get
            {
               return $@"{Path2Files}Thumbnails\";
                
            }
        }

        public static int MaxFileContentLength
        {
            get
            {
                return 1024 * 1024 * 2000; //Size = 5 MB;
            }
        }
    }
}