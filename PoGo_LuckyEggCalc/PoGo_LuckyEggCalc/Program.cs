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
        static void Main(string[] args)
        {
            DebugLog = ConfigLogger(DebugLog, "DebugLog");
            WriteToLog(DebugLog, "info", $"{Assembly.GetExecutingAssembly().FullName}", false);
        RERUN:
            var poke1 = new Pokemon("Pidgey", true, 68, 462, 12);
            var poke2 = new Pokemon("Ratatta", true, 56, 423, 25);
            var workingDir = $@"C:\Users\msuttie\Desktop\testdir\";
            var fileName = "testfile.txt";
            var filePath = $"{workingDir}{fileName}";
            //Directory.CreateDirectory(workingDir, new DirectorySecurity(workingDir, AccessControlSections.Owner));
            JsonOperations.WriteDataToFile(filePath, new object[] { poke1, poke2 });
            ExitApp();
            goto RERUN;
        }
    }



    [DataContract]
    class Pokemon
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
        string name;
        [DataMember]
        bool inPokedex;
        [DataMember]
        int qtyPokemon;
        [DataMember]
        int qtyCandy;
        [DataMember]
        int candyToEvolve;
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
            return null;
        }
    }

    class JsonOperations
    {
        internal static void WriteDataToFile(string _filePath, object[] _objs)
        {
            var stream = new FileStream(_filePath, FileMode.Create, FileAccess.ReadWrite);
            var ser = new DataContractJsonSerializer(typeof(Pokemon));
            foreach(object o in _objs)
            {
                ser.WriteObject(stream, o);
            }
            stream.Flush();
            stream.Close();
        }
    }
}
