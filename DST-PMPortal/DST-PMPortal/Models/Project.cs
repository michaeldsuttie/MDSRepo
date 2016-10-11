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

        public int CompareTo(Project other)
        {
            return TName.CompareTo(other.TName);
        }

        //[Key]
        //[DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ProjectId { get; set; } //uses Toggl Project Id for Primary Key
        public string Name { get; set; }
        public string Client { get; set; }
        public string JobTrackProject { get; set; }
        //public virtual ObservableCollection<Task> Tasks { get; set; }
        //[NotMapped]
        //public bool Modified { get; set; }

    }
}
