using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace DST_PMPortal
{
    [DataContract]
    class Task
    {
        internal string name;
        internal string costCode;
        internal bool billable;
        internal double budgetedHours;
        internal double remainingHours;
    }
}
