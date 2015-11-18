using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NLog;
using NLog.Targets;
using OSIsoft.AF;
using OSIsoft.AF.Asset;
using OSIsoft.AF.Search;
using OSIsoft.AF.Time;
using System.Reflection;
using OSIsoft.AF.PI;
using System.Diagnostics;


namespace MovePIValues
{
    class Program
    {
        public static Logger DebugLog;
        public static string workingDirectory = AppDomain.CurrentDomain.BaseDirectory;
        //public static string afServerName { get; set; }
        public static PISystem myAFServer { get; set; }
        //public static string afDatabaseName { get; set; }
        public static AFDatabase myAFDB { get; set; }
        //public static string piServerName { get; set; }
        public static PIServer myPIServer { get; set; }
        //public static string startTime { get; set; }
        //public static string endTime { get; set; }
        //public static string afElementRootName { get; set; }
        public static PIPoint piPoint { get; set; }

        static void Main(string[] args)
        {
            var sw = new Stopwatch();
            sw.Start();
            UpdateLogger();
            Console.WriteLine("###############################################");
            Console.WriteLine(string.Format("App Started at: {0}", DateTime.Now));
            DebugLog.Info(string.Format("App Started at: {0}", DateTime.Now));
            DebugLog.Info(string.Format("Seconds elapsed since start: {0}", sw.ElapsedMilliseconds / 1000));
            Console.WriteLine("###############################################");
            Console.WriteLine(Environment.NewLine);

            //AF connection parameters
            var afServerName = args[0];
            DebugLog.Info(string.Format("afServerName: '{0}'", afServerName));
            var afDatabaseName = args[1];
            DebugLog.Info(string.Format("afDatabaseName: '{0}'", afDatabaseName));
            //PI connection parameters
            var piServerName = args[2];
            DebugLog.Info(string.Format("piServerName: '{0}'", piServerName));
            var afElementRootName = args[3];
            //Search parameters
            DebugLog.Info(string.Format("afElementRootName: '{0}'", afElementRootName));
            //var startTimeDT = DateTime.Parse(args[4]);
            //var startTime = startTimeDT.ToString("dd-MMM-yyyy HH:mm:ss");
            var startTime = DateTime.Today.ToString("dd-MMM-yyyy HH:mm:ss");
            DebugLog.Info(string.Format("startTime: '{0}'", startTime));
            //var endTimeDT = DateTime.Parse(args[5]);
            //var endTime = endTimeDT.ToString("dd-MMM-yyyy HH:mm:ss");
            var endTime = DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss");

            DebugLog.Info(string.Format("endTime: '{0}'", endTime));

            //parseCSVtoList(workingDirectory);
            ConnectToAF(afServerName, afDatabaseName);
            ConnectToPI(piServerName);

            var ErrorElements = new AFNamedCollectionList<AFElement>();
            var ErrorPoints = new List<string>();
            var PIPoints = new List<PIPoint>();

            //get elements directly under root
            Console.WriteLine(string.Format("Retrieving elements from: {0}...", afElementRootName));
            DebugLog.Info(string.Format("Retrieving elements from: {0}...", afElementRootName));
            var FilteredElements1 = GetElementsByRoot(afElementRootName).Distinct();//.Where(x => (x.Name == "033325" || x.Name == "018475" || x.Name == "027587" || x.Name == "002240"));
            Console.WriteLine(string.Format("Retrieved {0} elements from: {1}", FilteredElements1.Count(), afElementRootName));
            DebugLog.Info(string.Format("Retrieved {0} elements from: {1}", FilteredElements1.Count(), afElementRootName));
            DebugLog.Info(string.Format("Seconds elapsed since start: {0}", sw.ElapsedMilliseconds / 1000));
            Console.WriteLine(Environment.NewLine);

            //get and aggregate elements below FilteredElements1
            Console.WriteLine(string.Format("Retrieving and aggregating elements from root elements previously retrieved..."));
            DebugLog.Info(string.Format("Retrieving and aggregating elements from root elements previously retrieved..."));
            var feTotalCount = FilteredElements1.Count();
            var feCurrentCount = 1;
            var AggregateElements2 = new AFNamedCollectionList<AFElement>();
            foreach (var fe1 in FilteredElements1)
            {
                var elements2Root = string.Format(@"{0}\{1}", afElementRootName, fe1.Name);
                var Elements2Temp = GetElementsByRoot(elements2Root);
                foreach (var e2T in Elements2Temp)
                {
                    //DebugLog.Info(string.Format(@"Retrieved element: {0}\{1}", fe1.Name, e2T.Name));
                    AggregateElements2.Add(e2T);
                    //DebugLog.Info(string.Format("Retrieved 'Energy_Daily' PIPoint: {0}", e2T.Attributes["Energy_Daily"].PIPoint));
                    //PIPoints.Add(e2T.Attributes["Energy_Daily"].PIPoint);

                }
                drawTextProgressBar(feCurrentCount, feTotalCount);
                feCurrentCount++;
            }
            Console.WriteLine(string.Format("Retrieved {0} aggregated elements", AggregateElements2.Count()));
            DebugLog.Info(string.Format("Retrieved {0} aggregated elements", AggregateElements2.Count()));
            DebugLog.Info(string.Format("Seconds elapsed since start: {0}", sw.ElapsedMilliseconds / 1000));
            Console.WriteLine(Environment.NewLine);

            //get pipoint references for aggregated elements
            Console.WriteLine(string.Format("Adding PIPoint references to list..."));
            DebugLog.Info(string.Format("Adding PIPoint references to list..."));
            var ae2TotalCount = AggregateElements2.Count;
            var ae2CurrentCount = 1;
            foreach (var ae2 in AggregateElements2)
            {
                try
                {
                    DebugLog.Info(string.Format(@"Retrieved 'Energy_Daily' PIPoint: '{0}' from {1}\{2} ", ae2.Attributes["Energy_Daily"].PIPoint, ae2.Parent.Name, ae2.Name));
                    PIPoints.Add(ae2.Attributes["Energy_Daily"].PIPoint);
                }
                catch (Exception getPIPointsEX)
                {
                    DebugLog.Error(string.Format(@"Failed to add PIPoint(s) from: {0}\{1}: {2}", ae2.Parent.Name, ae2.Name, getPIPointsEX.Message));
                    ErrorElements.Add(ae2);
                }
                drawTextProgressBar(ae2CurrentCount, ae2TotalCount);
                ae2CurrentCount++;
            }
            Console.WriteLine(string.Format("Retrieved {0} distinct point references.", PIPoints.Distinct().Count()));
            DebugLog.Info(string.Format("Retrieved {0} distinct point references.", PIPoints.Distinct().Count()));
            DebugLog.Info(string.Format("Seconds elapsed since start: {0}", sw.ElapsedMilliseconds / 1000));
            Console.WriteLine(Environment.NewLine);

            //get values, edit timestamp of current value, insert new value with edited timestamp, remove current value
            foreach (var p in PIPoints.Distinct())
            {
                Console.WriteLine(Environment.NewLine);
                Console.WriteLine(string.Format("Retrieving Values for PIPoint: {0}", p.Name));
                DebugLog.Info(string.Format("Retrieving Values for PIPoint: {0}", p.Name));
                var values = getPIValues(myPIServer, p, startTime, endTime);
                var count = 0;
                foreach (var v in values)
                {
                    var vTString = v.Timestamp.ToString();
                    var vTStringTrimmed = vTString.Substring(vTString.Length - 11);
                    if (vTStringTrimmed != "12:00:00 AM")
                    {
                        Console.WriteLine(string.Format("Current | Value: {0}, Timestamp: {1}", v.Value, v.Timestamp));
                        DebugLog.Info(string.Format("Current | Value: {0}, Timestamp: {1}", v.Value, v.Timestamp));
                        var newTimestamp = (DateTime)v.Timestamp;
                        var newTimestampTemp = newTimestamp.AddDays(-1).ToShortDateString();
                        var newAFValue = new AFValue();
                        newAFValue.Value = v.Value;
                        newAFValue.Timestamp = AFTime.Parse(newTimestampTemp);
                        Console.WriteLine(string.Format("New     | Value: {0}, Timestamp: {1}", newAFValue.Value, newAFValue.Timestamp));
                        DebugLog.Info(string.Format("New     | Value: {0}, Timestamp: {1}", newAFValue.Value, newAFValue.Timestamp));
                        //Console.WriteLine("Enter 'y' to insert new values and remove current.");
                        //var response = Console.ReadLine();
                        //if (response.ToString() == "y")
                        //{
                        p.UpdateValue(newAFValue, OSIsoft.AF.Data.AFUpdateOption.Insert);
                        Console.WriteLine("New Value Inserted.");
                        DebugLog.Info("New Value Inserted.");
                        p.UpdateValue(v, OSIsoft.AF.Data.AFUpdateOption.Remove);
                        Console.WriteLine("Current Value Removed.");
                        DebugLog.Info("Current Value Removed.");
                        //}
                        count++;
                    }
                }
                if (count == 0)
                {
                    Console.WriteLine(string.Format("No non-midnight values found for {0}", p.Name));
                    DebugLog.Info(string.Format("No non-midnight values found for {0}", p.Name));
                    Console.WriteLine(Environment.NewLine);
                }
            }
            DebugLog.Info(string.Format("Seconds elapsed since start: {0}", sw.ElapsedMilliseconds / 1000));

            //report errors
            if (ErrorElements.Count != 0)
            {
                Console.WriteLine(Environment.NewLine);
                Console.WriteLine("------------------------------------------------");
                var eeTotalCount = ErrorElements.Count;
                var eeCurrentCount = 1;
                Console.WriteLine(string.Format("Experienced errors for the following {0} elements...", eeTotalCount));
                DebugLog.Error(string.Format("Experienced errors for the following {0} elements...", eeTotalCount));
                foreach (var ee in ErrorElements)
                {
                    Console.WriteLine(string.Format(@"Element {0}/{1}: {2}\{3}", eeCurrentCount, eeTotalCount, ee.Parent, ee.Name));
                    DebugLog.Error(string.Format(@"Element {0}/{1}: {2}\{3}", eeCurrentCount, eeTotalCount, ee.Parent, ee.Name));
                    eeCurrentCount++;
                }
                Console.WriteLine("------------------------------------------------");
            }
            myAFServer.Disconnect();
            myPIServer.Disconnect();
            DebugLog.Info(string.Format("Seconds elapsed since start: {0}", sw.ElapsedMilliseconds / 1000));
            Exit();
        }

