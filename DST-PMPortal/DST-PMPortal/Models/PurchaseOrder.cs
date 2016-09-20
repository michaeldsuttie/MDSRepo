using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;


namespace DST_PMPortal
{
    [DataContract]
    class PurchaseOrder
    {
        internal string customer;
        internal string number;
        internal double amount;
        internal DateTime date;
    }
}
