using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PMPortal.ServerApp.Models;
using PMPortal.ServerApp.Services;

namespace PMPortal.Controllers
{
    [Route("api/[controller]")]
    public class TogglDataController : Controller
    {
        internal static readonly Repository _repo = new Repository();

        [HttpGet("[action]")]
        public async Task<IEnumerable<TogglProject>> GetProjects()
        {
            return _repo.TogglProjects = await _repo.GetAll(DateTime.Now.AddDays(-14), DateTime.Now);
        }

        //// GET api/TogglProject/16081
        //[HttpGet("{_projectNumber}")]
        //public async Task<TogglProject> GetProjectDetail(string _projectNumber)
        //{
        //    if (_repo.TogglProjects == null) _repo.TogglProjects = await TogglDataService.GetProjects();
        //    return _repo.TogglProjects.Where(x => x.Name.Contains(_projectNumber)).FirstOrDefault();
        //}

        //// GET api/TogglProject/16081/Users
        ////[HttpGet("{_id}/_users")]
        ////public async Task<List<TogglUser>> Get(string _id, string[] _users)
        ////{
        ////    if (TogglProjects == null) TogglProjects = await TogglDataService.GetProjects();
        ////    if (TogglUsers == null) TogglUsers = await TogglDataService.GetUsers();
        ////    //var users = TogglUsers.Where(x => x.Name.Contains(_id)).First();

        ////    return TogglUsers;
        ////}

        [HttpGet("[action]")]
        public async Task<IEnumerable<TogglTimeEntry>> GetProjectEntries(string ProjectNumber, DateTime Since, DateTime Until)
        {
            Since = DateTime.Now.AddDays(-2);
            Until = DateTime.Now;
            ProjectNumber = "16081";
            if (_repo.TogglProjects == null) _repo.TogglProjects = await TogglDataService.GetProjects();
            var FilteredTogglProjects = _repo.TogglProjects.Where(x => x.Name != null);
            _repo.TogglProjects = FilteredTogglProjects;

            _repo.TogglProjectTimeEntries = await TogglDataService.GetData(Since, Until);
            var FilteredTogglTimeEntries = _repo.TogglProjectTimeEntries.Where(x => x.Project != null);
            _repo.TogglProjectTimeEntries = FilteredTogglTimeEntries.Where(x => x.Project.Contains(ProjectNumber));
            return _repo.TogglProjectTimeEntries;
        }

        //// GET api/TogglProject/16081/2016-10-07_2016-10-07/summary
        //[HttpGet("{_projectNumber}/summary")]
        //public async Task<TogglProjectSummary> GetProjectSummary(string _projectNumber, DateTime _since, DateTime _until)
        //{

        //    if (_repo.TogglProjects == null) _repo.TogglProjects = await TogglDataService.GetProjects();
        //    var TogglTimeEntries = await TogglDataService.GetData(_since, _until);
        //    var filteredTogglTimeEntries = TogglTimeEntries.Where(x => x.Project != null);
        //    _repo.TogglProjectTimeEntries = filteredTogglTimeEntries.Where(x => x.Project.Contains(_projectNumber));

        //    var ProjectSummary = TogglDataService.SummarizeProjectEntries(_repo.TogglProjectTimeEntries);
        //    ProjectSummary.BillingPeriod = $"{_since}_{_until}";
        //    return ProjectSummary;
        //    //return null;
        //}
    }
}
