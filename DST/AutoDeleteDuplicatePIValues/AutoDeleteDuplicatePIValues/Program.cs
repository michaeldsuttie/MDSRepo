using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PISDK;
using System.Threading;
using NLog;
using NLog.Targets;
using System.Text.RegularExpressions;

namespace AutoDeleteDuplicatePIValues
{
    class Program
    {
        public static Logger DebugLog;

        public static string workingDir { get; set; }
        public static string piServerName { get; set; }
        public static string starttime { get; set; }
        public static string endtime { get; set; }
        public static PIPoint piPoint { get; set; }
        public static List<string> TagList { get; set; }
        //public static StringBuilder Messages { get; set; }

        static void Main(string[] args)
        {
            //Messages = new StringBuilder();
            UpdateLogger();
            string workingDir = $"{AppDomain.CurrentDomain.BaseDirectory}tagList.csv";
            TagList = new List<string>();

            parseCSVtoList(workingDir);

            WriteToLogs($"Working Directory: {workingDir}");
            WriteToLogs($"PI Server: {piServerName}");
            WriteToLogs($"Start Time: {starttime}");
            WriteToLogs($"End Time: {endtime}");

            int i = 1;
            WriteToLogs($"Will Process Tags:");
            foreach (var tag in TagList)
            {
                WriteToLogs($"Tag {i} of {TagList.Count} | {tag}");
                i++;
            }

            if (PromptUser("remove duplicate values", "enter"))
            {
                RemoveDuplicateValues();
                ExitApp();
            }
            //else if (PromptUser("remove all values", "enter"))
            //{
            //    RemoveAllValues();
            //    ExitApp();
            //}
            ExitApp();
        }

        public static void RemoveDuplicateValues()
        {
            int tagCount = TagList.Count();
            int currentTagNum = 1;

            foreach (var tag in TagList)
            {
                WriteToLogs($"Tag {currentTagNum} of {tagCount} | Tag: {tag} | Checking for duplicates.");
                int TotalEventsDeleted = 0;

                var piValues = getPIValues(tag);
                if (piValues == null)
                {
                    currentTagNum++;
                    continue;
                }

                int valueCount = piValues.Count;
                int currentValueNum = 1;

                PIValue previousValue = null;
                foreach (PIValue currentValue in piValues)
                {
                    if (previousValue == null)
                    {
                        previousValue = currentValue;
                        continue;
                    }
                    else if (currentValue.TimeStamp.UTCSeconds == previousValue.TimeStamp.UTCSeconds)
                    {
                        //int int1;
                        //int int2;
                        //double double1;
                        //double double2;

                        //bool ints = int.TryParse(currentValue.Value, out int1) && int.TryParse(currentValue.Value, out int2);
                        //bool doubles = double.TryParse(currentValue.Value, out double1) && double.TryParse(currentValue.Value, out double2);
                        if (currentValue.Value.ToString().ToLower() == "system.__comobject" || previousValue.Value.ToString().ToLower() == "system.__comobject")
                        {
                            WriteToLogs($"Values are not comparable. Moving to next Value.");
                            currentValueNum++;
                            continue;
                        }

                        string pattern = "[0 - 9] +.[0 - 9] +|[0 - 9]";
                        
                        var cVal = Regex.Match(currentValue.Value, pattern);
                        var pVal = Regex.Match(previousValue.Value, pattern);

                        if (cVal == string.Empty || pVal == string.Empty)
                        {
                            WriteToLogs($"Values are not comparable. Moving to next Value.");
                            currentValueNum++;
                            continue;
                        }
                        if (currentValue.Value == previousValue.Value)
                        {
                            try
                            {
                                piPoint.Data.RemoveValues(currentValue.TimeStamp.UTCSeconds, currentValue.TimeStamp.UTCSeconds, DataRemovalConstants.drRemoveFirstOnly);
                                WriteToLogs($"Tag {currentTagNum} of {tagCount} | KEPT: {previousValue.TimeStamp.LocalDate.ToString("dd-MMM-yyyy HH:mm:ss:ms")} {previousValue.Value} | REMOVED: {currentValue.TimeStamp.LocalDate.ToString("dd-MMM-yyyy HH:mm:ss:ms")} {currentValue.Value}");
                                currentValueNum++;
                                TotalEventsDeleted++;
                                continue;
                            }
                            catch (Exception EX_RemoveDuplicateValues)
                            {
                                WriteToLogs($"Tag {currentTagNum} of {tagCount} | Tag: {tag} | Error Deleting Value. Moving to next tag. Run utility again to remove ramining values. {EX_RemoveDuplicateValues.ToString()}");
                                currentValueNum++;
                                goto NextTag;
                            }
                        }
                        currentValueNum++;
                    }
                    else
                    {
                        //WriteToLogs($"Tag {currentTagNum} of {tagCount} | Value {currentValueNum} of {valueCount} | KEPT: {previousValue.TimeStamp.LocalDate.ToString("dd-MMM-yyyy HH:mm:ss")} {previousValue.Value.ToString()}");
                        previousValue = currentValue;
                        currentValueNum++;
                    }
                }
            NextTag:
                if (TotalEventsDeleted == 0) WriteToLogs($"Tag {currentTagNum} of {tagCount} | Tag: {tag} | No Duplicate Events Found.");
                else WriteToLogs($"Tag {currentTagNum} of {tagCount} | Tag: {tag} | Duplicate Values Deleted: {TotalEventsDeleted}");
                currentTagNum++;
            }
        }

