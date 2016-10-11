using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CoreTogglTest.Models;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace CoreTogglTest.Controllers
{
    [Route("api/[controller]")]
    public class TogglProjectController : Controller
    {
        public IEnumerable<TogglProject> TogglProjects { get; set; }
        public IEnumerable<TogglUser> TogglUsers { get; set; }
        public IEnumerable<TogglTimeEntry> TogglProjectTimeEntries { get; set; }


        // GET: api/TogglProject
        [HttpGet]
        public async Task<IEnumerable<TogglProject>> Get()
        {
            TogglProjects = await TogglDataService.GetProjects();
            return TogglProjects;
        }

        // GET api/TogglProject/16081
        [HttpGet("{_projectNumber}")]
        public async Task<TogglProject> GetProjectDetail(string _projectNumber)
        {
            if (TogglProjects == null) TogglProjects = await TogglDataService.GetProjects();
            return TogglProjects.Where(x => x.Name.Contains(_projectNumber)).FirstOrDefault();
        }

        // GET api/TogglProject/16081/Users
        //[HttpGet("{_id}/_users")]
        //public async Task<List<TogglUser>> Get(string _id, string[] _users)
        //{
        //    if (TogglProjects == null) TogglProjects = await TogglDataService.GetProjects();
        //    if (TogglUsers == null) TogglUsers = await TogglDataService.GetUsers();
        //    //var users = TogglUsers.Where(x => x.Name.Contains(_id)).First();

        //    return TogglUsers;
        //}

        // GET api/TogglProject/16081/2016-10-07_2016-10-07
        [HttpGet("{_projectNumber}/{_since}_{_until}")]
        public async Task<IEnumerable<TogglTimeEntry>> GetProjectEntries(string _projectNumber, string _since, string _until)
        {
            if (TogglProjects == null) TogglProjects = await TogglDataService.GetProjects();
            var TogglTimeEntries = await TogglDataService.GetData(DateTime.Parse(_since), DateTime.Parse(_until));
            var filteredTogglTimeEntries = TogglTimeEntries.Where(x => x.Project != null);
            TogglProjectTimeEntries = filteredTogglTimeEntries.Where(x => x.Project.Contains(_projectNumber));
            return TogglProjectTimeEntries;
        }

        // GET api/TogglProject/16081/2016-10-07_2016-10-07/summary
        [HttpGet("{_projectNumber}/{_since}_{_until}/summary")]
        public async Task<TogglProjectSummary> GetProjectSummary(string _projectNumber, string _since, string _until)
        {
            if (TogglProjects == null) TogglProjects = await TogglDataService.GetProjects();
            var TogglTimeEntries = await TogglDataService.GetData(DateTime.Parse(_since), DateTime.Parse(_until));
            var filteredTogglTimeEntries = TogglTimeEntries.Where(x => x.Project != null);
            TogglProjectTimeEntries = filteredTogglTimeEntries.Where(x => x.Project.Contains(_projectNumber));

            var ProjectSummary = SummarizeProjectEntries(TogglProjectTimeEntries);

            return ProjectSummary;
        }

        public TogglProjectSummary SummarizeProjectEntries(IEnumerable<TogglTimeEntry> entries)
        {
            var togglProjectSummary = new TogglProjectSummary();
            togglProjectSummary.Name = entries.FirstOrDefault().Project;

            var TaskGroups = entries.GroupBy(x => x.Task);
            foreach (var TaskGroup in TaskGroups)
            {
                var task = TaskGroup.FirstOrDefault().Task ?? "Task Null";
                if (!togglProjectSummary.TaskSummaries.ContainsKey(task)) togglProjectSummary.TaskSummaries.Add(task, new TogglTaskSummary());

                var userGroups = TaskGroup.GroupBy(x => x.User);
                foreach (var userGroup in userGroups)
                {
                    var user = userGroup.FirstOrDefault().User;
                    if (!togglProjectSummary.TaskSummaries[task].UserTaskSummaries.ContainsKey(user)) togglProjectSummary.TaskSummaries[task].UserTaskSummaries.Add(user, 0);

                    double totalMilliseconds = 0;
                    double totalHours = 0;
                    //string totalDescription = "";
                    //string seperator = "";
                    //string previousDiscription = "";

                    foreach (var e in userGroup)
                    {
                        totalMilliseconds += e.Duration;
                        //if (e.Description != previousDiscription)
                        //{
                        //    totalDescription += seperator + e.Description;
                        //    seperator = "; ";
                        //}
                        //previousDiscription = e.Description;
                    }

                    if (totalMilliseconds <= 60000)
                    {
                        totalMilliseconds = 0;
                    }
                    else
                    {
                        totalHours = RoundToQuarterHour(totalMilliseconds);
                    }
                    togglProjectSummary.TaskSummaries[task].UserTaskSummaries[user] = totalHours;
                }
                foreach(var u in togglProjectSummary.TaskSummaries[task].UserTaskSummaries)
                {
                    togglProjectSummary.TaskSummaries[task].TotalTaskHours += togglProjectSummary.TaskSummaries[task].UserTaskSummaries[u.Key]; 
                }
            }
            return togglProjectSummary;
        }

        private static double RoundToQuarterHour(double milliseconds)
        {
            double value = milliseconds / 1000.0 / 60.0;
            double remainder = value % 15.0;
            if (remainder < 7.5)
            {
                value -= remainder;
            }
            else
            {
                value += 15.0 - remainder;
            }
            value = value / 60.0;
            if (value < .25)
            {
                value = .25;
            }
            return value;
        }
    }
}
