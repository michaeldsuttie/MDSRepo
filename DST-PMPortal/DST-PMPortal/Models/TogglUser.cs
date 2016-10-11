using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DST_PMPortal.Models
{
    public class TogglUser
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public bool IsSelected { get; set; }

    }
}