        private static void drawTextProgressBar(int progress, int total)
        {
            //draw empty progress bar
            Console.CursorLeft = 0;
            Console.Write("["); //start
            Console.CursorLeft = 32;
            Console.Write("]"); //end
            Console.CursorLeft = 1;
            float onechunk = 30.0f / total;

            //draw filled part
            int position = 1;
            for (int i = 0; i < onechunk * progress; i++)
            {
                Console.BackgroundColor = ConsoleColor.Gray;
                Console.CursorLeft = position++;
                Console.Write(" ");
            }

            //draw unfilled part
            for (int i = position; i <= 31; i++)
            {
                Console.BackgroundColor = ConsoleColor.Black;
                Console.CursorLeft = position++;
                Console.Write(" ");
            }

            //draw totals
            Console.CursorLeft = 35;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Write(progress.ToString() + " of " + total.ToString() + "    "); //blanks at the end remove any excess
            if (total - progress == 0)
            {
                Console.WriteLine(Environment.NewLine);
            }
        }
        private static void ConnectToAF(string _afServerName, string _afDatabaseName)
        {
            myAFServer = new PISystems().DefaultPISystem;
            //myAFServer.GetPath().Where(x => x.Name == _afServerName);
            myAFDB = myAFServer.Databases[_afDatabaseName];
            try
            {
                Console.WriteLine(string.Format(@"Trying anonymous connection to: \\{0}\{1}", myAFServer.Name, myAFDB.Name));
                DebugLog.Info(string.Format(@"Trying anonymous connection to: \\{0}\{1}", myAFServer.Name, myAFDB.Name));

                myAFServer.Connect();
                Console.WriteLine(string.Format(@"Successfully connected to: \\{0}\{1}", myAFServer.Name, myAFDB.Name));
                DebugLog.Info(string.Format(@"Successfully connected to: \\{0}\{1}", myAFServer.Name, myAFDB.Name));
            }
            catch (Exception connectToAFDBEX)
            {
                Console.WriteLine(string.Format("Connection Failed: {0}", connectToAFDBEX.Message));
                DebugLog.Error(string.Format("Connection Failed: {0}", connectToAFDBEX.Message));
                Exit();
            }
            Console.WriteLine(Environment.NewLine);
        }
        private static void ConnectToPI(string _myPIServerName)
        {
            //myPIServer = new PIServers().DefaultPIServer;
            myPIServer = PIServer.FindPIServer(_myPIServerName);
            try
            {
                Console.WriteLine(string.Format(@"Trying anonymous connection to: {0}", myPIServer.Name));
                DebugLog.Info(string.Format(@"Trying anonymous connection to: {0}", myPIServer.Name));

                myPIServer.Connect();
                Console.WriteLine(string.Format(@"Successfully connected to: {0}", myPIServer.Name));
                DebugLog.Info(string.Format(@"Successfully connected to: {0}", myPIServer.Name));
            }
            catch (Exception connectToPIEX)
            {
                Console.WriteLine(string.Format("Connection Failed: {0}", connectToPIEX.Message));
                DebugLog.Error(string.Format("Connection Failed: {0}", connectToPIEX.Message));
                Exit();
            }
            Console.WriteLine(Environment.NewLine);
        }

