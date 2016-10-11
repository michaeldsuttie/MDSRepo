using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;

using DST_PMPortal.Models;
using DST_PMPortal.Controllers;

namespace DST_PMPortal.Services
{
    static public class TogglDataService
    {
        static internal List<TogglTimeEntry> GetData(DateTime since, DateTime until)
        {
            try
            {
                string txtSince = since.ToString("yyyy-MM-dd");
                string txtUntil = until.ToString("yyyy-MM-dd");
                string url = "https://toggl.com/reports/api/v2/details?workspace_id=669485&since=" + txtSince + "&until=" + txtUntil + "&user_agent=api_test&rounding=on";

                string ApiToken = "a2e7f93070afe8475626141477fec42b";
                string userpass = ApiToken + ":api_token";
                string userpassB64 = Convert.ToBase64String(Encoding.Default.GetBytes(userpass.Trim()));
                string authHeader = "Basic " + userpassB64;

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
                    authRequest.Headers.Add("Authorization", authHeader);
                    authRequest.Method = "GET";
                    authRequest.ContentType = "application/json";
                    authRequest.Timeout = 120000;

                    var response = (HttpWebResponse)authRequest.GetResponse();
                    string result = null;

                    using (Stream stream = response.GetResponseStream())
                    {
                        StreamReader sr = new StreamReader(stream);
                        result = sr.ReadToEnd();
                        sr.Close();
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
                AppBase.WriteToLog(AppBase.DebugLog, "error", $"EX_GetData(since={since},until={until}): {EX_GetData}");
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
                AppBase.WriteToLog(AppBase.DebugLog, "info", $"Toggl data parsed: {data.ToString()}");
                return data;
            }
            catch (Exception EX_ParseData)
            {
                AppBase.WriteToLog(AppBase.DebugLog, "error", $"EX_ParseData(RawData={RawData}): {EX_ParseData}");
                RecordCount = -1;
                RecordsPerPage = -1;
                return null;
            }
        }

        static internal List<TogglUser> GetUsers()
        {
            try
            {
                string url = "https://www.toggl.com/api/v8/workspaces/669485/users";
                string ApiToken = "a2e7f93070afe8475626141477fec42b";
                string userpass = $"{ApiToken}:api_token";
                string userpassB64 = Convert.ToBase64String(Encoding.Default.GetBytes(userpass.Trim()));
                string authHeader = $"Basic {userpassB64}";

                HttpWebRequest authRequest = (HttpWebRequest)WebRequest.Create(url);
                authRequest.Headers.Add("Authorization", authHeader);
                authRequest.Method = "GET";
                authRequest.ContentType = "application/json";

                var response = (HttpWebResponse)authRequest.GetResponse();
                string result = null;
                using (Stream stream = response.GetResponseStream())
                {
                    StreamReader sr = new StreamReader(stream);
                    result = sr.ReadToEnd();
                    sr.Close();
                }

                if (null != result) return ParseUserData(result);
                return null;

            }
            catch (Exception EX_GetUsers)
            {
                AppBase.WriteToLog(AppBase.DebugLog, "error", $"EX_GetUsers(): {EX_GetUsers}");
                return null;
            }
        }

        static internal async Task<List<Project>> GetProjects()
        {
            try
            {
                string url = "https://toggl.com/api/v8/workspaces/669485/projects";

                string apiToken = "a2e7f93070afe8475626141477fec42b";
                string userPass = $"{apiToken}:api_token";
                string userpassB64 = Convert.ToBase64String(Encoding.Default.GetBytes(userPass.Trim()));
                string authHeader = $"Basic {userpassB64}";

                HttpWebRequest authRequest = (HttpWebRequest)WebRequest.Create(url);
                authRequest.Headers.Add("Authorization", authHeader);
                authRequest.Method = "GET";
                authRequest.ContentType = "application/json";
                authRequest.Timeout = 120000;

                var response = (HttpWebResponse)await authRequest.GetResponseAsync();
                string result;
                using (Stream stream = response.GetResponseStream())
                {
                    StreamReader sr = new StreamReader(stream);
                    result = await sr.ReadToEndAsync();
                    sr.Close();
                }

                if (null != result)
                {
                    Debug.WriteLine(result.ToString());
                    return ParseProjectData(result).ToList();
                }
                return null;

            }
            catch (Exception EX_GetProjects)
            {
                AppBase.WriteToLog(AppBase.DebugLog, "error", $"EX_GetProjects(): {EX_GetProjects}");
                return null;
            }
        }

        static private List<Project> ParseProjectData(string rawData)
        {
            try
            {
                var d = JArray.Parse(rawData);
                var data = (from r in d
                            select new Project
                            {
                                ProjectId = (int)r["id"],
                                Name = (string)r["name"]
                            }).OrderBy(x => x.Name).ToList();
                return data;
            }
            catch (Exception EX_ParseProjectData)
            {
                AppBase.WriteToLog(AppBase.DebugLog, "error", $"EX_ParseProjectData(rawData={rawData}): {EX_ParseProjectData}");
                return null;
            }
        }

        static internal List<Task> GetProjectTasks(int projectId)
        {
        Retry:
            try
            {
                string url = "https://toggl.com/api/v8/projects/" + projectId + "/tasks";
                string apiToken = "a2e7f93070afe8475626141477fec42b";
                string userPass = $"{apiToken}:api_token";
                string userpassB64 = Convert.ToBase64String(Encoding.Default.GetBytes(userPass.Trim()));
                string authHeader = $"Basic {userpassB64}";

                HttpWebRequest authRequest = (HttpWebRequest)WebRequest.Create(url);
                authRequest.Headers.Add("Authorization", authHeader);
                authRequest.Method = "GET";
                authRequest.ContentType = "application/json";
                authRequest.Timeout = 120000;

                var response = (HttpWebResponse)authRequest.GetResponse();
                string result = "null";
                using (Stream stream = response.GetResponseStream())
                {
                    StreamReader sr = new StreamReader(stream);
                    result = sr.ReadToEnd();
                    sr.Close();
                }
                //if (result != "null") return ParseProjectTaskData(result);
                return null;
            }
            catch (Exception EX_GetProjectTasks)
            {
                if (EX_GetProjectTasks.ToString().Contains("(429)"))
                {
                    AppBase.WriteToLog(AppBase.DebugLog, "info", "Rate Limit Exceeded(1 request / s, per IP per API token).Waiting 1s.");


                    Thread.Sleep(1000);
                    goto Retry;
                }
                AppBase.WriteToLog(AppBase.DebugLog, "error", $"EX_GetProjectTasks(projectId={projectId}): {EX_GetProjectTasks}");
                return null;
            }
        }

        //static private List<Task> ParseProjectTaskData(string rawData)
        //{
        //    try
        //    {
        //        var d = JArray.Parse(rawData);

        //        var data = (from r in d
        //                    select new Task
        //                    {
        //                        TaskId = (int)r["id"],
        //                        Name = (string)r["name"]
        //                    }).ToList();
        //        return data;
        //    }
        //    catch (Exception EX_ParseProjectTaskData)
        //    {
        //        AppBase.WriteToLog(AppBase.DebugLog, "error", $"EX_ParseProjectTaskData(rawData={rawData}): {EX_ParseProjectTaskData}");
        //        return null;
        //    }
        //}

        static private List<TogglUser> ParseUserData(string rawData)
        {
            try
            {
                var d = JArray.Parse(rawData);
                var data = (from r in d
                            select new TogglUser
                            {
                                UserId = (int)r["id"],
                                Name = (string)r["fullname"]
                            }).ToList();
                return data;
            }
            catch (Exception EX_ParseUserData)
            {
                AppBase.WriteToLog(AppBase.DebugLog, "error", $"EX_ParseUserData(rawData={rawData}): {EX_ParseUserData}");
                return null;
            }
        }
    }
}
