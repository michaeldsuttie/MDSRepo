using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreTogglTest.Models
{
    public class TogglTimeEntry
    {
        public int Id { get; set; }
        public string User { get; set; }
        public string Project { get; set; }
        public string Description { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public int Duration { get; set; }
        public string Task { get; set; }
        public string Client { get; set; }
        public DateTime Updated { get; set; }
        public int TaskID { get; set; }
        public bool NoError { get; set; }
    }
}
