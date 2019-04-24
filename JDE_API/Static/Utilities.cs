using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;

namespace JDE_API.Static
{
    public static class Utilities
    {
        public static string uniqueToken()
        {
            string token;
            Guid g = Guid.NewGuid();
            token = Convert.ToBase64String(g.ToByteArray());
            token = token.Replace("=", "");
            token = token.Replace("+", "");
            token = token.Replace("/", "");
            token = token.Replace("\\", "");
            return token;
        }

        public static string GetToken()
        {
            Models.DbModel db = new Models.DbModel();
            string token;
            bool duplicate = true;
            do
            {
                Guid g = Guid.NewGuid();
                token = Convert.ToBase64String(g.ToByteArray());
                token = token.Replace("=", "");
                token = token.Replace("+", "");
                token = token.Replace("/", "");

                //check if newly created token is unique
                if (!db.JDE_Tenants.Where(x => x.TenantToken == token).Any())
                {
                    if (!db.JDE_Places.Where(x => x.PlaceToken == token).Any())
                    {
                        if (!db.JDE_Parts.Where(x => x.Token == token).Any())
                        {
                            duplicate = false;
                        }
                    }
                }

            } while (duplicate);


            return token;
        }

        public static string ToAscii(string s)
        {
            return String.Join("",
         s.Normalize(NormalizationForm.FormD)
        .Where(c => char.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark));
        }
    }
}