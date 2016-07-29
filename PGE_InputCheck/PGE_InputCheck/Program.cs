using NLog;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Microsoft.Office.Interop.Excel;

[assembly: AssemblyVersion("2.3.*")]

namespace PGE_InputCheck
{
    class Program
    {
        private static Logger DebugLog { get; set; }
        public static Tag PreviousTag { get; set; }
        public static List<Tag> ErrorTags { get; set; }

        static void Main(string[] args)
        {
            DebugLog = ConfigLogger(DebugLog, "DebugLog");
            WriteToLog(DebugLog, "info", $"{Assembly.GetExecutingAssembly().FullName}", false);
        RERUN:
            ErrorTags = new List<Tag>();
#if DEBUG
            //var fileName = "Foley_Rnch";
            //var fileName = "Livermore_Jct";
            //var fileName = "LRCV_419B";
            //var fileName = "Martinez_Sta";
            //var fileName = "Old_Redwood";
            //var fileName = "Palm_Tract";
            //var fileName = "Tracy_Sta";
            //var fileName = "Vernalis_Meter";
            //var fileName = "Brentwood_PLC_2";
            //var fileName = "Ruby_Intertie";
            //var fileName = "Lodi_Field";
            //var fileName = "Wild_Goose_Grdly";
            //var fileName = "Panoche.csv";
            //var fileName = "Bethany_K.csv";
            //var fileName = "Bethany_K.xlsx";
            //var fileName = "Burney_K - Copy.xlsx";
            var fileName = "PGT_Malin.xlsx";

            string filePath = $"{AppDomain.CurrentDomain.BaseDirectory}{fileName}";
#else
            if (args.Length == 0)
            {
                WriteToLog(DebugLog, "error", $"Please supply a file name with the file extension.", false);
                ExitApp();
            }

            string filePath = $"{AppDomain.CurrentDomain.BaseDirectory}{args[0].ToString()}";
            if (!File.Exists(filePath))
            {
                WriteToLog(DebugLog, "error", $@"'{args[0].ToString()}' not found in '{AppDomain.CurrentDomain.BaseDirectory}'. Check spelling and file location.", false);
                ExitApp();
            }
#endif
            WriteToLog(DebugLog, "info", "Parsing file...", false);
            var xlsxContent = parseXLSX(filePath);

            if (xlsxContent == null)
            {
                WriteToLog(DebugLog, "error", "Could not parse file. Please check file contents and try again.", false);
                ExitApp();
            }

            WriteToLog(DebugLog, "info", "Checking parameters...", false);
            foreach (var record in xlsxContent)
            {
                if (ErrorTags.Contains(record.Value)) continue;
                var progressionValid = CheckValueProgression(record.Value);
                var clearValid = CheckClearValue(record.Value);
                var underrangeValid = CheckUnderrange(record.Value);
                var overrangeValid = CheckOverrange(record.Value);
                var linearityValid = CheckLinearity(record.Value);
                if (record.Value.DataType == "INT")
                {
                    var IntValueValid = CheckIntValue(record.Value);
                }
            }
            if (ErrorTags.Count != 0) DisplayErrorTags(ErrorTags);
            else
            {
                Console.Write(Environment.NewLine);
                WriteToLog(DebugLog, "info", "No errors found!", false);
            }
            while (true)
            {
                Console.Write(Environment.NewLine);
                WriteToLog(DebugLog, "info", "Do you want to run again? [Y/N]", false);
                var keyPress = Console.ReadLine().ToUpper();
                if (keyPress == "Y")
                {
                    Console.Write(Environment.NewLine);
                    WriteToLog(DebugLog, "info", "Re-run initiated...", false);
                    goto RERUN;
                }
                else if (keyPress == "N")
                {
                    Console.Write(Environment.NewLine);
                    ExitApp();
                }
            }
        }

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

