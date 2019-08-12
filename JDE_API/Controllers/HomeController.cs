using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace JDE_API.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }

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

            return View(tokens);
        }
    }
}
