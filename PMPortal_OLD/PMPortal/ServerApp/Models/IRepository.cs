using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMPortal.ServerApp.Models
{
    interface IRepository
    {
        Task<IEnumerable<TogglProject>> GetAll(DateTime Since, DateTime Until);
        Task<IEnumerable<TogglProject>> GetProjects();
        Task<TogglProjectBillingSummary> GetProjectBillingSummary(string ProjectNumber, DateTime Since, DateTime Until);



        void Add(TogglProject item);
        TogglProject Find(string key);
        TogglProject Remove(string key);
        void Update(TogglProject item);
        void Refresh();
    }
}
