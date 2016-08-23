using System.IO;
using System.Runtime.Serialization.Json;

namespace PoGo_LuckyEggCalc
{
    class JsonOperations
    {
        internal static void WritePokedexToFile(string _filePath, Pokedex _pokedex)
        {
            using (var stream = new FileStream(_filePath, FileMode.Create, FileAccess.ReadWrite))
            {
                var ser = new DataContractJsonSerializer(typeof(Pokedex));
                ser.WriteObject(stream, _pokedex);
            }
        }
        internal static Pokedex ReadPokedexFromFile(string _filePath, Pokedex _pokedex)
        {
            using (var stream = new FileStream(_filePath, FileMode.Open, FileAccess.Read))
            {
                var ser = new DataContractJsonSerializer(typeof(Pokedex));
                _pokedex = (Pokedex)ser.ReadObject(stream);
            }
            return _pokedex;
        }
    }

}