        public static Dictionary<string, Tag> parseXLSX(string _path)
        {
            var excel = new Application();
            var workBook = excel.Workbooks.Open(_path);
            try
            {
                _Worksheet tab = workBook.Sheets[1];
                var usedRange = tab.UsedRange;
                var lastRow = usedRange.Find("*", SearchOrder: XlSearchOrder.xlByRows, SearchDirection: XlSearchDirection.xlPrevious).Row;
                var range = tab.Range[tab.Cells[3, 2], tab.Cells[lastRow, 9]];
                var previousTag = new Tag();
                var XLSXContent = new Dictionary<string, Tag>();
                var regexNumberPattern = @"-?[0-9]+[.][0-9]+|-?[0-9]+";
                var regexAlarmStatePattern = @"Overrange|Underrange|Clear|LL|L|HH|H|MOP";
                var regexDataTypePattern = @"INT|REAL";

                foreach (var row in Enumerable.Range(1, lastRow))
                {
                    var tempTag = new Tag();

                    tempTag.name = range.Cells[row, 1].Text;
                    tempTag.register_EURange = range.Cells[row, 2].Text;
                    tempTag.DataType = Regex.Match(range.Cells[row, 4].Text, regexDataTypePattern).Value;
                    string registerValue = Regex.Match(range.Cells[row, 6].Text, regexNumberPattern).Value;
                    string expectedResult = Regex.Match(range.Cells[row, 7].Text, regexNumberPattern).Value;
                    string AlarmState = Regex.Match(range.Cells[row, 8].Text, regexAlarmStatePattern).Value;

                    var rvEmpty = registerValue != string.Empty;
                    var erEmpty = expectedResult != string.Empty;
                    var rvString = string.Empty;
                    var erString = string.Empty;

                    if (tempTag.name.Contains("Cmd") || AlarmState == string.Empty) continue;

                    if (tempTag.name == string.Empty) tempTag.name = previousTag.name;
                    if (ErrorTags.Find(x => x.name == tempTag.name) != null) continue;

                    if (XLSXContent.ContainsKey(tempTag.name))
                    {
                        tempTag = previousTag;
                        if (registerValue == string.Empty || expectedResult == string.Empty || AlarmState == string.Empty)
                        {
                            WriteToLog(DebugLog, "error", $"Parameter cell empty @ row {row + 2}.");
                            tempTag.Errors.Add("Parameter Empty.");
                            ErrorTags.Add(tempTag);
                            previousTag = tempTag;
                            continue;
                        }
                    }
                    else
                    {
                        if (tempTag.register_EURange == string.Empty || tempTag.DataType == string.Empty || registerValue == string.Empty || expectedResult == string.Empty || AlarmState == string.Empty)
                        {
                            WriteToLog(DebugLog, "error", $"Parameter cell empty @ row {row + 2}.");
                            tempTag.Errors.Add("Parameter Empty.");
                            ErrorTags.Add(tempTag);
                            previousTag = tempTag;
                            continue;
                        }

                        var rawRangeMatches = Regex.Matches(tempTag.register_EURange, regexNumberPattern);
                        var rawRangeMatchesList = new List<string>();
                        var parsedRangeMatches = new List<double>();
                        foreach (var m in rawRangeMatches)
                        {
                            rawRangeMatchesList.Add(m.ToString());
                        }
                        for (int c = 0; c < rawRangeMatchesList.Count; c++)
                        {
                            if (rawRangeMatchesList[c].ToString().EndsWith("."))
                            {
                                var nextMatch = rawRangeMatchesList[c + 1];
                                var val = rawRangeMatchesList[c].ToString() + nextMatch.ToString();
                                if (val.StartsWith("-") && !(c % 2 == 0)) val = val.Replace("-", "");
                                parsedRangeMatches.Add(double.Parse(val.ToString()));
                                rawRangeMatchesList.RemoveAt(c + 1);
                            }
                            else
                            {
                                if (rawRangeMatchesList[c].StartsWith("-") && !(c % 2 == 0)) rawRangeMatchesList[c] = rawRangeMatchesList[c].Replace("-", "");
                                parsedRangeMatches.Add(double.Parse(rawRangeMatchesList[c].ToString()));
                            }
                        }
                        tempTag.Low_RegisterRange = parsedRangeMatches[0];
                        tempTag.High_RegisterRange = parsedRangeMatches[1];
                        tempTag.Low_EURange = parsedRangeMatches[2];
                        tempTag.High_EURange = parsedRangeMatches[3];
                    }

                    switch (AlarmState)
                    {
                        case "Underrange":
                            rvString = "Underrange_RegisterValue";
                            erString = "Underrange_ExpectedResult";
                            break;
                        case "LL":
                            rvString = "LL_RegisterValue";
                            erString = "LL_ExpectedResult";
                            break;
                        case "L":
                            rvString = "L_RegisterValue";
                            erString = "L_ExpectedResult";
                            break;
                        case "H":
                            rvString = "H_RegisterValue";
                            erString = "H_ExpectedResult";
                            break;
                        case "HH":
                            rvString = "HH_RegisterValue";
                            erString = "HH_ExpectedResult";
                            break;
                        case "MOP":
                            rvString = "MOP_RegisterValue";
                            erString = "MOP_ExpectedResult";
                            break;
                        case "Clear":
                            if (!tempTag.IntParameters.ContainsKey("LowClear_RegisterValue"))
                            {
                                rvString = "LowClear_RegisterValue";
                                erString = "LowClear_ExpectedResult";
                            }
                            else
                            {
                                rvString = "HighClear_RegisterValue";
                                erString = "HighClear_ExpectedResult";
                            }
                            break;
                        case "Overrange":
                            rvString = "Overrange_RegisterValue";
                            erString = "Overrange_ExpectedResult";
                            break;
                    }

                    if (tempTag.DataType == "INT")
                    {
                        tempTag.IntParameters.Add(rvString, int.Parse(registerValue));
                        tempTag.DoubleParameters.Add(erString, double.Parse(expectedResult));
                    }
                    else if (tempTag.DataType == "REAL")
                    {
                        tempTag.DoubleParameters.Add(rvString, double.Parse(registerValue));
                        tempTag.DoubleParameters.Add(erString, double.Parse(expectedResult));
                    }

                    if (!XLSXContent.ContainsKey(tempTag.name))
                    {
                        XLSXContent.Add(tempTag.name, tempTag);
                    }
                    previousTag = tempTag;
                }
                return XLSXContent;
            }
            catch (Exception EX_ParseXLSX)
            {
                WriteToLog(DebugLog, "error", $"EX_ParseXLSX: {EX_ParseXLSX}");
                return null;
            }
            finally
            {
                workBook.Close();
                excel.Quit();
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
                        WriteToLog(DebugLog, "info", $"{_Tag.name}: No LL parameter. Cannot verify increasing 'Register Value' order.");
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
                        WriteToLog(DebugLog, "info", $"{_Tag.name}: No LL parameter. Cannot verify increasing 'Register Value' order.");
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
                    WriteToLog(DebugLog, "info", $"{_Tag.name}: Successfully verified 'Register Value' parameters are in increasing order.");
                    return true;
                }
                else
                {
                    WriteToLog(DebugLog, "info", $"{_Tag.name}: 'Register Value' parameters are not in order.");
                    _Tag.Errors.Add("Parameter order.");
                    if (!ErrorTags.Contains(_Tag)) ErrorTags.Add(_Tag);
                    return false;
                }
            }
            catch (Exception EX_CheckParameterOrder)
            {
                WriteToLog(DebugLog, "error", $"{_Tag.name}: EX_CheckParameterOrder: {EX_CheckParameterOrder}");
                _Tag.Errors.Add("EX_CheckParameterOrder.");
                if (!ErrorTags.Contains(_Tag)) ErrorTags.Add(_Tag);
                return false;
            }
        }