        public static AFNamedCollectionList<AFElement> GetElementsByRoot(string searchRootName)
        {
            var database = myAFDB;
            var searchRoot = database.Elements[searchRootName];
            var query = "*";
            var field = AFSearchField.Name;
            var searchFullHeirarchy = false;
            var sortField = AFSortField.Name;
            var sortOrder = AFSortOrder.Ascending;
            var maxCount = 100000;

            try
            {
                var _elements = AFElement.FindElements(database, searchRoot, query, field, searchFullHeirarchy, sortField, sortOrder, maxCount);
                return _elements;
            }
            catch (Exception getElementsEX)
            {
                Console.WriteLine(string.Format(@"Failed to retrieve elements from: \\{0}\{1}: {2}", myAFServer.Name, myAFDB, getElementsEX.Message));
                DebugLog.Error(string.Format(@"Failed to retrieve elements from: \\{0}\{1}: {2}", myAFServer.Name, myAFDB, getElementsEX.Message));
                Exit();
                return null;
            }
        }

        private static AFValues getPIValues(PIServer _myPIServer, PIPoint piPoint, string _startTime, string _endTime)
        {
            AFValues _afValues;
            PIPoint pointToRetrieve = PIPoint.FindPIPoint(myPIServer, piPoint.ID);
            AFTimeRange _afTimeRange = new AFTimeRange(_startTime, _endTime);
            try
            {
                _afValues = pointToRetrieve.RecordedValues(_afTimeRange, OSIsoft.AF.Data.AFBoundaryType.Inside, "", true);
                return _afValues;
            }
            catch (Exception getPIValuesEX)
            {
                Console.WriteLine(string.Format("There was an error collecting archived data from PI for PI Server: {0}: {1}", myPIServer.Name, getPIValuesEX));
                DebugLog.Error(string.Format("There was an error collecting archived data from PI for PI Server: {0}: {1}", myPIServer.Name, getPIValuesEX));
            }
            return null;
        }

