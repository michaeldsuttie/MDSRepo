using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PMPortal.ServerApp.Models;
using System.Net;
using System.Text;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.IO;
using Newtonsoft.Json.Linq;

namespace PMPortal.ServerApp.Services
{
    public class TogglDataService
    {
        static private string CreateAuthHeader()
        {
            string apiToken = "a2e7f93070afe8475626141477fec42b";
            string userPass = $"{apiToken}:api_token";
            string userPassB64 = Convert.ToBase64String(Encoding.ASCII.GetBytes(userPass.Trim()));
            string authHeader = $"Basic {userPassB64}";

            return authHeader;
        }

        static internal async Task<List<TogglProject>> GetProjects()
        {
            try
            {
                string url = "https://toggl.com/api/v8/workspaces/669485/projects";

                HttpWebRequest authRequest = (HttpWebRequest)WebRequest.Create(url);
                authRequest.Headers["Authorization"] = CreateAuthHeader();
                authRequest.Method = "GET";
                authRequest.ContentType = "application/json";

                var response = await authRequest.GetResponseAsync();
                string result;
                using (Stream stream = response.GetResponseStream())
                {
                    StreamReader sr = new StreamReader(stream);
                    result = await sr.ReadToEndAsync();
                }

                if (null != result)
                {
                    //Debug.WriteLine(result.ToString());
                    return ParseProjectData(result).ToList();
                }
                return null;
            }
            catch (Exception EX_GetProjects)
            {
                //MainViewModel.WriteToLogs("allLogs", "error", $"EX_GetProjects(): {EX_GetProjects}");
                return null;
            }
        }

        static private List<TogglProject> ParseProjectData(string rawData)
        {
            try
            {
                var d = JArray.Parse(rawData);
                var data = (from r in d
                            select new TogglProject
                            {
                                Id = (int)r["id"],
                                Name = (string)r["name"]
                            }).OrderBy(x => x.Name).ToList();
                return data;
            }
            catch (Exception EX_ParseProjectData)
            {
                //MainViewModel.WriteToLogs("allLogs", "error", $"EX_ParseProjectData(rawData={rawData}): {EX_ParseProjectData}");
                return null;
            }
        }

        static internal async Task<List<TogglUser>> GetUsers()
        {
            try
            {
                string url = "https://www.toggl.com/api/v8/workspaces/669485/users";

                HttpWebRequest authRequest = (HttpWebRequest)WebRequest.Create(url);
                authRequest.Headers["Authorization"] = CreateAuthHeader();
                authRequest.Method = "GET";
                authRequest.ContentType = "application/json";

                var response = await authRequest.GetResponseAsync();
                string result = null;
                using (Stream stream = response.GetResponseStream())
                {
                    StreamReader sr = new StreamReader(stream);
                    result = sr.ReadToEnd();
                    //sr.Close();
                }

                if (null != result) return ParseUserData(result);
                return null;

            }
            catch (Exception EX_GetUsers)
            {
                //MainViewModel.WriteToLogs("allLogs", "error", $"EX_GetUsers(): {EX_GetUsers}");
                return null;
            }
        }

        static private List<TogglUser> ParseUserData(string rawData)
        {
            try
            {
                var d = JArray.Parse(rawData);
                var data = (from r in d
                            select new TogglUser
                            {
                                Id = (int)r["id"],
                                Name = (string)r["fullname"]
                            }).ToList();
                return data;
            }
            catch (Exception EX_ParseUserData)
            {
                //MainViewModel.WriteToLogs("allLogs", "error", $"EX_ParseUserData(rawData={rawData}): {EX_ParseUserData}");
                return null;
            }
        }

