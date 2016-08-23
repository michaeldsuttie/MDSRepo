using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PoGo_LuckyEggCalc
{
    [DataContract]
    class Pokedex
    {
        public Pokedex()
        {
            Inventory = new List<Pokemon>();
        }

        [DataMember]
        internal string userName = "NotSpecified";
        [DataMember]
        internal List<Pokemon> Inventory { get; set; }
    }

}
