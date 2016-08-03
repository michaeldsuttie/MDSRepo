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
using Newtonsoft.Json;

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
            var fileName = "pokedex.json";
            var filePath = $"{workingDir}{fileName}";
        RERUN:
            var Pokedex = new Pokedex();
            //var tempDex = new Dictionary<string, Pokemon>();

            var poke1 = new Pokemon("Pidgey", true, 68, 462, 12);
            var poke2 = new Pokemon("Ratatta", true, 56, 423, 25);
            Pokedex.Inventory.Add(poke2.name, poke2);
            Pokedex.Inventory.Add(poke1.name, poke1);
            Console.WriteLine("BeforeSort:");
            foreach (var o in Pokedex.Inventory)
            {
                Console.WriteLine(o.Key);
            }
            //Pokedex = (Pokedex)Pokedex.Inventory.OrderBy(x => x.Key);
            //Pokedex.Inventory = from entry in Pokedex.Inventory orderby entry.Key ascending select entry;
            Console.WriteLine("AfterSort:");
            foreach (var o in Pokedex.Inventory)
            {
                Console.WriteLine(o.Key);
            }

            JsonOperations.WriteDataToFile(filePath, new object[] { poke1, poke2 });
            JsonOperations.ReadDataFromFile(filePath);
            ExitApp();
            goto RERUN;
        }
    }



    [DataContract]
    public class Pokemon : IComparable<Pokemon>
    {
        public Pokemon(string _name = "", bool _inPokedex = false, int _qtyPokemon = -1, int _qtyCandy = -1, int _candyToEvolve = -1)
        {
            name = _name;
            inPokedex = _inPokedex;
            qtyPokemon = _qtyPokemon;
            qtyCandy = _qtyCandy;
            candyToEvolve = _candyToEvolve;
        }

        [DataMember]
        //[JsonProperty("name")]
        internal string name;
        [DataMember]
        //[JsonProperty("inPokedex")]
        internal bool inPokedex;
        [DataMember]
        //[JsonProperty("qtyPokemon")]
        internal int qtyPokemon;
        [DataMember]
        //[JsonProperty("qtyCandy")]
        internal int qtyCandy;
        [DataMember]
        //[JsonProperty("candyToEvolve")]
        internal int candyToEvolve;

        public int CompareTo(Pokemon other)
        {
            return name.CompareTo(other.name);
        }
    }

    class Pokedex
    {
        public Pokedex()
        {
            Inventory = new Dictionary<string, Pokemon>();
        }
        internal Dictionary<string, Pokemon> Inventory { get; set; }
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
            //this.Application.Workbooks.Open(@"C:\Test\YourWorkbook.xlsx")
            var xlWorksheet = xlWorkbook.Worksheets[1];
            var usedRange = xlWorksheet.UsedRange;
            //var lastRow = usedRange.Find("*", SearchOrder: XlSearchOrder.xlByRows, SearchDirection: XlSearchDirection.xlPrevious).Row;
            //var range = xlWorksheet.Range[xlWorksheet.Cells[3, 2], xlWorksheet.Cells[lastRow, 9]];

            xlWorksheet.Cells[1, 1] = "It works!";

            xlWorkbook.Close();
            xlApp.Quit();

        }
        internal static Dictionary<string, Pokemon> Getdata()
        {
            throw new NotImplementedException();
        }
    }

    class JsonOperations
    {
        internal static void WriteDataToFile(string _filePath, object[] _objs)
        {
            var stream = new FileStream(_filePath, FileMode.Create, FileAccess.ReadWrite);
            var ser = new DataContractJsonSerializer(typeof(Pokemon));
            foreach (object o in _objs)
            {
                ser.WriteObject(stream, o);
            }
            stream.Flush();
            stream.Close();
        }

        internal static object ReadDataFromFile(string _filePath)
        {
            List<object> objs = new List<object>();
            string content = File.ReadAllText(_filePath);
            var ser = new DataContractJsonSerializer(typeof(Pokemon));
            var stream = new FileStream(_filePath, FileMode.Open, FileAccess.Read);
            //ser.
            //while(ser.ReadObject.ToString() != null)
            //{
            //    objs.Add()
            //}
            //var pokedex = ser.ReadObject()
            //var pokedex = JsonConvert.DeserializeObject<Pokemon>(content);
            return objs;
        }
    }
}
