using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMPortal.ServerApp.Models
{
    public class TogglTaskSummary
    {
        public TogglTaskSummary()
        {
            //Id = -1;
            //Name = "Undefined";
            UserTaskSummaries = new Dictionary<string, double>();
            TotalTaskHours = 0;
        }
        //public int Id { get; set; }
        //public string Name { get; set; }
        public Dictionary<string,double> UserTaskSummaries { get; set; }
        public double TotalTaskHours { get; set; }
    }
}
