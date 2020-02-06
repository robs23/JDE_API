using JDE_API.Controllers;
using JDE_API.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using ProcessStatus = JDE_API.Controllers.ProcessStatus;

namespace JDE_API.Static
{
    public static class Utilities
    {
        //private static Models.DbModel db = new Models.DbModel();
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

        public async static Task CompleteAllProcessesOfTheTypeInThePlaceAsync(DbModel db, int thePlace, int theType, int excludeProcess, int UserId, string reasonForClosure = null)
        {

            bool? requireClosing = db.JDE_ActionTypes.Where(i => i.ActionTypeId == theType).FirstOrDefault().ClosePreviousInSamePlace;
            if(requireClosing == null) { requireClosing = false; }
            if ((bool)requireClosing)
            {
                IQueryable<JDE_Processes> processes = null;
                processes = db.JDE_Processes.Where(p => p.PlaceId == thePlace && p.ActionTypeId==theType && p.ProcessId<excludeProcess && (p.IsCompleted == false || p.IsCompleted == null) && (p.IsSuccessfull == false || p.IsSuccessfull == null));
                if (processes.Any())
                {
                    foreach(var p in processes)
                    {
                        await CompleteProcessAsync(db,(int)processes.FirstOrDefault().TenantId, p, UserId, reasonForClosure);
                    }
                }
            }
        }

        public async static Task CompleteProcessAsync(DbModel db, int tenantId, JDE_Processes item, int UserId, string reasonForClosure = null)
        {
            string OldValue = new JavaScriptSerializer().Serialize(item);
            item.FinishedOn = DateTime.Now;
            item.FinishedBy = UserId;
            item.IsActive = false;
            item.IsCompleted = true;
            item.IsFrozen = false;
            item.LastStatus = (int)ProcessStatus.Finished;
            item.LastStatusBy = UserId;
            item.LastStatusOn = DateTime.Now;
            var User = db.JDE_Users.AsNoTracking().Where(u => u.UserId == UserId).FirstOrDefault();
            if (reasonForClosure == null)
            {
                item.Output = $"Przymusowe zamknięcie zgłoszenia przez {User.Name + " " + User.Surname}";
            }
            else
            {
                item.Output = reasonForClosure;
            }
            await CompleteProcessesHandlingsAsync(db, item.ProcessId, UserId);
            JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = "Zamknięcie zgłoszenia", TenantId = tenantId, Timestamp = DateTime.Now, OldValue = OldValue, NewValue = new JavaScriptSerializer().Serialize(item) };
            db.JDE_Logs.Add(Log);
            db.Entry(item).State = EntityState.Modified;
            //db.SaveChanges();
        }

        public static async Task CompleteProcessesHandlingsAsync(DbModel db, int ProcessId, int UserId, string reasonForClosure = null)
        {
            //it completes all open handlings for given process
            string descr = string.Empty;
            var items = db.JDE_Handlings.AsNoTracking().Where(p => p.ProcessId == ProcessId && p.IsCompleted == false);
            var User = db.JDE_Users.AsNoTracking().Where(u => u.UserId == UserId).FirstOrDefault();

            if (items.Any())
            {
                foreach (var item in items)
                {
                    item.FinishedOn = DateTime.Now;
                    item.IsActive = false;
                    item.IsFrozen = false;
                    item.IsCompleted = true;
                    if (reasonForClosure == null)
                    {
                        item.Output = $"Obsługa została zakończona przy zamykaniu zgłoszenia przez {User.Name + " " + User.Surname}";
                    }
                    else
                    {
                        item.Output = reasonForClosure;
                    }

                    descr = "Zakończenie obsługi";
                    JDE_Logs Log = new JDE_Logs { UserId = UserId, Description = descr, TenantId = User.TenantId, Timestamp = DateTime.Now, OldValue = new JavaScriptSerializer().Serialize(items.FirstOrDefault()), NewValue = new JavaScriptSerializer().Serialize(item) };
                    db.JDE_Logs.Add(Log);
                    db.Entry(item).State = EntityState.Modified;
                }
                try
                {
                    //db.SaveChanges();
                }
                catch (Exception ex)
                {

                }
            }
        }
    }
}