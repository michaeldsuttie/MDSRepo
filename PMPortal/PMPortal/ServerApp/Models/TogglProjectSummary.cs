using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMPortal.ServerApp.Models
{
    public class TogglProjectSummary
    {
        public TogglProjectSummary()
        {
            //Id = -1;
            Name = "Undefined";
            //End = DateTime.Now;
            //Start = End.AddDays(-14);
            BillingPeriod = "Undefined";
            TaskSummaries = new Dictionary<string, TogglTaskSummary>();
        }
        //public int Id { get; set; }
        public string Name { get; set; }
        //public DateTime Start { get; set; }
        //public DateTime End { get; set; }
        public string BillingPeriod { get; set; }
        public Dictionary<string,TogglTaskSummary> TaskSummaries { get; set; }
    }
}
