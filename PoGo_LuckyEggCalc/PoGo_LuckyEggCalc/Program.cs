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

            //Console.WriteLine("Writing json data to xlsx...");
            //ExcelOperations.WriteJSONtoXLSX(excelFilePath, "'userName': 'michaeldsuttie'");

            //Console.WriteLine("Writing read Pokedex to xlsx...");
            //ExcelOperations.WriteData(excelFilePath, readDex);
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
            var xlApp = new Application();
            var xlWorkbook = xlApp.Application.Workbooks.Add();
            var xlWorksheet = xlWorkbook.Worksheets[1];
            var usedRange = xlWorksheet.UsedRange;
            //var lastRow = usedRange.Find("*", SearchOrder: XlSearchOrder.xlByRows, SearchDirection: XlSearchDirection.xlPrevious).Row;
            //var range = xlWorksheet.Range[xlWorksheet.Cells[3, 2], xlWorksheet.Cells[lastRow, 9]];

            //xlWorksheet.Cells[1, 1] = $"{}";

            xlWorkbook.SaveAs(_filePath);
            xlWorkbook.Close();
            xlApp.Quit();
        }
        internal static void WriteData(string _filePath, Pokedex _pokedex)
        {
            var xlApp = new Application();
            xlApp.DisplayAlerts = false;
            var xlWorkbook = xlApp.Application.Workbooks.Add();
            var xlWorksheet = xlWorkbook.Worksheets[1];

            try
            {
                //var usedRange = xlWorksheet.UsedRange;
                //var lastRow = usedRange.Find("*", SearchOrder: XlSearchOrder.xlByRows, SearchDirection: XlSearchDirection.xlPrevious).Row;
                //var range = xlWorksheet.Range[xlWorksheet.Cells[3, 2], xlWorksheet.Cells[lastRow, 9]];

                xlWorksheet.Cells[1, 1] = $"{_pokedex.userName}'s Pokedex";
                xlWorksheet.Cells[2, 1] = "Name";
                xlWorksheet.Cells[2, 2] = "In Pokedex?";
                xlWorksheet.Cells[2, 3] = "# of Pokemon";
                xlWorksheet.Cells[2, 4] = "# of Candies";
                xlWorksheet.Cells[2, 5] = "Evolution Cost (Candy)";
                xlWorksheet.Cells[2, 6] = "Next Stage";
                var r = 3;
                foreach (var p in _pokedex.Inventory)
                {
                    var c = 1;
                    xlWorksheet.Cells[r, c++] = p.name;
                    xlWorksheet.Cells[r, c++] = p.inPokedex;
                    xlWorksheet.Cells[r, c++] = p.qtyPokemon;
                    xlWorksheet.Cells[r, c++] = p.qtyCandy;
                    xlWorksheet.Cells[r, c++] = p.candyToEvolve;
                    xlWorksheet.Cells[r, c++] = p.nextStage;
                    r++;
                }
                if (File.Exists(_filePath)) File.Delete(_filePath);
                xlWorkbook.SaveAs(_filePath);

            }
            catch(Exception _ExcelWriteData)
            {

            }
            finally
            {
                xlWorkbook.Close();
                xlApp.Quit();
            }
        }

        internal static Pokedex GetData(string _filePath)
        {
            var dex = new Pokedex();
            var xlApp = new Application();
            xlApp.DisplayAlerts = false;
            var xlWorkbook = xlApp.Application.Workbooks.Open(_filePath);
            var xlWorksheet = xlWorkbook.Worksheets[1];

            try
            {
                dex.userName = xlWorksheet.Cells[1, 1].Value;
                var r = 3;
                while (xlWorksheet.Cells[r, 1].Value != string.Empty && xlWorksheet.Cells[r, 1].Value != null)
                {
                    var c = 1;
                    string name = xlWorksheet.Cells[r, c++].Value;
                    bool inPokedex = xlWorksheet.Cells[r, c++].Value;
                    int qtyPokemon = int.Parse(xlWorksheet.Cells[r, c++].Value.ToString());
                    int qtyCandy = int.Parse(xlWorksheet.Cells[r, c++].Value.ToString());
                    int candyToEvolve = int.Parse(xlWorksheet.Cells[r, c++].Value.ToString());
                    string nextStage = xlWorksheet.Cells[r, c++].Value;
                    dex.Inventory.Add(new Pokemon(name,inPokedex,qtyPokemon,qtyCandy,candyToEvolve,nextStage));
                    r++;
                }
                return dex;
            }
            catch (Exception _ExcelReadData)
            {
                return null;
            }
            finally
            {
                xlWorkbook.Close();
                xlApp.Quit();
            }
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
