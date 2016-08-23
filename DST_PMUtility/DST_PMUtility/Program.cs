using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DST_PMUtility
{
    class Program
    {
        static void Main(string[] args)
        {
            DebugLog = ConfigLogger(DebugLog, "DebugLog");
            WriteToLog(DebugLog, "info", $"{Assembly.GetExecutingAssembly().FullName}", false);
            var workingDir = AppDomain.CurrentDomain.BaseDirectory;
            var jsonFileName = "pokedex.json";
            var jsonFilePath = $"{workingDir}{jsonFileName}";

            var excelFileName = "pokedex.xlsx";
            var excelFilePath = $"{workingDir}{excelFileName}";

        RERUN:
            var pokedex = new Pokedex();
            //pokedex.userName = "michaeldsuttie";
            //var poke1 = new Pokemon("Pidgey", true, 68, 462, 12, "Pidgeotto");
            //var poke2 = new Pokemon("Ratatta", true, 56, 423, 25, "Ratticate");
            //pokedex.Inventory.Add(poke2);
            //pokedex.Inventory.Add(poke1);

            //Console.WriteLine("Initialized Pokedex:");
            //foreach (var o in pokedex.Inventory)
            //{
            //    Console.WriteLine($"Name: { o.name} | Qty: {o.qtyPokemon} | Candies: {o.qtyCandy} | CandyToEvolve: {o.candyToEvolve} | NextStage: {o.nextStage}");
            //}
            //Console.WriteLine();

            //Console.WriteLine("Writing Initialized pokedex to json file...");
            //JsonOperations.WritePokedexToFile(jsonFilePath, pokedex);
            //Console.WriteLine();

            Console.WriteLine("Reading pokedex from json file...");
            var readDex = new Pokedex();
            readDex = JsonOperations.ReadPokedexFromFile(jsonFilePath, pokedex);
            Console.WriteLine($"Pokedex UserName: {readDex.userName}");
            foreach (var o in readDex.Inventory)
            {
                Console.WriteLine($"Name: { o.name} | Qty: {o.qtyPokemon} | Candies: {o.qtyCandy} | CandyToEvolve: {o.candyToEvolve} | NextStage: {o.nextStage}");
            }

            Console.WriteLine("Writing pokedex (read from JSON) to xlsx file...");
            ExcelOperations.WritePokedextoXLSX(excelFilePath, readDex);

            Console.WriteLine("Reading pokedex from xlsx file...");
            readDex = new Pokedex();
            readDex = ExcelOperations.GetData(excelFilePath);
            Console.WriteLine($"Pokedex UserName: {readDex.userName}");
            foreach (var o in readDex.Inventory)
            {
                Console.WriteLine($"Name: { o.name} | Qty: {o.qtyPokemon} | Candies: {o.qtyCandy} | CandyToEvolve: {o.candyToEvolve} | NextStage: {o.nextStage}");
            }


            ExitApp();
            goto RERUN;

        }
    }
}
