using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreTogglTest.Services;

namespace CoreTogglTest.Models
{
    internal class Repository:IRepository
    {
        public IEnumerable<TogglProject> TogglProjects { get; set; }
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
