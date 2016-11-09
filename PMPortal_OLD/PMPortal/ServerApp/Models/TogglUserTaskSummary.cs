using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMPortal.ServerApp.Models
{
    public class TogglUserTaskSummary
    {
        public string User { get; set; }
        public DateTime Date { get; set; }
        public double Duration { get; set; }
    }
}
