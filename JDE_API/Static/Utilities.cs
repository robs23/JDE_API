using JDE_API.Controllers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

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

        public static List<IProcessable> FilterByLength(List<IProcessable> nItems, string length)
        {
            var min = Regex.Match(length, @"\d+").Value;
            int mins = 0;
            int.TryParse(min, out mins);
            var sign = length.Substring(0, length.Length - min.Length);
            if ((sign.Equals(">") || sign.Equals("<") || sign.Equals("=<") || sign.Equals("<=") || sign.Equals("=>") || sign.Equals(">=") || sign.Equals("=")) && mins >= 0)
            {
                // don't do anything unless you've got both min and sign
                if (sign.Equals("="))
                {
                    nItems = nItems.Where(i => i.Length == mins).ToList();
                }
                else if (sign.Equals("<=") || sign.Equals("=<"))
                {
                    nItems = nItems.Where(i => i.Length <= mins).ToList();
                }
                else if (sign.Equals(">=") || sign.Equals("=>"))
                {
                    nItems = nItems.Where(i => i.Length >= mins).ToList();
                }
                else if (sign.Equals(">"))
                {
                    nItems = nItems.Where(i => i.Length > mins).ToList();
                }
                else if (sign.Equals("<"))
                {
                    nItems = nItems.Where(i => i.Length < mins).ToList();
                }

            }
            return nItems;
        }

        public static List<IProcessable> FilterByStatus(List<IProcessable> nItems, string status)
        {
            int start = 0;
            int end = 0;
            string word = "";

            if (status.Contains("!Status.ToLower().Contains") || status.Contains("Status<>"))
            {
                //Doesn't contain or different than
                //let's get just query parameter
                if (status.Contains("Contains"))
                {
                    word = "!Status.ToLower().Contains";
                }
                else
                {
                    word = "Status<>";
                }
                status = status.Replace(word, "");
                start = status.IndexOf("\"");
                end = status.IndexOf("\"", start + 1);
                status = status.Substring(start + 1, end - (start + 1));
                if ("Zrealizowany".Contains(status) || ("Zrealizowany".ToLower().Contains(status)))
                {
                    nItems = nItems.Where(i => i.IsSuccessfull == false).ToList();
                }
                else if ("Zakończony".Contains(status) || ("Zakończony".ToLower().Contains(status)))
                {
                    nItems = nItems.Where(i => i.IsCompleted == false).ToList();
                }
                else if ("Wstrzymany".Contains(status) || ("Wstrzymany".ToLower().Contains(status)))
                {
                    nItems = nItems.Where(i => i.IsFrozen == false).ToList();
                }
                else if ("Rozpoczęty".Contains(status) || ("Rozpoczęty".ToLower().Contains(status)))
                {
                    nItems = nItems.Where(i => i.IsActive == false).ToList();
                }
                else if ("Planowany".Contains(status) || ("Planowany".ToLower().Contains(status)))
                {
                    nItems = nItems.Where(i => i.IsCompleted == true || i.IsSuccessfull == true || i.IsActive == true || i.IsFrozen == true).ToList();
                }
            }
            else if (status.Contains("Status.ToLower().Contains") || status.Contains("Status="))
            {
                //Contains or equal to
                //let's get just query parameter
                if (status.Contains("Contains"))
                {
                    word = "Status.ToLower().Contains";
                }
                else
                {
                    word = "Status=";
                }
                status = status.Replace(word, "");
                start = status.IndexOf("\"");
                end = status.IndexOf("\"", start + 1);
                status = status.Substring(start + 1, end - (start + 1));
                if ("Zrealizowany".Contains(status) || ("Zrealizowany".ToLower().Contains(status)))
                {
                    nItems = nItems.Where(i => i.IsSuccessfull == true).ToList();
                }
                else if ("Zakończony".Contains(status) || ("Zakończony".ToLower().Contains(status)))
                {
                    nItems = nItems.Where(i => i.IsCompleted == true).ToList();
                }
                else if ("Wstrzymany".Contains(status) || ("Wstrzymany".ToLower().Contains(status)))
                {
                    nItems = nItems.Where(i => i.IsCompleted == false && i.IsFrozen == true).ToList();
                }
                else if ("Rozpoczęty".Contains(status) || ("Rozpoczęty".ToLower().Contains(status)))
                {
                    nItems = nItems.Where(i => i.IsCompleted == false && i.IsFrozen == false && i.IsActive == true).ToList();
                }
                else if ("Planowany".Contains(status) || ("Planowany".ToLower().Contains(status)))
                {
                    nItems = nItems.Where(i => i.IsCompleted == false && i.IsSuccessfull == false && i.IsActive == false && i.IsFrozen == false).ToList();
                }
                else
                {
                    nItems.Clear();
                }

            }
            return nItems;
        }

        public static List<IProcessable> FilterByAssignedUserNames(List<IProcessable> nItems, string assignedUserNames)
        {
            int start = 0;
            int end = 0;
            string word = "";

            if (assignedUserNames.Contains("!AssignedUserNames.ToLower().Contains") || assignedUserNames.Contains("AssignedUserNames<>"))
            {
                //Doesn't contain or different than
                //let's get just query parameter
                if (assignedUserNames.Contains("Contains"))
                {
                    word = "!AssignedUserNames.ToLower().Contains";
                }
                else
                {
                    word = "AssignedUserNames<>";
                }
                assignedUserNames = assignedUserNames.Replace(word, "");
                start = assignedUserNames.IndexOf("\"");
                end = assignedUserNames.IndexOf("\"", start + 1);
                assignedUserNames = assignedUserNames.Substring(start + 1, end - (start + 1));
                
            }else if(assignedUserNames.Contains("AssignedUserNames.ToLower().Contains") || assignedUserNames.Contains("AssignedUserNames="))
            {
                if (assignedUserNames.Contains("Contains"))
                {
                    word = "AssignedUserNames.ToLower().Contains";
                }
                else
                {
                    word = "AssignedUserNames=";
                }
                assignedUserNames = assignedUserNames.Replace(word, "");
                start = assignedUserNames.IndexOf("\"");
                end = assignedUserNames.IndexOf("\"", start + 1);
                assignedUserNames = assignedUserNames.Substring(start + 1, end - (start + 1));
                nItems = nItems.Where(i => i.AssignedUserNames.ToLower().Contains(assignedUserNames)).ToList();
            }

            return nItems;
        }

        public static void ProduceThumbnail(string path)
        {
            string fileName = Path.GetFileName(path);
            using (Image image = Image.FromFile(path))
            {
                Image thumb = ResizeImage(image, new Size(120, 120), true);
                Bitmap nBitmap = new Bitmap(thumb);
                thumb.Dispose();
                thumb = null;
                nBitmap.Save(Path.Combine(RuntimeSettings.Path2Files + "\\Thumbnails", fileName));
            }
        }

        public static Image ResizeImage(Image image, Size size, bool preserveAspectRatio = true)
        {
            int newWidth;
            int newHeight;
            if (preserveAspectRatio)
            {
                var originalWidth = image.Width;
                var originalHeight = image.Height;
                var percentWidth = size.Width / (float)originalWidth;
                var percentHeight = size.Height / (float)originalHeight;
                var percent = percentHeight < percentWidth ? percentHeight : percentWidth;
                newWidth = (int)(originalWidth * percent);
                newHeight = (int)(originalHeight * percent);
            }
            else
            {
                newWidth = size.Width;
                newHeight = size.Height;
            }
            Image newImage = new Bitmap(newWidth, newHeight);
            using (var graphicsHandle = Graphics.FromImage(newImage))
            {
                graphicsHandle.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphicsHandle.SmoothingMode = SmoothingMode.HighQuality;
                graphicsHandle.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphicsHandle.DrawImage(image, 0, 0, newWidth, newHeight);
            }
            return newImage;
        }

        public async static void  DeleteAttachment(string name)
        {

        }
    }
}