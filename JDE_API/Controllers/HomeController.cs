using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web;
using System.Web.Http.Description;
using System.Web.Mvc;

namespace JDE_API.Controllers
{
    public class HomeController : Controller
    {
        [Route(""), HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        public RedirectResult Index()
        {
            return Redirect("/swagger/");
        }

        //public ActionResult Index()
        //{
        //    ViewBag.Title = "Home Page";
        //    return View();
        //}

        public ActionResult GenerateUniqueTokens(int NumberOfItems)
        {
            List<string> tokens = new List<string>();
            if (NumberOfItems > 0)
            {
                for (int i = 0; i <= NumberOfItems; i++)
                {
                    string token = Static.Utilities.GetToken();
                    if (!tokens.Where(item => tokens.Contains(token)).Any())   
                    {
                        tokens.Add(token);
                    }
                    
                }
            }

            return View("GenerateUniqueTokens", tokens);
        }

        public ActionResult ClearCache()
        {
            string msg = string.Empty;
            try
            {
                int httpContextCashRemovedCount = 0;
                int memoryCashRemovedCount = 0;

                foreach (System.Collections.DictionaryEntry entry in System.Web.HttpContext.Current.Cache)
                {
                    System.Web.HttpContext.Current.Cache.Remove((string)entry.Key);
                    httpContextCashRemovedCount++;
                }
                MemoryCache cache = MemoryCache.Default;
                List<string> cacheKeys = cache.Select(kvp => kvp.Key).ToList();

                foreach (string cacheKey in cacheKeys)
                {
                    cache.Remove(cacheKey);
                    memoryCashRemovedCount++;
                }
                if(httpContextCashRemovedCount > 0 || memoryCashRemovedCount > 0)
                {
                    msg = $"Usunięto {httpContextCashRemovedCount} pozycji z cashu HttpContext i/lub {memoryCashRemovedCount} pozycji z cashu pamięci";
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }
            
            return View("ClearCache", msg);
        }
    }
}