        static internal async Task<List<TogglTimeEntry>> GetData(DateTime since, DateTime until)
        {
            try
            {
                string txtSince = since.ToString("yyyy-MM-dd");
                string txtUntil = until.ToString("yyyy-MM-dd");
                string url = "https://toggl.com/reports/api/v2/details?workspace_id=669485&since=" + txtSince + "&until=" + txtUntil + "&user_agent=api_test&rounding=on";


                int recordCount = 0;
                int recordsPerPage = 0;
                int pageCount = 0;
                int pages = 0;

                List<TogglTimeEntry> entries = new List<TogglTimeEntry>();
                do
                {
                    pageCount++;

                    url = "https://toggl.com/reports/api/v2/details?workspace_id=669485&since=" + txtSince + "&until=" + txtUntil + "&user_agent=api_test&rounding=off&page=" + pageCount;
                    HttpWebRequest authRequest = (HttpWebRequest)WebRequest.Create(url);
                    authRequest.Headers["Authorization"] = CreateAuthHeader();
                    authRequest.Method = "GET";
                    authRequest.ContentType = "application/json";

                    var response = await authRequest.GetResponseAsync();
                    string result = null;

                    using (Stream stream = response.GetResponseStream())
                    {
                        StreamReader sr = new StreamReader(stream);
                        result = sr.ReadToEnd();
                        //sr.Close();
                    }

                    if (null != result)
                    {
                        System.Diagnostics.Debug.WriteLine(result.ToString());
                        var data = ParseData(result, out recordCount, out recordsPerPage);
                        entries.AddRange(data);
                        pages = (recordCount + recordsPerPage - 1) / recordsPerPage;
                    }
                } while (pageCount < pages);
                return entries;
            }
            catch (Exception EX_GetData)
            {
                //MainViewModel.WriteToLogs("allLogs", "error", $"EX_GetData(since={since},until={until}): {EX_GetData}");
                return null;
            }
        }

        static private List<TogglTimeEntry> ParseData(string RawData, out int RecordCount, out int RecordsPerPage)
        {
            try
            {
                var n = JObject.Parse(RawData);
                RecordCount = (int)n["total_count"];
                RecordsPerPage = (int)n["per_page"];
                var d = (JArray)n["data"];
                var item = d[0]["user"];
                var data = (from r in d
                            select new TogglTimeEntry
                            {
                                Id = (int)r["id"],
                                User = (string)r["user"],
                                Project = (string)r["project"],
                                Description = (string)r["description"],
                                Start = DateTime.Parse((string)r["start"]),
                                End = DateTime.Parse((string)r["end"]),
                                Duration = (int)r["dur"],
                                Task = (string)r["task"],
                                TaskID = (r["tid"] == null) ? Convert.ToInt32(r["tid"]) : 0,
                                Client = (string)r["client"],
                                Updated = DateTime.Parse((string)r["updated"])
                            }).ToList();
                //MainViewModel.WriteToLogs("allLogs", "info", $"Toggl data parsed: {data.ToString()}");
                return data;
            }
            catch (Exception EX_ParseData)
            {
                //MainViewModel.WriteToLogs("allLogs", "error", $"EX_ParseData(RawData={RawData}): {EX_ParseData}");
                RecordCount = -1;
                RecordsPerPage = -1;
                return null;
            }
        }

        static internal TogglProjectSummary SummarizeProjectEntries(IEnumerable<TogglTimeEntry> entries)
        {
            var user = "";
            var task = "";
            var togglProjectSummary = new TogglProjectSummary();

            togglProjectSummary.Name = entries.FirstOrDefault().Project;

            var userGroups = entries.GroupBy(x => x.User);
            foreach (var userGroup in userGroups)
            {
                var dateGroups = userGroup.GroupBy(x => x.Start.Date);
                foreach (var dateGroup in dateGroups)
                {
                    var taskGroups = dateGroup.GroupBy(x => x.Task);
                    foreach (var taskGroup in taskGroups)
                    {
                        double totalMilliseconds = 0;

                        task = taskGroup.FirstOrDefault().Task ?? "Task Null";
                        user = userGroup.FirstOrDefault().User;

                        if (!togglProjectSummary.TaskSummaries.ContainsKey(task)) togglProjectSummary.TaskSummaries.Add(task, new TogglTaskSummary());

                        if (!togglProjectSummary.TaskSummaries[task].UserTaskSummaries.ContainsKey(user)) togglProjectSummary.TaskSummaries[task].UserTaskSummaries.Add(user, 0);

                        foreach (var e in taskGroup)
                        {
                            totalMilliseconds += e.Duration;
                        }

                        if (totalMilliseconds <= 60000) continue;

                        togglProjectSummary.TaskSummaries[task].UserTaskSummaries[user] += RoundToQuarterHour(totalMilliseconds);
                        togglProjectSummary.TaskSummaries[task].TotalTaskHours += RoundToQuarterHour(totalMilliseconds);
                    }
                }
            }
            return togglProjectSummary;
        }

        static private double RoundToQuarterHour(double milliseconds)
        {
            double hours = milliseconds / 1000.0 / 60.0;
            double remainder = hours % 15.0;
            if (remainder < 7.5)
            {
                hours -= remainder;
            }
            else
            {
                hours += 15.0 - remainder;
            }

            double decimalHours = hours / 60.0;
            if (decimalHours < .25) decimalHours = .25;

            return decimalHours;
        }
    }
}
