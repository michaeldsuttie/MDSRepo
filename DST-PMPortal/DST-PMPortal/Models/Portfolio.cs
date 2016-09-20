using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;


namespace DST_PMPortal
{
    [DataContract]
    class Portfolio
    {

        public Portfolio(string _PMName = "unspecefied")
        {
            PMName = _PMName;
        }

        [DataMember]
        internal string PMName;
    }
}
