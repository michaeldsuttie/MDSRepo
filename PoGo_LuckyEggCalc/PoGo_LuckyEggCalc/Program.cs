using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.IO;
using System.Runtime.Serialization.Json;
using NLog;
using NLog.Targets;

namespace PoGo_LuckyEggCalc
{
    class Program : AppBase
    {
        static void Main(string[] args)
        {
            Pokemon poke1 = new Pokemon("Pidgey", true, 68, 462, 12);
            Pokemon poke2 = new Pokemon("Ratatta", true, 56, 423, 25);

            MemoryStream stream1 = new MemoryStream();
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Pokemon));
            ser.WriteObject(stream1, poke1);
            ser.WriteObject(stream1, poke2);
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

        private static Logger ConfigLogger(Logger _logger, string _logName)
        {
            _logger = LogManager.GetLogger(_logName);
            var _fileTarget = (FileTarget)LogManager.Configuration.LoggingRules.First().Targets.First();
            _fileTarget.FileName = $@"{AppDomain.CurrentDomain.BaseDirectory}\logs\{DateTime.Now.ToString("yyyy-MM-dd")}\{DateTime.Now.ToString("HH-mm-ss")}.txt";
            LogManager.ReconfigExistingLoggers();
            return _logger;
        }

        private static void WriteToLog(Logger _logger, string _severity, string _message, bool _suppressConsoleOutput = true)
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

        private static void ExitApp()
        {
            WriteToLog(DebugLog, "info", "Exiting app...", false);
            Environment.Exit(0);
        }

    }
}
