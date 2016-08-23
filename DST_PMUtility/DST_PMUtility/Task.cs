using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DST_PMUtility
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
