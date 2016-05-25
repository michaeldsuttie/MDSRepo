using NLog;
using NLog.Targets;
using OSIsoft.AF.PI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PIConsoleAppBase
{
    class Program
    {
        private static Logger DebugLog;
        private static string PIServerName { get; set; }
        private static string StartTime { get; set; }
        private static string EndTime { get; set; }


        static void Main(string[] args)
        {
            DebugLog = ConfigLogger(DebugLog, "DebugLog");
            ParseConfigXML("config.xml", "config");
            ConnectToPI(PIServerName);
            ExitApp();
        }

        private static Logger ConfigLogger(Logger _logger, string _logName)
        {
            _logger = LogManager.GetLogger(_logName);
            var _fileTarget = (FileTarget)LogManager.Configuration.LoggingRules.First().Targets.First();
            _fileTarget.FileName = $@"{AppDomain.CurrentDomain.BaseDirectory}\logs\{DateTime.Now.ToString("yyyy-MM-dd")}\{DateTime.Now.ToString("HH-mm-ss")}.txt";
            LogManager.ReconfigExistingLoggers();
            return _logger;
        }

        private static void WriteToLog(Logger _logger, string _severity, string _message)
        {
            _severity = _severity.ToLower();
            Console.WriteLine(_message);
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

        private static void ParseConfigXML(string _name, string _rootElement)
        {
            var stackTrace = new StackTrace();
            WriteToLog(DebugLog, "debug", $"Entered Method: {stackTrace.GetFrame(0).GetMethod().Name}");

            try
            {
                var xdoc = new XDocument();
                xdoc = XDocument.Load($@"{AppDomain.CurrentDomain.BaseDirectory}\{_name}");

                var config = xdoc.Element(_rootElement).Descendants();
                foreach (XElement setting in config)
                {
                    switch (setting.Name.ToString().ToLower())
                    {
                        case "piservername":
                            PIServerName = setting.Value;
                            WriteToLog(DebugLog, "info", $"     {setting.Name}: {setting.Value}");
                            break;
                        case "starttime":
                            StartTime = setting.Value;
                            WriteToLog(DebugLog, "info", $"     {setting.Name}: {setting.Value}");
                            break;
                        case "endtime":
                            EndTime = setting.Value;
                            WriteToLog(DebugLog, "info", $"     {setting.Name}: {setting.Value}");
                            break;
                        default:
                            WriteToLog(DebugLog, "error", $"Did not recognize setting: {setting.Name.ToString().ToLower()}");
                            break;
                    }
                }
                WriteToLog(DebugLog, "info", $"     Successfully processed XML configuration file '{AppDomain.CurrentDomain.BaseDirectory}config.xml'");
            }
            catch (Exception ex_ParseConfigXML)
            {
                WriteToLog(DebugLog, "error", $"     Failed to process XML configuration file: {ex_ParseConfigXML.Message}");
                ExitApp();
            }
        }

        private static PIServer ConnectToPI(string _piServerName)
        {
            var stackTrace = new StackTrace();
            WriteToLog(DebugLog, "debug", $"Entered Method: {stackTrace.GetFrame(0).GetMethod().Name}");

            try
            {
                var _PIServer = PIServer.FindPIServer(_piServerName);
                _PIServer.Connect();
                WriteToLog(DebugLog, "info", $"     Successfully connected to PI server '{_piServerName}'");
                return _PIServer;
            }
            catch (Exception ex_ConnectToPI)
            {
                WriteToLog(DebugLog, "error", $"     Failed to connect to PI server '{_piServerName}': {ex_ConnectToPI.Message}");
                ExitApp();
                return null;
            }
        }

        private static void ExitApp()
        {
            WriteToLog(DebugLog, "info", "Press any key to exit...");
            Console.ReadKey();
            WriteToLog(DebugLog, "info", "Exiting app...");
            Environment.Exit(0);
        }
    }
}