        //public static void RemoveAllValues()
        //{
        //    int tagCount = TagList.Count();
        //    int currentTagNum = 1;

        //    foreach (var tag in TagList)
        //    {
        //        WriteToLogs($"Tag: {tag} | Clearing All Values");
        //        int TotalEventsDeleted = 0;

        //        var piValues = getPIValues(tag);
        //        if (piValues == null) continue;

        //        int valueCount = piValues.Count;
        //        int currentValueNum = 1;

        //        foreach (PIValue value in piValues)
        //        {
        //            WriteToLogs($"Tag {currentTagNum} of {tagCount} | Value {currentValueNum} of {valueCount} | {value.TimeStamp.LocalDate.ToString()}: {value.Value.ToString()}");
        //            try
        //            {
        //                piPoint.Data.RemoveValues(value.TimeStamp.UTCSeconds, value.TimeStamp.UTCSeconds, DataRemovalConstants.drRemoveFirstOnly);
        //            }
        //            catch (Exception EX_RemoveAllValues)
        //            {
        //                WriteToLogs($"Tag: {tag} | Error Deleting Value. {EX_RemoveAllValues.ToString()}");
        //            }
        //            currentValueNum++;
        //            TotalEventsDeleted++;
        //        }

        //        if (TotalEventsDeleted == 0) WriteToLogs($"Tag: {tag} | No Events Found.");
        //        else WriteToLogs($"Tag: {tag} | Events Deleted: {TotalEventsDeleted}");
        //        currentTagNum++;
        //    }
        //}

        public static void parseCSVtoList(string path)
        {
            TagList.Clear();
            if (!File.Exists(path))
            {
                WriteToLogs($"File not found or in use. {path}");
                ExitApp();
            }
            using (StreamReader readFile = new StreamReader(path))
            {
                string line;
                string[] row;
                int rowCount = 0;

                while ((line = readFile.ReadLine()) != null)
                {
                    row = line.Split(',');
                    if (row[0].ToLower().Contains("x")) TagList.Add(row[1]);
                    else if (row[0].ToLower().Contains("starttime")) starttime = DateTime.Parse(row[1]).ToString("dd-MMM-yyyy HH:mm:ss");
                    else if (row[0].ToLower().Contains("endtime")) endtime = DateTime.Parse(row[1]).ToString("dd-MMM-yyyy HH:mm:ss");
                    else if (row[0].ToLower().Contains("piserver")) piServerName = row[1];
                    else if (row[0].ToLower().Contains("workingdirectory")) workingDir = row[1];
                    rowCount += 1;
                }
            }
        }

