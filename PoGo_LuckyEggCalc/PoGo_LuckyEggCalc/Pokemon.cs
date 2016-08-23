using System;
using System.Runtime.Serialization;

namespace PoGo_LuckyEggCalc
{
    [DataContract]
    public class Pokemon : IComparable<Pokemon>
    {
        public Pokemon(string _name = "", bool _inPokedex = false, int _qtyPokemon = -1, int _qtyCandy = -1, int _candyToEvolve = -1, string _nextStage = "None")
        {
            name = _name;
            inPokedex = _inPokedex;
            qtyPokemon = _qtyPokemon;
            qtyCandy = _qtyCandy;
            candyToEvolve = _candyToEvolve;
            nextStage = _nextStage;
        }

        [DataMember]
        internal string name;
        [DataMember]
        internal bool inPokedex;
        [DataMember]
        internal int qtyPokemon;
        [DataMember]
        internal int qtyCandy;
        [DataMember]
        internal int candyToEvolve;
        [DataMember]
        internal string nextStage;

        public int CompareTo(Pokemon other)
        {
            return name.CompareTo(other.name);
        }
    }
}