        public static bool CheckIntValue(Tag _Tag)
        {
            try
            {
                var result = false;
                var num = 32767;

                var check1 = true;
                if (_Tag.IntParameters.ContainsKey("LowClear_RegisterValue")) check1 = _Tag.IntParameters["LowClear_RegisterValue"] <= num;
                var check2 = true;
                if (_Tag.IntParameters.ContainsKey("Underrange_RegisterValue")) check2 = _Tag.IntParameters["Underrange_RegisterValue"] <= num;
                var check3 = true;
                if (_Tag.IntParameters.ContainsKey("Overrange_RegisterValue")) check3 = _Tag.IntParameters["Overrange_RegisterValue"] <= num;
                var check4 = true;
                if (_Tag.IntParameters.ContainsKey("HighClear_RegisterValue")) check4 = _Tag.IntParameters["HighClear_RegisterValue"] <= num;
                var check5 = true;
                if (_Tag.IntParameters.ContainsKey("`LL_RegisterValue")) check5 = _Tag.IntParameters["LL_RegisterValue"] <= num;
                var check6 = true;
                if (_Tag.IntParameters.ContainsKey("L_RegisterValue")) check6 = _Tag.IntParameters["L_RegisterValue"] <= num;
                var check7 = true;
                if (_Tag.IntParameters.ContainsKey("H_RegisterValue")) check7 = _Tag.IntParameters["H_RegisterValue"] <= num;
                var check8 = true;
                if (_Tag.IntParameters.ContainsKey("HH_RegisterValue")) check8 = _Tag.IntParameters["HH_RegisterValue"] <= num;
                var check9 = true;
                if (_Tag.IntParameters.ContainsKey("MOP_RegisterValue")) check9 = _Tag.IntParameters["MOP_RegisterValue"] <= num;

                result = check1 && check2 && check3 && check4 && check5 && check6 && check7 && check8 && check9;
                if (result)
                {
                    WriteToLog(DebugLog, "info", $"{_Tag.name}: Successfully verified Int values are less than {num}.");
                    return true;
                }
                else
                {
                    WriteToLog(DebugLog, "info", $"{_Tag.name}: Int Value >= {num}.");
                    _Tag.Errors.Add($"Int Value.");
                    if (!ErrorTags.Contains(_Tag)) ErrorTags.Add(_Tag);
                    return false;
                }
            }
            catch (Exception EX_CheckIntValue)
            {
                WriteToLog(DebugLog, "error", $"{_Tag.name}: EX_CheckIntValue: {EX_CheckIntValue}");
                _Tag.Errors.Add("EX_CheckIntValue.");
                if (!ErrorTags.Contains(_Tag)) ErrorTags.Add(_Tag);
                return false;
            }
        }

