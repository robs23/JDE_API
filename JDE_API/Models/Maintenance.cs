using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace JDE_API.Models
{
    public class Maintenance : Process
    {
        private Models.DbModel db = new Models.DbModel();
        public int? AssumedTime
        {
            get
            {
                var allTimes = (from prac in db.JDE_ProcessActions
                         join a in db.JDE_Actions on prac.ActionId equals a.ActionId
                         where prac.ProcessId == ProcessId
                         select a.GivenTime ).Sum();
                return allTimes;

            }
        }
    }
}