        public static void UpdateLogger()
        {
            string userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name.Substring(8).ToLower();

            DebugLog = LogManager.GetLogger("DebugLog");
            var debugFileTarget = (FileTarget)LogManager.Configuration.LoggingRules.First().Targets.First();
            debugFileTarget.FileName = string.Format(AppDomain.CurrentDomain.BaseDirectory + @"\Logs\{0}\{1}_{2}_DEBUG.txt", DateTime.Now.ToString("yyyy-MM-dd"),
                DateTime.Now.ToString("HH-mm-ss"), userName);
            LogManager.ReconfigExistingLoggers();

            DebugLog.Info(string.Format("LogConfigured: '{0}'", DebugLog.Name));
        }

        public static void Exit()
        {
            Console.WriteLine(Environment.NewLine);
            //Console.WriteLine("Press any key to exit...");
            DebugLog.Info(string.Format("Exiting app at: {0}", System.DateTime.Now));
            //Console.ReadKey();
            Environment.Exit(0);
        }

        //public static void parseCSVtoList(string workingDirectory)
        //{
        //    DebugLog.Info(string.Format("workingDirectory: '{0}'", workingDirectory));

        //    var path = workingDirectory + "config.csv";
        //    DebugLog.Info(string.Format("path: '{0}'", path));
        //    try
        //    {
        //        using (StreamReader readFile = new StreamReader(path))
        //        {
        //            string line;
        //            string[] row;
        //            int rowCount = 0;

