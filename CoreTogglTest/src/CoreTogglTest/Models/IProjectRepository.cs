using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreTogglTest.Models
{
    interface IProjectRepository
    {
        void Add(TogglProject item);
        IEnumerable<TogglProject> GetAll();
        TogglProject Find(string key);
        TogglProject Remove(string key);
        void Update(TogglProject item);
    }
}
