﻿using NLog;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelConsoleAppBase
{
    class Program
    {
        private static Logger DebugLog;
        //private static List<string> Headers { get; set; }
        public static Tag PreviousTag { get; set; }
        public static List<Tag> ErrorTags { get; set; }

        static void Main(string[] args)
        {
            DebugLog = ConfigLogger(DebugLog, "DebugLog");
            var fileName01 = "Foley_Rnch";
            var fileName02 = "Livermore_Jct";
            var fileName03 = "LRCV_419B";
            //string filename = args[0].ToString();
            ErrorTags = new List<Tag>();

            var csvContent = parseCSV($"{AppDomain.CurrentDomain.BaseDirectory}{fileName01}.csv");

            foreach (var record in csvContent)
            {
                //WriteToLog(DebugLog, "info", $"name: {record.Value.name} | register_EURange: {record.Value.register_EURange} | DataType: {record.Value.DataType} | LowClear_RegisterValue: {record.Value.LowClear_RegisterValue} | LowClear_ExpectedResult: {record.Value.LowClear_ExpectedResult} | Underrange_RegisterValue: {record.Value.Underrange_RegisterValue} | Underrange_ExpectedResult: {record.Value.Underrange_ExpectedResult} | Overrange_RegisterValue: {record.Value.Overrange_RegisterValue} | Overrange_ExpectedResult: {record.Value.Overrange_ExpectedResult} | LL_RegisterValue: {record.Value.LL_RegisterValue} | LL_ExpectedResult: {record.Value.LL_ExpectedResult} | L_RegisterValue: {record.Value.L_RegisterValue} | L_ExpectedResult: {record.Value.L_ExpectedResult} | H_RegisterValue: {record.Value.H_RegisterValue} | H_ExpectedResult: {record.Value.H_ExpectedResult} | HH_RegisterValue: {record.Value.HH_RegisterValue} | HH_ExpectedResult: {record.Value.HH_ExpectedResult} | MOP_RegisterValue: {record.Value.MOP_RegisterValue} | MOP_ExpectedResult: {record.Value.MOP_ExpectedResult} | HighClear_RegisterValue: {record.Value.HighClear_RegisterValue} | HighClear_ExpectedResult: {record.Value.HighClear_ExpectedResult}");
                var parametersValid = ParseParameters(record.Value);
                if (ErrorTags.Contains(record.Value)) continue;
                var progressionValid = CheckValueProgression(record.Value);
            }
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

        private static void ExitApp()
        {
            WriteToLog(DebugLog, "info", "Press any key to exit...");
            Console.ReadKey();
            WriteToLog(DebugLog, "info", "Exiting app...");
            Environment.Exit(0);
        }

        public static Dictionary<string, Tag> parseCSV(string _path)
        {
            try
            {
                using (StreamReader readFile = new StreamReader(_path))
                {
                    string line;
                    int rowIndex = 0;
                    //Headers = new List<string>();
                    PreviousTag = new Tag();
                    var CSVContent = new Dictionary<string, Tag>();
                    var templist = new List<string>();

                    while ((line = readFile.ReadLine()) != null)
                    {
                        string[] row = line.Split(',');
                        if (row[1].Contains("_Cmd") || (!row[1].Contains("_") && !PreviousTag.name.Contains("_")))
                        {
                            rowIndex++;
                            continue;
                        }
                        //if (row[0] == "Process Value (PV) Description")
                        //{
                        //    var headerString = "Headers: ";
                        //    foreach (var h in row)
                        //    {
                        //        Headers.Add(h);
                        //        headerString += $"{h},";
                        //    }
                        //    headerString = headerString.Remove(headerString.Length - 1, 1);
                        //    WriteToLog(DebugLog, "info", headerString);
                        //}
                        if (CSVContent.ContainsKey((row[1] == string.Empty ? PreviousTag.name : row[1])))
                        {
                            var record = CSVContent.Where(x => x.Key == PreviousTag.name).FirstOrDefault();
                            for (int i = 0; i < row.Count(); i++)
                            {
                                switch (i)
                                {
                                    case 6:
                                        switch (row[8])
                                        {
                                            case "Underrange-6%":
                                                record.Value.Underrange_RegisterValue = row[i];
                                                //WriteToLog(DebugLog, "info", $"     Underrange_RegisterValue: {row[i]}");
                                                break;
                                            case "LL":
                                                record.Value.LL_RegisterValue = row[i];
                                                //WriteToLog(DebugLog, "info", $"     LL_RegisterValue: {row[i]}");
                                                break;
                                            case "L":
                                                record.Value.L_RegisterValue = row[i];
                                                //WriteToLog(DebugLog, "info", $"     L_RegisterValue: {row[i]}");
                                                break;
                                            case "H":
                                                record.Value.H_RegisterValue = row[i];
                                                //WriteToLog(DebugLog, "info", $"     H_RegisterValue: {row[i]}");
                                                break;
                                            case "HH":
                                                record.Value.HH_RegisterValue = row[i];
                                                //WriteToLog(DebugLog, "info", $"     HH_RegisterValue: {row[i]}");
                                                break;
                                            case "MOP":
                                                record.Value.MOP_RegisterValue = row[i];
                                                //WriteToLog(DebugLog, "info", $"     MOP_RegisterValue: {row[i]}");
                                                break;
                                            case "Clear":
                                                record.Value.HighClear_RegisterValue = row[i];
                                                //WriteToLog(DebugLog, "info", $"     HighClear_RegisterValue: {row[i]}");
                                                break;
                                            case "Overrange-6%":
                                                record.Value.Overrange_RegisterValue = row[i];
                                                break;
                                        }
                                        break;
                                    case 7:
                                        switch (row[8])
                                        {
                                            case "Underrange-6%":
                                                record.Value.Underrange_ExpectedResult = row[i];
                                                //WriteToLog(DebugLog, "info", $"     Underrange_ExpectedResult: {row[i]}");
                                                break;
                                            case "LL":
                                                record.Value.LL_ExpectedResult = row[i];
                                                //WriteToLog(DebugLog, "info", $"     LL_ExpectedResult: {row[i]}");
                                                break;
                                            case "L":
                                                record.Value.L_ExpectedResult = row[i];
                                                //WriteToLog(DebugLog, "info", $"     L_ExpectedResult: {row[i]}");
                                                break;
                                            case "H":
                                                record.Value.H_ExpectedResult = row[i];
                                                //WriteToLog(DebugLog, "info", $"     H_ExpectedResult: {row[i]}");
                                                break;
                                            case "HH":
                                                record.Value.HH_ExpectedResult = row[i];
                                                //WriteToLog(DebugLog, "info", $"     HH_ExpectedResult: {row[i]}");
                                                break;
                                            case "MOP":
                                                record.Value.MOP_ExpectedResult = row[i];
                                                //WriteToLog(DebugLog, "info", $"     MOP_ExpectedResult: {row[i]}");
                                                break;
                                            case "Clear":
                                                record.Value.HighClear_ExpectedResult = row[i];
                                                //WriteToLog(DebugLog, "info", $"     HighClear_ExpectedResult: {row[i]}");
                                                break;
                                            case "Overrange-6%":
                                                record.Value.Overrange_ExpectedResult = row[i];
                                                break;
                                        }
                                        break;
                                }
                            }
                        }
                        else if (!CSVContent.ContainsKey(row[1]) && rowIndex >= 2)
                        {
                            if (PreviousTag == null)
                            {
                                PreviousTag = new Tag();
                            }
                            var tempRecord = new Tag();
                            for (int i = 0; i < row.Count(); i++)
                            {
                                switch (i)
                                {
                                    case 1:
                                        tempRecord.name = row[i];
                                        PreviousTag.name = row[i];
                                        //WriteToLog(DebugLog, "info", $"{Environment.NewLine}     name: {row[i]}");
                                        break;
                                    case 2:
                                        tempRecord.register_EURange = row[i];
                                        //PreviousTag.register_EURange = row[i];
                                        //WriteToLog(DebugLog, "info", $"     register_EURange: {row[i]}");
                                        break;
                                    case 4:
                                        tempRecord.DataType = row[i];
                                        //PreviousTag.DataType = row[i];
                                        //WriteToLog(DebugLog, "info", $"     DataType: {row[i]}");
                                        break;
                                    case 6:
                                        tempRecord.LowClear_RegisterValue = row[i];
                                        //PreviousTag.LowClear_RegisterValue = row[i];
                                        //WriteToLog(DebugLog, "info", $"     LowClear_RegisterValue: {row[i]}");
                                        break;
                                    case 7:
                                        tempRecord.LowClear_ExpectedResult = row[i];
                                        //PreviousTag.LowClear_ExpectedResult = row[i];
                                        //WriteToLog(DebugLog, "info", $"     LowClear_ExpectedResult: {row[i]}");
                                        break;
                                }
                            }
                            CSVContent.Add(row[1], tempRecord);
                        }
                        else
                        {
                            WriteToLog(DebugLog, "info", $"Row[{rowIndex}] not processed.");
                        }

                        rowIndex++;
                    }
                    WriteToLog(DebugLog, "info", $"Sucessfully parsed CSV @ '{_path}'. {CSVContent.Count} Tags added.");
                    return CSVContent;
                }
            }
            catch (Exception EX_ParseCSV)
            {
                WriteToLog(DebugLog, "error", $"Could not parse CSV: {EX_ParseCSV}");
                return null;
            }
        }

        public static bool CheckValueProgression(Tag _Tag)
        {
            try
            {
                var result = false;
                if (_Tag.DataType == "REAL")
                {
                    if (!_Tag.DoubleParameters.ContainsKey("LL_RegisterValue"))
                    {
                        WriteToLog(DebugLog, "info", $"No parameters to verify are in increasing order for tag '{_Tag.name}'");
                        return true;
                    }
                    var check1 = _Tag.DoubleParameters["LL_RegisterValue"] < _Tag.DoubleParameters["L_RegisterValue"];
                    var check2 = _Tag.DoubleParameters["L_RegisterValue"] < _Tag.DoubleParameters["H_RegisterValue"];
                    var check3 = _Tag.DoubleParameters["H_RegisterValue"] < _Tag.DoubleParameters["HH_RegisterValue"];
                    var check4 = true;
                    if (_Tag.DoubleParameters.ContainsKey("MOP_RegisterValue"))
                    {
                        check4 = _Tag.DoubleParameters["HH_RegisterValue"] < _Tag.DoubleParameters["MOP_RegisterValue"];
                    }
                    result = check1 && check2 && check3 && check4;
                }
                else if (_Tag.DataType == "INT")
                {
                    if (!_Tag.IntParameters.ContainsKey("LL_RegisterValue"))
                    {
                        WriteToLog(DebugLog, "info", $"No parameters to verify are in increasing order for tag '{_Tag.name}'");
                        return true;
                    }
                    var check1 = _Tag.IntParameters["LL_RegisterValue"] < _Tag.IntParameters["L_RegisterValue"];
                    var check2 = _Tag.IntParameters["L_RegisterValue"] < _Tag.IntParameters["H_RegisterValue"];
                    var check3 = _Tag.IntParameters["H_RegisterValue"] < _Tag.IntParameters["HH_RegisterValue"];
                    var check4 = true;
                    if (_Tag.IntParameters.ContainsKey("MOP_RegisterValue"))
                    {
                        check4 = _Tag.IntParameters["HH_RegisterValue"] < _Tag.IntParameters["MOP_RegisterValue"];
                    }
                    result = check1 && check2 && check3 && check4;
                }
                if (result)
                {
                    WriteToLog(DebugLog, "info", $"Successfully verified parameters are in increasing order for tag '{_Tag.name}'");
                    return true;
                }
                else
                {
                    WriteToLog(DebugLog, "info", $"Parameters are not in order for tag '{_Tag.name}'.");
                    ErrorTags.Add(_Tag);
                    return false;
                }
            }
            catch (Exception EX_ParseParameters)
            {
                WriteToLog(DebugLog, "error", $"Could not verify parameter order for tag '{_Tag.name}': {EX_ParseParameters}");
                ErrorTags.Add(_Tag);
                return false;
            }
        }

        public static bool ParseParameters(Tag _Tag)
        {
            try
            {
                if (_Tag.DataType == "REAL")
                {
                    if (_Tag.LL_RegisterValue != string.Empty)
                    {
                        _Tag.DoubleParameters.Add("LL_RegisterValue", double.Parse(_Tag.LL_RegisterValue));
                    }
                    if (_Tag.L_RegisterValue != string.Empty)
                    {
                        _Tag.DoubleParameters.Add("L_RegisterValue", double.Parse(_Tag.L_RegisterValue));
                    }
                    if (_Tag.H_RegisterValue != string.Empty)
                    {
                        _Tag.DoubleParameters.Add("H_RegisterValue", double.Parse(_Tag.H_RegisterValue));
                    }
                    if (_Tag.HH_RegisterValue != string.Empty)
                    {
                        _Tag.DoubleParameters.Add("HH_RegisterValue", double.Parse(_Tag.HH_RegisterValue));
                    }
                    if (_Tag.MOP_RegisterValue != string.Empty)
                    {
                        _Tag.DoubleParameters.Add("MOP_RegisterValue", double.Parse(_Tag.MOP_RegisterValue));
                    }
                    WriteToLog(DebugLog, "info", $"Successfully parsed parameters for tag '{_Tag.name}'");

                    return true;
                }
                else if (_Tag.DataType == "INT")
                {
                    if (_Tag.LL_RegisterValue != string.Empty)
                    {
                        _Tag.IntParameters.Add("LL_RegisterValue", int.Parse(_Tag.LL_RegisterValue));
                    }
                    if (_Tag.L_RegisterValue != string.Empty)
                    {
                        _Tag.IntParameters.Add("L_RegisterValue", int.Parse(_Tag.L_RegisterValue));
                    }
                    if (_Tag.H_RegisterValue != string.Empty)
                    {
                        _Tag.IntParameters.Add("H_RegisterValue", int.Parse(_Tag.H_RegisterValue));
                    }
                    if (_Tag.HH_RegisterValue != string.Empty)
                    {
                        _Tag.IntParameters.Add("HH_RegisterValue", int.Parse(_Tag.HH_RegisterValue));
                    }
                    if (_Tag.MOP_RegisterValue != string.Empty)
                    {
                        _Tag.IntParameters.Add("MOP_RegisterValue", int.Parse(_Tag.MOP_RegisterValue));
                    }
                    WriteToLog(DebugLog, "info", $"Successfully parsed parameters for tag '{_Tag.name}'");

                    return true;
                }
                return false;
            }
            catch (Exception EX_ParseParameters)
            {
                WriteToLog(DebugLog, "error", $"Could not parse parameters for tag '{_Tag.name}': {EX_ParseParameters}");
                return false;
            }
        }
    }
    public class Tag
    {
        public string name = string.Empty;
        public string register_EURange = string.Empty;
        public string DataType = string.Empty;

        public string LowClear_RegisterValue = string.Empty;
        public string LowClear_ExpectedResult = string.Empty;
        public string Underrange_RegisterValue = string.Empty;
        public string Underrange_ExpectedResult = string.Empty;
        public string LL_RegisterValue = string.Empty;
        public string LL_ExpectedResult = string.Empty;
        public string L_RegisterValue = string.Empty;
        public string L_ExpectedResult = string.Empty;
        public string H_RegisterValue = string.Empty;
        public string H_ExpectedResult = string.Empty;
        public string HH_RegisterValue = string.Empty;
        public string HH_ExpectedResult = string.Empty;
        public string MOP_RegisterValue = string.Empty;
        public string MOP_ExpectedResult = string.Empty;
        public string Overrange_RegisterValue = string.Empty;
        public string Overrange_ExpectedResult = string.Empty;
        public string HighClear_RegisterValue = string.Empty;
        public string HighClear_ExpectedResult = string.Empty;

        public Dictionary<string, int> IntParameters = new Dictionary<string, int>();
        public Dictionary<string, double> DoubleParameters = new Dictionary<string, double>();
    }
}