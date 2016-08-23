using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace DST_PMUtility
{
    [DataContract]
    public class Project : IComparable<Project>
    {
        public Project(string _name = "", bool _inPokedex = false, int _qtyPokemon = -1, int _qtyCandy = -1, int _candyToEvolve = -1, string _nextStage = "None")
        {
            name = _name;
            inPokedex = _inPokedex;
            qtyPokemon = _qtyPokemon;
            qtyCandy = _qtyCandy;
            candyToEvolve = _candyToEvolve;
            nextStage = _nextStage;
        }

        [DataMember]
        internal string TName;
        [DataMember]
        internal string JTName;
        [DataMember]
        internal string managerName;
        [DataMember]
        internal List<string> coworkers;
        [DataMember]
        internal int ;
        [DataMember]
        internal int candyToEvolve;
        [DataMember]
        internal string nextStage;

        public int CompareTo(Project other)
        {
            return name.CompareTo(other.name);
        }
    }

}
