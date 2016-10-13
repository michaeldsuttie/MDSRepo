using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CoreTogglTest.Models;
using CoreTogglTest.Services;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace CoreTogglTest.Controllers
{
    [Route("api/[controller]")]
    public class TogglProjectController : Controller
    {
        internal static readonly Repository _repo = new Repository();


        //public IEnumerable<TogglProject> TogglProjects { get; set; }
        //public IEnumerable<TogglUser> TogglUsers { get; set; }
        //public IEnumerable<TogglTimeEntry> TogglProjectTimeEntries { get; set; }


        // GET: api/TogglProject
        [HttpGet]
        public async Task<IEnumerable<TogglProject>> Get()
        {
            return _repo.TogglProjects = await _repo.GetAll(DateTime.Now.AddDays(-14), DateTime.Now);
        }

        // GET api/TogglProject/16081
        [HttpGet("{_projectNumber}")]
        public async Task<TogglProject> GetProjectDetail(string _projectNumber)
        {
            if (_repo.TogglProjects == null) _repo.TogglProjects = await TogglDataService.GetProjects();
            return _repo.TogglProjects.Where(x => x.Name.Contains(_projectNumber)).FirstOrDefault();
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
            if (_repo.TogglProjects == null) _repo.TogglProjects = await TogglDataService.GetProjects();
            var TogglTimeEntries = await TogglDataService.GetData(DateTime.Parse(_since), DateTime.Parse(_until));
            var filteredTogglTimeEntries = TogglTimeEntries.Where(x => x.Project != null);
            _repo.TogglProjectTimeEntries = filteredTogglTimeEntries.Where(x => x.Project.Contains(_projectNumber));
            return _repo.TogglProjectTimeEntries;
        }

        // GET api/TogglProject/16081/2016-10-07_2016-10-07/summary
        [HttpGet("{_projectNumber}/summary")]
        public async Task<TogglProjectSummary> GetProjectSummary(string _projectNumber, DateTime _since, DateTime _until)
        {

            if (_repo.TogglProjects == null) _repo.TogglProjects = await TogglDataService.GetProjects();
            var TogglTimeEntries = await TogglDataService.GetData(_since, _until);
            var filteredTogglTimeEntries = TogglTimeEntries.Where(x => x.Project != null);
            _repo.TogglProjectTimeEntries = filteredTogglTimeEntries.Where(x => x.Project.Contains(_projectNumber));

            var ProjectSummary = TogglDataService.SummarizeProjectEntries(_repo.TogglProjectTimeEntries);
            ProjectSummary.BillingPeriod = $"{_since}_{_until}";
            return ProjectSummary;
            //return null;
        }
    }
}
