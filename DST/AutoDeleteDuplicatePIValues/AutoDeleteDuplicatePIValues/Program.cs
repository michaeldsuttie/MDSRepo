using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PISDK;

namespace AutoDeleteDuplicatePIValues
{
    class Program
    {
        public static string workingDir { get; set; }
        public static string piServerName { get; set; }
        public static string starttime { get; set; }
        public static string endtime { get; set; }
        public static PIPoint piPoint { get; set; }
        public static List<string> TagList { get; set; }
        public static List<string> Messages { get; set; }

        static void Main(string[] args)
        {
            string workingDir = AppDomain.CurrentDomain.BaseDirectory + "tagList.csv";
            Messages = new List<string>();
            TagList = new List<string>();

            parseCSVtoList(workingDir);

            Console.WriteLine("Working Directory: {0}", workingDir);
            Messages.Add(string.Format("Working Directory:,{0}", workingDir));
            Console.WriteLine("PI Server: {0}", piServerName);
            Messages.Add(string.Format("PI Server:,{0}", piServerName));
            Console.WriteLine("Start Time: {0}", starttime);
            Messages.Add(string.Format("Start Time:,{0}", starttime));
            Console.WriteLine("End Time: {0}", endtime);
            Messages.Add(string.Format("End Time:,{0}", endtime));

            foreach (var tag in TagList)
            {
                Console.WriteLine("Will process tag: {0}", tag);
            }

            Console.WriteLine("Enter 'clearalldata' to only remove all values for tag over timerange. Else, press Enter to remove Duplicate values only.");
            var response = Console.ReadLine();
            if (response.ToString() == "clearalldata")
            {
                Console.WriteLine("Will remove all values for selected points over timerange.");
                Console.WriteLine("Press Enter To Continue...");
                Console.ReadLine();
                Console.WriteLine(Environment.NewLine);
                Console.WriteLine(Environment.NewLine);

                RemoveAllValues();
            }
            else
            {
                Console.WriteLine("Will remove duplicate values for selected points over timerange.");
                Console.WriteLine("Press Enter To Continue...");
                Console.ReadLine();
                Console.WriteLine(Environment.NewLine);
                Console.WriteLine(Environment.NewLine);

                RemoveDuplicateValues();
            }


            Messages.Add(string.Format("Process Completed"));

            WriteToFile(Messages);
            Console.WriteLine("Press Enter To Exit...");
            Console.ReadLine();
        }

        public static void RemoveDuplicateValues()
        {
            int tagCount = TagList.Count();
            int currentTagNum = 1;

            foreach (var tag in TagList)
            {
                if (tag.Contains(piServerName) || tag.Contains(starttime) || tag.Contains(endtime) || tag.Contains(workingDir))
                {
                    continue;
                }
                Console.WriteLine("Clearing Duplicates for: " + tag);
                int TotalEventsDeleted = 0;


                var piValues = getPIValues(tag);
                if (piValues == null)
                {
                    continue;
                }

                //Find Duplicates
                PIValue tmpValue = null;

                int valueCount = piValues.Count;
                int currentValueNum = 1;


                foreach (PIValue currentValue in piValues)
                {
                    if (tmpValue == null)
                    {
                        tmpValue = currentValue;
                        continue;
                    }
                    else if (currentValue.TimeStamp.UTCSeconds == tmpValue.TimeStamp.UTCSeconds)
                    {
                        if (currentValue.Value <= tmpValue.Value)
                        {
                            Console.WriteLine(string.Format("{0}, {1}", currentValue.Value.ToString(), currentValue.TimeStamp.LocalDate.ToString()));
                            Messages.Add(string.Format("{0},{1},{2},{3}", "KEPT", tag, tmpValue.Value.ToString(), tmpValue.TimeStamp.LocalDate.ToString()));
                            Messages.Add(string.Format("{0},{1},{2},{3}", "REMOVED", tag, currentValue.Value.ToString(), currentValue.TimeStamp.LocalDate.ToString()));
                            try
                            {
                                piPoint.Data.RemoveValues(currentValue.TimeStamp.UTCSeconds, currentValue.TimeStamp.UTCSeconds, DataRemovalConstants.drRemoveFirstOnly);
                            }
                            catch (Exception deleteE)
                            {
                                Console.WriteLine(string.Format("There was an error while deleting a duplicate value for tag:,{0}", tag));
                                Messages.Add(string.Format("{0},There was an error while deleting a duplicate value for tag", tag, tag));
                                Console.WriteLine(deleteE.ToString());
                            }
                            TotalEventsDeleted++;
                        }
                        else if (tmpValue.Value <= currentValue.Value)
                        {
                            Console.WriteLine(tmpValue.Value.ToString() + " " + tmpValue.TimeStamp.LocalDate.ToString());
                            try
                            {
                                piPoint.Data.RemoveValues(tmpValue.TimeStamp.UTCSeconds, tmpValue.TimeStamp.UTCSeconds, DataRemovalConstants.drRemoveFirstOnly);
                            }
                            catch (Exception deleteE)
                            {
                                Console.WriteLine(string.Format("There was an error while deleting a duplicate value for tag: {0}", tag));
                                Messages.Add(string.Format("{0},There was an error while deleting a duplicate value for tag", tag));
                                Console.WriteLine(deleteE.ToString());
                            }
                            TotalEventsDeleted++;
                        }
                    }

                    else
                    {
                        tmpValue = currentValue;
                    }
                }

                if (TotalEventsDeleted == 0)
                {
                    Console.WriteLine(string.Format("No duplicate events for tag: {0}", tag));
                    Messages.Add(string.Format("{0},No duplicate events for tag", tag));
                    Console.WriteLine(Environment.NewLine);
                }
                else
                {
                    Console.WriteLine(string.Format("{0} events have been deleted for tag: {1}", TotalEventsDeleted, tag));
                    Console.WriteLine(Environment.NewLine);
                    Messages.Add(string.Format("{0},{1} events have been deleted for tag", tag, TotalEventsDeleted));
                }
            }
        }