        public static bool CheckClearValue(Tag _Tag)
        {
            try
            {
                var result = false;
                if (_Tag.DataType == "REAL")
                {
                    if (_Tag.DoubleParameters["LowClear_RegisterValue"] != _Tag.DoubleParameters["HighClear_RegisterValue"])
                    {
                        WriteToLog(DebugLog, "info", $"{_Tag.name}: Clear values are not equal.");
                        _Tag.Errors.Add("Clear.");
                        if (!ErrorTags.Contains(_Tag)) ErrorTags.Add(_Tag);
                        return false;
                    }
                    if (!_Tag.DoubleParameters.ContainsKey("L_RegisterValue"))
                    {
                        WriteToLog(DebugLog, "info", $"{_Tag.name}: No L parameter to verify Clear against.");
                        return true;
                    }
                    var check1 = _Tag.DoubleParameters["LowClear_RegisterValue"] > _Tag.DoubleParameters["L_RegisterValue"];
                    var check2 = _Tag.DoubleParameters["LowClear_RegisterValue"] < _Tag.DoubleParameters["H_RegisterValue"];

                    result = check1 && check2;
                }
                else if (_Tag.DataType == "INT")
                {
                    if (_Tag.IntParameters["LowClear_RegisterValue"] != _Tag.IntParameters["HighClear_RegisterValue"])
                    {
                        WriteToLog(DebugLog, "info", $"{_Tag.name}: Clear values are not equal.");
                        _Tag.Errors.Add("Clear.");
                        if (!ErrorTags.Contains(_Tag)) ErrorTags.Add(_Tag);
                        return false;
                    }
                    if (!_Tag.DoubleParameters.ContainsKey("L_RegisterValue"))
                    {
                        WriteToLog(DebugLog, "info", $"{_Tag.name}: No L parameter to verify Clear against.");
                        return true;
                    }
                    var check1 = _Tag.DoubleParameters["LowClear_RegisterValue"] > _Tag.DoubleParameters["L_RegisterValue"];
                    var check2 = _Tag.DoubleParameters["LowClear_RegisterValue"] < _Tag.DoubleParameters["H_RegisterValue"];

                    result = check1 && check2;
                }
                if (result)
                {
                    WriteToLog(DebugLog, "info", $"{_Tag.name}: Successfully verified Clear is between L and H.");
                    return true;
                }
                else
                {
                    WriteToLog(DebugLog, "info", $"{_Tag.name}: Clear Register Value is not between L and H.");
                    _Tag.Errors.Add("Clear.");
                    if (!ErrorTags.Contains(_Tag)) ErrorTags.Add(_Tag);
                    return false;
                }
            }
            catch (Exception EX_CheckClearValue)
            {
                WriteToLog(DebugLog, "error", $"{_Tag.name}: EX_CheckClearValue: {EX_CheckClearValue}");
                _Tag.Errors.Add("EX_CheckClearValue.");
                if (!ErrorTags.Contains(_Tag)) ErrorTags.Add(_Tag);
                return false;
            }
        }

        public static bool CheckUnderrange(Tag _Tag)
        {
            try
            {
                var result = false;
                if (_Tag.DataType == "REAL")
                {
                    if (!_Tag.DoubleParameters.ContainsKey("LL_RegisterValue"))
                    {
                        WriteToLog(DebugLog, "info", $"{_Tag.name}: No LL parameter to verify Underrange against.");
                        return true;
                    }
                    result = _Tag.DoubleParameters["Underrange_RegisterValue"] < _Tag.DoubleParameters["LL_RegisterValue"];
                }
                else if (_Tag.DataType == "INT")
                {
                    if (!_Tag.IntParameters.ContainsKey("LL_RegisterValue"))
                    {
                        WriteToLog(DebugLog, "info", $"{_Tag.name}: No LL parameter to verify Underrange against.");
                        return true;
                    }
                    result = _Tag.IntParameters["Underrange_RegisterValue"] < _Tag.IntParameters["LL_RegisterValue"];
                }
                if (result)
                {
                    WriteToLog(DebugLog, "info", $"{_Tag.name}: Successfully verified Underrange is below LL.");
                    return true;
                }
                else
                {
                    WriteToLog(DebugLog, "info", $"{_Tag.name}: Underrange Register Value is not below LL.");
                    _Tag.Errors.Add("Underrange.");
                    if (!ErrorTags.Contains(_Tag)) ErrorTags.Add(_Tag);
                    return false;
                }
            }
            catch (Exception EX_CheckUnderrange)
            {
                WriteToLog(DebugLog, "error", $"{_Tag.name}: EX_CheckUnderrange: {EX_CheckUnderrange}");
                _Tag.Errors.Add("EX_CheckUnderrange.");
                if (!ErrorTags.Contains(_Tag)) ErrorTags.Add(_Tag);
                return false;
            }
        }

