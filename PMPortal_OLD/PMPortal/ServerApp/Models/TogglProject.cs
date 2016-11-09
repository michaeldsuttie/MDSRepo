using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace PMPortal.ServerApp.Models
{
    public class TogglProject
    {
        public int Id { get; set; } 
        public string Name { get; set; }
        public string Client { get; set; }
        public string JTProject { get; set; }
        public IEnumerable<Task> Tasks { get; set; }
        public IEnumerable<TogglTimeEntry> Entries { get; set; }
        //public bool Modified { get; set; }
    }
}