        //            while ((line = readFile.ReadLine()) != null)
        //            {
        //                row = line.Split(',');
        //                //if (row[0].Contains("x"))
        //                //{
        //                //    string elementName = row[1];
        //                //    ElementStringList.Add(elementName);
        //                //    DebugLog.Info(string.Format("elementName: '{0}'", elementName));
        //                //}
        //                if (row[0].Contains("endTime"))
        //                {
        //                    var endTimeDT = DateTime.Parse(row[1]);
        //                    endTime = endTimeDT.ToString("dd-MMM-yyyy HH:mm:ss");
        //                    DebugLog.Info(string.Format("endTime: '{0}'", endTime));
        //                }
        //                else if (row[0].Contains("startTime"))
        //                {
        //                    var startTimeDT = DateTime.Parse(row[1]);
        //                    startTime = startTimeDT.ToString("dd-MMM-yyyy HH:mm:ss");
        //                    DebugLog.Info(string.Format("startTime: '{0}'", startTime));
        //                }
        //                else if (row[0].Contains("afElementRootName"))
        //                {
        //                    afElementRootName = row[1];
        //                    DebugLog.Info(string.Format("afElementRootName: '{0}'", afElementRootName));
        //                }
        //                else if (row[0].Contains("afDatabaseName"))
        //                {
        //                    afDatabaseName = row[1];
        //                    DebugLog.Info(string.Format("afDatabaseName: '{0}'", afDatabaseName));
        //                }
        //                else if (row[0].Contains("afServerName"))
        //                {
        //                    afServerName = row[1];
        //                    DebugLog.Info(string.Format("afServerName: '{0}'", afServerName));
        //                }
        //                //else if (row[0].Contains("elementListPath"))
        //                //{
        //                //    elementListPath = row[1];
        //                //    DebugLog.Info(string.Format("elementListPath: '{0}'", elementListPath));
        //                //}
        //                rowCount += 1;
        //            }
        //        }
        //    }
        //    catch (Exception ParseCSVtoListEX)
        //    {
        //        Console.WriteLine(string.Format("Failed to parse CSV: {0}", ParseCSVtoListEX.Message));
        //        DebugLog.Error(string.Format("Failed to parse CSV: {0}", ParseCSVtoListEX.Message));
        //        Exit();
        //    }
        //}
    }
}