        public static bool CheckOverrange(Tag _Tag)
        {
            try
            {
                var result = false;
                if (_Tag.DataType == "REAL")
                {
                    if (!_Tag.DoubleParameters.ContainsKey("Overrange_RegisterValue"))
                    {
                        WriteToLog(DebugLog, "info", $"{_Tag.name}: Overrange is missing.");
                        _Tag.Errors.Add("Overrange.");
                        if (!ErrorTags.Contains(_Tag)) ErrorTags.Add(_Tag);
                        return false;
                    }
                    if (!_Tag.DoubleParameters.ContainsKey("MOP_RegisterValue"))
                    {
                        WriteToLog(DebugLog, "info", $"{_Tag.name}: No MOP parameter to verify Overrange against.");
                        return true;
                    }
                    result = _Tag.DoubleParameters["Overrange_RegisterValue"] > _Tag.DoubleParameters["MOP_RegisterValue"];
                }
                else if (_Tag.DataType == "INT")
                {
                    if (!_Tag.IntParameters.ContainsKey("Overrange_RegisterValue"))
                    {
                        WriteToLog(DebugLog, "info", $"{_Tag.name}: Overrange is missing.");
                        _Tag.Errors.Add("Overrange.");
                        if (!ErrorTags.Contains(_Tag)) ErrorTags.Add(_Tag);
                        return false;
                    }

                    if (!_Tag.IntParameters.ContainsKey("MOP_RegisterValue"))
                    {
                        WriteToLog(DebugLog, "info", $"{_Tag.name}: No MOP parameter to verify Overrange against.");
                        return true;
                    }
                    result = (_Tag.IntParameters["Overrange_RegisterValue"] > _Tag.IntParameters["MOP_RegisterValue"]) && (_Tag.IntParameters["Overrange_RegisterValue"] < 32767);
                }
                if (result)
                {
                    WriteToLog(DebugLog, "info", $"{_Tag.name}: Successfully verified Overrange is above MOP.");
                    return true;
                }
                else
                {
                    WriteToLog(DebugLog, "info", $"{_Tag.name}: Overrange is not above MOP.");
                    _Tag.Errors.Add("Overrange.");
                    if (!ErrorTags.Contains(_Tag)) ErrorTags.Add(_Tag);
                    return false;
                }
            }
            catch (Exception EX_CheckOverrange)
            {
                WriteToLog(DebugLog, "error", $"{_Tag.name}: EX_CheckOverrange: {EX_CheckOverrange}");
                _Tag.Errors.Add("EX_CheckOverrange.");
                if (!ErrorTags.Contains(_Tag)) ErrorTags.Add(_Tag);
                return false;
            }
        }

