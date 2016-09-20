using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace DST_PMPortal
{
    [DataContract]
    public class Project : IComparable<Project>
    {
        public Project(string _TName = "unspecefied", string _JTName = "unspecefied", string _PMName = "unspecefied")
        {
            TName = _TName;
            JTName = _JTName;
            PMName = _PMName;
        }

        [DataMember]
        internal string TName;
        [DataMember]
        internal string JTName;
        [DataMember]
        internal string PMName;

        [DataMember]
        internal int candyToEvolve;
        [DataMember]
        internal string nextStage;

        public int CompareTo(Project other)
        {
            return TName.CompareTo(other.TName);
        }
    }

}
