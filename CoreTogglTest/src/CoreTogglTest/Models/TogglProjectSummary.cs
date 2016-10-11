using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreTogglTest.Models
{
    public class TogglProjectSummary
    {
        public TogglProjectSummary()
        {
            //Id = -1;
            Name = "Undefined";
            TaskSummaries = new Dictionary<string, TogglTaskSummary>();
        }
        //public int Id { get; set; }
        public string Name { get; set; }
        public Dictionary<string,TogglTaskSummary> TaskSummaries { get; set; }
    }
}
