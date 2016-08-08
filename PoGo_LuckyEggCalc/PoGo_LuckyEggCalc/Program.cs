using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.IO;
using NLog;
using NLog.Targets;
using Microsoft.Office.Interop.Excel;
using System.Reflection;
using System.Runtime.Serialization.Json;

namespace PoGo_LuckyEggCalc
{
    class Program : AppBase
    {
        public List<Pokemon> Pokedex { get; set; }

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
            pokedex.userName = "michaeldsuttie";
            var poke1 = new Pokemon("Pidgey", true, 68, 462, 12, "Pidgeotto");
            var poke2 = new Pokemon("Ratatta", true, 56, 423, 25, "Ratticate");
            pokedex.Inventory.Add(poke2);
            pokedex.Inventory.Add(poke1);

            Console.WriteLine("Initialized Pokedex:");
            foreach (var o in pokedex.Inventory)
            {
                Console.WriteLine($"Name: { o.name} | Qty: {o.qtyPokemon} | Candies: {o.qtyCandy} | CandyToEvolve: {o.candyToEvolve} | NextStage: {o.nextStage}");
            }
            Console.WriteLine();

            Console.WriteLine("Writing pokedex to json file...");
            JsonOperations.WriteDataToFile(jsonFilePath, pokedex);
            Console.WriteLine();

            Console.WriteLine("Reading pokedex from json file...");
            var readDex = new Pokedex();
            readDex = JsonOperations.ReadDataFromFile(jsonFilePath, pokedex);
            Console.WriteLine($"Pokedex UserName: {readDex.userName}");
            foreach (var o in readDex.Inventory)
            {
                Console.WriteLine($"Name: { o.name} | Qty: {o.qtyPokemon} | Candies: {o.qtyCandy} | CandyToEvolve: {o.candyToEvolve} | NextStage: {o.nextStage}");
            }

            Console.WriteLine("Writing json data to xlsx...");
            ExcelOperations.WriteJSONtoXLSX(excelFilePath, "'userName': 'michaeldsuttie'");

            ExitApp();
            goto RERUN;
        }
    }



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

    class AppBase
    {
        internal static Logger DebugLog;
        internal static Logger ConfigLogger(Logger _logger, string _logName)
        {
            _logger = LogManager.GetLogger(_logName);
            var _fileTarget = (FileTarget)LogManager.Configuration.LoggingRules.First().Targets.First();
            _fileTarget.FileName = $@"{AppDomain.CurrentDomain.BaseDirectory}\logs\{DateTime.Now.ToString("yyyy-MM-dd")}\{DateTime.Now.ToString("HH-mm-ss")}.txt";
            LogManager.ReconfigExistingLoggers();
            return _logger;
        }
        internal static void WriteToLog(Logger _logger, string _severity, string _message, bool _suppressConsoleOutput = true)
        {
            _severity = _severity.ToLower();

            if (!_suppressConsoleOutput)
            {
                Console.WriteLine(_message);
            }

            switch (_severity)
            {
                case "debug":
                    _logger.Debug(_message);
                    break;
                case "info":
                    _logger.Info(_message);
                    break;
                case "error":
                    _logger.Error(_message);
                    break;
                default:
                    _logger.Error($"Incorrect severity specified for message: '{_message}'");
                    break;
            }
        }
        internal static void ExitApp()
        {
            while (true)
            {
                Console.Write(Environment.NewLine);
                WriteToLog(DebugLog, "info", "Do you want to run again? [Y/N]", false);
                var keyPress = Console.ReadLine().ToUpper();
                if (keyPress == "Y")
                {
                    Console.Write(Environment.NewLine);
                    WriteToLog(DebugLog, "info", "Re-run initiated...", false);
                    return;
                }
                else if (keyPress == "N")
                {
                    Console.Write(Environment.NewLine);
                    WriteToLog(DebugLog, "info", "Exiting app...", false);
                    Environment.Exit(0);
                }
            }
        }
    }

    class ExcelOperations
    {
        internal static void WriteJSONtoXLSX(string _filePath, string _json)
        {
            //create file
            using (var stream = new FileStream(_filePath, FileMode.Create, FileAccess.ReadWrite))
            {
                var xlApp = new Application();
                var xlWorkbook = xlApp.Application.Workbooks.Open(_filePath);
                //this.Application.Workbooks.Open(@"C:\Test\YourWorkbook.xlsx")
                var xlWorksheet = xlWorkbook.Worksheets[1];
                var usedRange = xlWorksheet.UsedRange;
                //var lastRow = usedRange.Find("*", SearchOrder: XlSearchOrder.xlByRows, SearchDirection: XlSearchDirection.xlPrevious).Row;
                //var range = xlWorksheet.Range[xlWorksheet.Cells[3, 2], xlWorksheet.Cells[lastRow, 9]];

                xlWorksheet.Cells[1, 1] = "It works!";

                xlWorkbook.Save();

                xlWorkbook.Close();
                xlApp.Quit();
            }
        }
        internal static Dictionary<string, Pokemon> Getdata()
        {
            throw new NotImplementedException();
        }
    }

    class JsonOperations
    {
        internal static void WriteDataToFile(string _filePath, Pokedex _pokedex)
        {
            using (var stream = new FileStream(_filePath, FileMode.Create, FileAccess.ReadWrite))
            {
                var ser = new DataContractJsonSerializer(typeof(Pokedex));
                ser.WriteObject(stream, _pokedex);
            }
        }
        internal static Pokedex ReadDataFromFile(string _filePath, Pokedex _pokedex)
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
