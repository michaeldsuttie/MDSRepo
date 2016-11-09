using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PMPortal.ServerApp.Services;

namespace PMPortal.ServerApp.Models
{
    internal class Repository:IRepository
    {
        public IEnumerable<TogglProject> TogglProjects { get; set; }
        public TogglProjectBillingSummary TogglProjectBillingSummary { get; set; }
        //public IEnumerable<TogglUser> TogglUsers { get; set; }
        public IEnumerable<TogglTimeEntry> TogglProjectTimeEntries { get; set; }

        public void Add(TogglProject item)
        {
            throw new NotImplementedException();
        }

        public TogglProject Find(string key)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<TogglProject>> GetAll(DateTime Since, DateTime Until)
        {
            TogglProjects = await TogglDataService.GetProjects();
            //foreach (var p in TogglProjects)
            //{
            //    p.Entries = await TogglDataService.GetData(Since, Until);
            //}
            return TogglProjects;
        }

        public async Task<IEnumerable<TogglProject>> GetProjects()
        {
            //if (TogglProjects != null) return TogglProjects;
            return TogglProjects = await TogglDataService.GetProjects();
        }
        public async Task<TogglProjectBillingSummary> GetProjectBillingSummary(string ProjectNumber, DateTime Since, DateTime Until)
        {
            //if (TogglProjects != null) return TogglProjects;
            //return TogglProjects = await TogglDataService.GetProjects();

            if (TogglProjects == null) TogglProjects = await TogglDataService.GetProjects();
            var TogglTimeEntries = await TogglDataService.GetData(Since, Until);
            var filteredTogglTimeEntries = TogglTimeEntries.Where(x => x.Project != null);
            TogglProjectTimeEntries = filteredTogglTimeEntries.Where(x => x.Project.Contains(ProjectNumber));

            var ProjectSummary = TogglDataService.SummarizeProjectEntries(TogglProjectTimeEntries);
            ProjectSummary.BillingPeriod = $"{Since}_{Until}";
            return ProjectSummary;
        }

        public TogglProject Remove(string key)
        {
            throw new NotImplementedException();
        }

        public void Update(TogglProject item)
        {
            throw new NotImplementedException();
        }

        public void Refresh()
        {

        }
    }
}