        public static void RemoveAllValues()
        {

            int tagCount = TagList.Count();
            int currentTagNum = 1;

            foreach (var tag in TagList)
            {
                if (tag.Contains(piServerName) || tag.Contains(starttime) || tag.Contains(endtime) || tag.Contains(workingDir))
                {
                    continue;
                }
                Console.WriteLine("Clearing all values for: " + tag);
                int TotalEventsDeleted = 0;

                var piValues = getPIValues(tag);
                if (piValues == null)
                {
                    continue;
                }

                int valueCount = piValues.Count;
                int currentValueNum = 1;

                foreach (PIValue value in piValues)
                {
                    Console.WriteLine(string.Format("(TAG {0} of {1}),(VALUE {2} of {3}),{4}, {5}", currentTagNum, tagCount, currentValueNum, valueCount, value.TimeStamp.LocalDate.ToString(), value.Value.ToString()));
                    Messages.Add(string.Format("(TAG {0} of {1}),(VALUE {2} of {3}),{4},{5},{6}", currentTagNum, tagCount, currentValueNum, valueCount, tag, value.TimeStamp.LocalDate.ToString(), value.Value.ToString()));
                    try
                    {
                        piPoint.Data.RemoveValues(value.TimeStamp.UTCSeconds, value.TimeStamp.UTCSeconds, DataRemovalConstants.drRemoveFirstOnly);
                    }
                    catch (Exception deleteE)
                    {
                        Console.WriteLine(string.Format("There was an error while deleting a value for tag:,{0}", tag));
                        Messages.Add(string.Format("{0},There was an error while deleting a value for tag", tag, tag));
                        Console.WriteLine(deleteE.ToString());
                    }
                    currentValueNum++;
                    TotalEventsDeleted++;
                }

                if (TotalEventsDeleted == 0)
                {
                    Console.WriteLine(string.Format("No events found for tag: {0}", tag));
                    Messages.Add(string.Format("{0},No values found for tag", tag));
                    Console.WriteLine(Environment.NewLine);
                }
                else
                {
                    Console.WriteLine(string.Format("{0} events have been deleted for tag: {1}", TotalEventsDeleted, tag));
                    Console.WriteLine(Environment.NewLine);
                    Messages.Add(string.Format("{0},{1} events have been deleted for tag", tag, TotalEventsDeleted));
                }
                currentTagNum++;
            }
        }

        public static void parseCSVtoList(string path)
        {
            TagList.Clear();

            using (StreamReader readFile = new StreamReader(path))
            {
                string line;
                string[] row;
                int rowCount = 0;

                while ((line = readFile.ReadLine()) != null)
                {
                    row = line.Split(',');
                    if (row[0].Contains("x"))
                    {
                        string tagName = row[1];
                        TagList.Add(tagName);
                    }
                    else if (row[0].Contains("startTime"))
                    {
                        var startTimeDT = DateTime.Parse(row[1]);
                        starttime = startTimeDT.ToString("dd-MMM-yyyy hh:mm:ss");
                    }
                    else if (row[0].Contains("endTime"))
                    {
                        var endTimeDT = DateTime.Parse(row[1]);
                        endtime = endTimeDT.ToString("dd-MMM-yyyy hh:mm:ss");
                    }
                    else if (row[0].Contains("piServer"))
                    {
                        piServerName = row[1];
                    }
                    else if (row[0].Contains("workingDirectory"))
                    {
                        workingDir = row[1];
                    }
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
                Console.WriteLine(string.Format("{0} Not Found", tag));
            }

            try
            {
                piPoint = piServer.PIPoints[tag];
                _piValues = piPoint.Data.RecordedValues(starttime, endtime, BoundaryTypeConstants.btInside);
                return _piValues;
            }
            catch (Exception collectionE)
            {
                Console.WriteLine(collectionE.ToString());
                Console.WriteLine("There was an error collecting archived data from PI for PI Server: " + piServerName);
            }
            return null;
        }

        private static void WriteToFile(List<string> messages)
        {
            string fileName = "Log";
            string filePath = AppDomain.CurrentDomain.BaseDirectory;
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
            fileName = filePath + fileName;

            var FileNameFormat = fileName + "{0}";
            int i = 1;
            while (File.Exists(fileName + ".csv"))
            {
                fileName = string.Format(FileNameFormat, "(" + (i++) + ")");
            }

            StreamWriter sw = new StreamWriter(fileName + ".csv", true);
            foreach (var message in messages)
            {
                sw.Write(message);
                sw.Write(Environment.NewLine);
            }
            sw.Flush();
            sw.Close();
        }
    }
}