        public static bool CheckLinearity(Tag _Tag)
        {
            try
            {
                var result = false;
                var factor = 0;
                //equal
                var equal = _Tag.Low_RegisterRange == _Tag.Low_EURange && _Tag.High_RegisterRange == _Tag.High_EURange;

                //factor of 10
                var registerOver10 = _Tag.Low_RegisterRange / 10 == _Tag.Low_EURange && _Tag.High_RegisterRange / 10 == _Tag.High_EURange;
                var euOver10 = _Tag.Low_RegisterRange == _Tag.Low_EURange / 10 && _Tag.High_RegisterRange == _Tag.High_EURange / 10;
                if (registerOver10 || euOver10) factor = 10;

                //factor of 100
                var registerOver100 = _Tag.Low_RegisterRange / 100 == _Tag.Low_EURange && _Tag.High_RegisterRange / 100 == _Tag.High_EURange;
                var euOver100 = _Tag.Low_RegisterRange == _Tag.Low_EURange / 100 && _Tag.High_RegisterRange == _Tag.High_EURange / 100;
                if (registerOver100 || euOver100) factor = 100;

                //factor of 1000
                var registerOver1000 = _Tag.Low_RegisterRange / 1000 == _Tag.Low_EURange && _Tag.High_RegisterRange / 1000 == _Tag.High_EURange;
                var euOver1000 = _Tag.Low_RegisterRange == _Tag.Low_EURange / 1000 && _Tag.High_RegisterRange == _Tag.High_EURange / 1000;
                if (registerOver1000 || euOver1000) factor = 1000;

                if (!equal && !registerOver10 && !euOver10 && !registerOver100 && !euOver100 && !registerOver1000 && !euOver1000)
                {
                    result = true;
                }

                if (_Tag.DataType == "REAL")
                {
                    if (!result && equal)
                    {
                        var check1 = true;
                        if (_Tag.DoubleParameters.ContainsKey("LowClear_RegisterValue")) check1 = _Tag.DoubleParameters["LowClear_RegisterValue"] == _Tag.DoubleParameters["LowClear_ExpectedResult"];
                        var check2 = true;
                        if (_Tag.DoubleParameters.ContainsKey("Underrange_RegisterValue")) check2 = _Tag.DoubleParameters["Underrange_RegisterValue"] == _Tag.DoubleParameters["Underrange_ExpectedResult"];
                        var check3 = true;
                        if (_Tag.DoubleParameters.ContainsKey("Overrange_RegisterValue")) check3 = _Tag.DoubleParameters["Overrange_RegisterValue"] == _Tag.DoubleParameters["Overrange_ExpectedResult"];
                        var check4 = true;
                        if (_Tag.DoubleParameters.ContainsKey("HighClear_RegisterValue")) check4 = _Tag.DoubleParameters["HighClear_RegisterValue"] == _Tag.DoubleParameters["HighClear_ExpectedResult"];
                        var check5 = true;
                        if (_Tag.DoubleParameters.ContainsKey("`LL_RegisterValue")) check5 = _Tag.DoubleParameters["LL_RegisterValue"] == _Tag.DoubleParameters["LL_ExpectedResult"];
                        var check6 = true;
                        if (_Tag.DoubleParameters.ContainsKey("L_RegisterValue")) check6 = _Tag.DoubleParameters["L_RegisterValue"] == _Tag.DoubleParameters["L_ExpectedResult"];
                        var check7 = true;
                        if (_Tag.DoubleParameters.ContainsKey("H_RegisterValue")) check7 = _Tag.DoubleParameters["H_RegisterValue"] == _Tag.DoubleParameters["H_ExpectedResult"];
                        var check8 = true;
                        if (_Tag.DoubleParameters.ContainsKey("HH_RegisterValue")) check8 = _Tag.DoubleParameters["HH_RegisterValue"] == _Tag.DoubleParameters["HH_ExpectedResult"];
                        var check9 = true;
                        if (_Tag.DoubleParameters.ContainsKey("MOP_RegisterValue")) check9 = _Tag.DoubleParameters["MOP_RegisterValue"] == _Tag.DoubleParameters["MOP_ExpectedResult"];

                        result = check1 && check2 && check3 && check4 && check5 && check6 && check7 && check8 && check9;
                    }
                    else if (!result && (registerOver10 || registerOver100 || registerOver1000))
                    {
                        var check1 = true;
                        if (_Tag.DoubleParameters.ContainsKey("LowClear_RegisterValue")) check1 = _Tag.DoubleParameters["LowClear_RegisterValue"] / factor == _Tag.DoubleParameters["LowClear_ExpectedResult"];
                        var check2 = true;
                        if (_Tag.DoubleParameters.ContainsKey("Underrange_RegisterValue")) check2 = _Tag.DoubleParameters["Underrange_RegisterValue"] / factor == _Tag.DoubleParameters["Underrange_ExpectedResult"];
                        var check3 = true;
                        if (_Tag.DoubleParameters.ContainsKey("Overrange_RegisterValue")) check3 = _Tag.DoubleParameters["Overrange_RegisterValue"] / factor == _Tag.DoubleParameters["Overrange_ExpectedResult"];
                        var check4 = true;
                        if (_Tag.DoubleParameters.ContainsKey("HighClear_RegisterValue")) check4 = _Tag.DoubleParameters["HighClear_RegisterValue"] / factor == _Tag.DoubleParameters["HighClear_ExpectedResult"];
                        var check5 = true;
                        if (_Tag.DoubleParameters.ContainsKey("LL_RegisterValue")) check5 = _Tag.DoubleParameters["LL_RegisterValue"] / factor == _Tag.DoubleParameters["LL_ExpectedResult"];
                        var check6 = true;
                        if (_Tag.DoubleParameters.ContainsKey("L_RegisterValue")) check6 = _Tag.DoubleParameters["L_RegisterValue"] / factor == _Tag.DoubleParameters["L_ExpectedResult"];
                        var check7 = true;
                        if (_Tag.DoubleParameters.ContainsKey("H_RegisterValue")) check7 = _Tag.DoubleParameters["H_RegisterValue"] / factor == _Tag.DoubleParameters["H_ExpectedResult"];
                        var check8 = true;
                        if (_Tag.DoubleParameters.ContainsKey("HH_RegisterValue")) check8 = _Tag.DoubleParameters["HH_RegisterValue"] / factor == _Tag.DoubleParameters["HH_ExpectedResult"];
                        var check9 = true;
                        if (_Tag.DoubleParameters.ContainsKey("MOP_RegisterValue")) check9 = _Tag.DoubleParameters["MOP_RegisterValue"] / factor == _Tag.DoubleParameters["MOP_ExpectedResult"];

                        result = check1 && check2 && check3 && check4 && check5 && check6 && check7 && check8 && check9;
                    }
                    else if (!result && (euOver10 || euOver100 || euOver1000))
                    {
                        var check1 = true;
                        if (_Tag.DoubleParameters.ContainsKey("LowClear_RegisterValue")) check1 = _Tag.DoubleParameters["LowClear_RegisterValue"] == _Tag.DoubleParameters["LowClear_ExpectedResult"] / factor;
                        var check2 = true;
                        if (_Tag.DoubleParameters.ContainsKey("Underrange_RegisterValue")) check2 = _Tag.DoubleParameters["Underrange_RegisterValue"] == _Tag.DoubleParameters["Underrange_ExpectedResult"] / factor;
                        var check3 = true;
                        if (_Tag.DoubleParameters.ContainsKey("Overrange_RegisterValue")) check3 = _Tag.DoubleParameters["Overrange_RegisterValue"] == _Tag.DoubleParameters["Overrange_ExpectedResult"] / factor;
                        var check4 = true;
                        if (_Tag.DoubleParameters.ContainsKey("HighClear_RegisterValue")) check4 = _Tag.DoubleParameters["HighClear_RegisterValue"] == _Tag.DoubleParameters["HighClear_ExpectedResult"] / factor;
                        var check5 = true;
                        if (_Tag.DoubleParameters.ContainsKey("LL_RegisterValue")) check5 = _Tag.DoubleParameters["LL_RegisterValue"] == _Tag.DoubleParameters["LL_ExpectedResult"] / factor;
                        var check6 = true;
                        if (_Tag.DoubleParameters.ContainsKey("L_RegisterValue")) check6 = _Tag.DoubleParameters["L_RegisterValue"] == _Tag.DoubleParameters["L_ExpectedResult"] / factor;
                        var check7 = true;
                        if (_Tag.DoubleParameters.ContainsKey("H_RegisterValue")) check7 = _Tag.DoubleParameters["H_RegisterValue"] == _Tag.DoubleParameters["H_ExpectedResult"] / factor;
                        var check8 = true;
                        if (_Tag.DoubleParameters.ContainsKey("HH_RegisterValue")) check8 = _Tag.DoubleParameters["HH_RegisterValue"] == _Tag.DoubleParameters["HH_ExpectedResult"] / factor;
                        var check9 = true;
                        if (_Tag.DoubleParameters.ContainsKey("MOP_RegisterValue")) check9 = _Tag.DoubleParameters["MOP_RegisterValue"] == _Tag.DoubleParameters["MOP_ExpectedResult"] / factor;

                        result = check1 && check2 && check3 && check4 && check5 && check6 && check7 && check8 && check9;
                    }
                }
                else if (_Tag.DataType == "INT")
                {
                    if (!result && equal)
                    {
                        var check1 = true;
                        if (_Tag.IntParameters.ContainsKey("LowClear_RegisterValue")) check1 = _Tag.IntParameters["LowClear_RegisterValue"] == _Tag.DoubleParameters["LowClear_ExpectedResult"];
                        var check2 = true;
                        if (_Tag.IntParameters.ContainsKey("Underrange_RegisterValue")) check2 = _Tag.IntParameters["Underrange_RegisterValue"] == _Tag.DoubleParameters["Underrange_ExpectedResult"];
                        var check3 = true;
                        if (_Tag.IntParameters.ContainsKey("Overrange_RegisterValue")) check3 = _Tag.IntParameters["Overrange_RegisterValue"] == _Tag.DoubleParameters["Overrange_ExpectedResult"];
                        var check4 = true;
                        if (_Tag.IntParameters.ContainsKey("HighClear_RegisterValue")) check4 = _Tag.IntParameters["HighClear_RegisterValue"] == _Tag.DoubleParameters["HighClear_ExpectedResult"];
                        var check5 = true;
                        if (_Tag.IntParameters.ContainsKey("`LL_RegisterValue")) check5 = _Tag.IntParameters["LL_RegisterValue"] == _Tag.DoubleParameters["LL_ExpectedResult"];
                        var check6 = true;
                        if (_Tag.IntParameters.ContainsKey("L_RegisterValue")) check6 = _Tag.IntParameters["L_RegisterValue"] == _Tag.DoubleParameters["L_ExpectedResult"];
                        var check7 = true;
                        if (_Tag.IntParameters.ContainsKey("H_RegisterValue")) check7 = _Tag.IntParameters["H_RegisterValue"] == _Tag.DoubleParameters["H_ExpectedResult"];
                        var check8 = true;
                        if (_Tag.IntParameters.ContainsKey("HH_RegisterValue")) check8 = _Tag.IntParameters["HH_RegisterValue"] == _Tag.DoubleParameters["HH_ExpectedResult"];
                        var check9 = true;
                        if (_Tag.IntParameters.ContainsKey("MOP_RegisterValue")) check9 = _Tag.IntParameters["MOP_RegisterValue"] == _Tag.DoubleParameters["MOP_ExpectedResult"];

                        result = check1 && check2 && check3 && check4 && check5 && check6 && check7 && check8 && check9;
                    }
                    else if (!result && (registerOver10 || registerOver100 || registerOver1000))
                    {
                        var check1 = true;
                        if (_Tag.IntParameters.ContainsKey("LowClear_RegisterValue")) check1 = (double)_Tag.IntParameters["LowClear_RegisterValue"] / factor == _Tag.DoubleParameters["LowClear_ExpectedResult"];
                        var check2 = true;
                        if (_Tag.IntParameters.ContainsKey("Underrange_RegisterValue")) check2 = (double)_Tag.IntParameters["Underrange_RegisterValue"] / factor == _Tag.DoubleParameters["Underrange_ExpectedResult"];
                        var check3 = true;
                        if (_Tag.IntParameters.ContainsKey("Overrange_RegisterValue")) check3 = (double)_Tag.IntParameters["Overrange_RegisterValue"] / factor == _Tag.DoubleParameters["Overrange_ExpectedResult"];
                        var check4 = true;
                        if (_Tag.IntParameters.ContainsKey("HighClear_RegisterValue")) check4 = (double)_Tag.IntParameters["HighClear_RegisterValue"] / factor == _Tag.DoubleParameters["HighClear_ExpectedResult"];
                        var check5 = true;
                        if (_Tag.IntParameters.ContainsKey("LL_RegisterValue")) check5 = (double)_Tag.IntParameters["LL_RegisterValue"] / factor == _Tag.DoubleParameters["LL_ExpectedResult"];
                        var check6 = true;
                        if (_Tag.IntParameters.ContainsKey("L_RegisterValue")) check6 = (double)_Tag.IntParameters["L_RegisterValue"] / factor == _Tag.DoubleParameters["L_ExpectedResult"];
                        var check7 = true;
                        if (_Tag.IntParameters.ContainsKey("H_RegisterValue")) check7 = (double)_Tag.IntParameters["H_RegisterValue"] / factor == _Tag.DoubleParameters["H_ExpectedResult"];
                        var check8 = true;
                        if (_Tag.IntParameters.ContainsKey("HH_RegisterValue")) check8 = (double)_Tag.IntParameters["HH_RegisterValue"] / factor == _Tag.DoubleParameters["HH_ExpectedResult"];
                        var check9 = true;
                        if (_Tag.IntParameters.ContainsKey("MOP_RegisterValue")) check9 = (double)_Tag.IntParameters["MOP_RegisterValue"] / factor == _Tag.DoubleParameters["MOP_ExpectedResult"];

                        result = check1 && check2 && check3 && check4 && check5 && check6 && check7 && check8 && check9;
                    }
                    else if (!result && (euOver10 || euOver100 || euOver1000))
                    {
                        var check1 = true;
                        if (_Tag.IntParameters.ContainsKey("LowClear_RegisterValue")) check1 = _Tag.IntParameters["LowClear_RegisterValue"] == _Tag.DoubleParameters["LowClear_ExpectedResult"] / factor;
                        var check2 = true;
                        if (_Tag.IntParameters.ContainsKey("Underrange_RegisterValue")) check2 = _Tag.IntParameters["Underrange_RegisterValue"] == _Tag.DoubleParameters["Underrange_ExpectedResult"] / factor;
                        var check3 = true;
                        if (_Tag.IntParameters.ContainsKey("Overrange_RegisterValue")) check3 = _Tag.IntParameters["Overrange_RegisterValue"] == _Tag.DoubleParameters["Overrange_ExpectedResult"] / factor;
                        var check4 = true;
                        if (_Tag.IntParameters.ContainsKey("HighClear_RegisterValue")) check4 = _Tag.IntParameters["HighClear_RegisterValue"] == _Tag.DoubleParameters["HighClear_ExpectedResult"] / factor;
                        var check5 = true;
                        if (_Tag.IntParameters.ContainsKey("LL_RegisterValue")) check5 = _Tag.IntParameters["LL_RegisterValue"] == _Tag.DoubleParameters["LL_ExpectedResult"] / factor;
                        var check6 = true;
                        if (_Tag.IntParameters.ContainsKey("L_RegisterValue")) check6 = _Tag.IntParameters["L_RegisterValue"] == _Tag.DoubleParameters["L_ExpectedResult"] / factor;
                        var check7 = true;
                        if (_Tag.IntParameters.ContainsKey("H_RegisterValue")) check7 = _Tag.IntParameters["H_RegisterValue"] == _Tag.DoubleParameters["H_ExpectedResult"] / factor;
                        var check8 = true;
                        if (_Tag.IntParameters.ContainsKey("HH_RegisterValue")) check8 = _Tag.IntParameters["HH_RegisterValue"] == _Tag.DoubleParameters["HH_ExpectedResult"] / factor;
                        var check9 = true;
                        if (_Tag.IntParameters.ContainsKey("MOP_RegisterValue")) check9 = _Tag.IntParameters["MOP_RegisterValue"] == _Tag.DoubleParameters["MOP_ExpectedResult"] / factor;

                        result = check1 && check2 && check3 && check4 && check5 && check6 && check7 && check8 && check9;
                    }
                }
                if (result)
                {
                    WriteToLog(DebugLog, "info", $"{_Tag.name}: Successfully verified linearity of values, or ranges are not linear.");
                    return true;
                }
                else
                {
                    WriteToLog(DebugLog, "info", $"{_Tag.name}: Values are not linear.");
                    _Tag.Errors.Add("Linearity.");
                    if (!ErrorTags.Contains(_Tag)) ErrorTags.Add(_Tag);
                    return false;
                }
            }
            catch (Exception EX_CheckLinearity)
            {
                WriteToLog(DebugLog, "error", $"{_Tag.name}: EX_CheckLinearity: {EX_CheckLinearity}");
                _Tag.Errors.Add("EX_CheckLinearity.");
                if (!ErrorTags.Contains(_Tag)) ErrorTags.Add(_Tag);
                return false;
            }
        }

        public static void DisplayErrorTags(List<Tag> _ErrorTags)
        {
            var displayString = $"{Environment.NewLine}The following tags should be checked:{Environment.NewLine}";
            foreach (var et in _ErrorTags)
            {
                displayString += $"{et.name}:";
                foreach (var e in et.Errors)
                {
                    displayString += $"{e} ";
                }
                displayString += $"{Environment.NewLine}";
            }
            WriteToLog(DebugLog, "info", displayString, false);
        }
    }

    public class Tag
    {
        public string name = string.Empty;
        public string register_EURange = string.Empty;
        public double Low_RegisterRange = 0.0;
        public double High_RegisterRange = 0.0;
        public double Low_EURange = 0.0;
        public double High_EURange = 0.0;
        public string DataType = string.Empty;

        public Dictionary<string, int> IntParameters = new Dictionary<string, int>();
        public Dictionary<string, double> DoubleParameters = new Dictionary<string, double>();
        public List<string> Errors = new List<string>();
    }

    [AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
    [ComVisible(true)]
    public sealed class AssemblyVersionAttribute : Attribute { }
}