        private static PIValues getPIValues(string tag)
        {
            PISDK.PISDK sdk = new PISDK.PISDK();
            Server piServer = sdk.Servers[piServerName];
            PIValues _piValues;

            try
            {
                piPoint = piServer.PIPoints[tag];
            }
            catch
            {
                WriteToLogs($"Tag: {tag} | Not Found.");
                return null;
            }

            try
            {
                //string starttimeUTC = DateTime.Parse(starttime).ToUniversalTime().ToString();
                //string endtimeUTC = DateTime.Parse(endtime).ToUniversalTime().ToString();
                _piValues = piPoint.Data.RecordedValues(starttime, endtime, BoundaryTypeConstants.btInside);

                int i = 1;
                foreach (PIValue v in _piValues)
                {
                    WriteToLogs($"Tag: {tag} | Value {i} of {_piValues.Count} | Value: {v.TimeStamp.LocalDate.ToString()} | {v.Value}", true);
                    i++;
                }
                return _piValues;
            }
            catch (Exception EX_getPIValues)
            {
                WriteToLogs($"There was an error collecting archived data from PI Server: {piServerName}. {EX_getPIValues.ToString()}");
            }
            return null;
        }
        //private static void WriteSBToFile(StringBuilder sb)
        //{
        //    string fileName = "Log";
        //    string filePathBase = AppDomain.CurrentDomain.BaseDirectory;
        //    if (!Directory.Exists(filePathBase)) Directory.CreateDirectory(filePathBase);
        //    string filePath = $"{filePathBase}{fileName}";

        //    var FilePathFormat = filePath + "{0}";
        //    int i = 1;
        //    while (File.Exists($"{filePath}.csv")) filePath = string.Format(FilePathFormat, $"({(i++)})");

        //    StreamWriter sw = new StreamWriter(filePath + ".csv", true);
        //    sw.Write(Messages.ToString());
        //    sw.Flush();
        //    sw.Close();
        //}
        private static void WriteToLogs(string message, bool suppress = false)
        {
            //string m = $"{DateTime.Now}: {message}";
            //Messages.AppendLine(m);
            DebugLog.Debug(message);
            if (suppress) return;
            Console.WriteLine(message);
        }
        private static bool PromptUser(string action, string key)
        {
            ConsoleKey ck = new ConsoleKey();
            switch (key.ToLower())
            {
                case "enter":
                    ck = ConsoleKey.Enter;
                    break;
                case "escape":
                    ck = ConsoleKey.Escape;
                    break;
            }
            WriteToLogs($"Press '{key.ToLower()}' to {action}. Press 'Space' to continue.");
            if (Console.ReadKey(true).Key == ck)
            {
                WriteToLogs($"Are you sure you would like to {action}? Press '{key}' to confirm or press 'space' to continue.");
                return Console.ReadKey(true).Key == ck;
            }
            return false;
        }
        public static void UpdateLogger()
        {
            string[] unSplit = System.Security.Principal.WindowsIdentity.GetCurrent().Name.ToLower().Split('\\');
            string domain = unSplit[0];
            string userName = unSplit[1];
            DebugLog = LogManager.GetLogger("DebugLog");
            var debugFileTarget = (FileTarget)LogManager.Configuration.LoggingRules.First().Targets.First();
            debugFileTarget.FileName = $@"{AppDomain.CurrentDomain.BaseDirectory}\Logs\{DateTime.Now.ToString("yyyy-MM-dd")}\{DateTime.Now.ToString("HH-mm-ss")}_{userName}_DEBUG.txt";
            LogManager.ReconfigExistingLoggers();
        }
        public static void ExitApp()
        {
            WriteToLogs($"Exiting app. Check log for details. Press any key to continue.");
            Console.ReadKey();
            Environment.Exit(-1);
        }
    }
}