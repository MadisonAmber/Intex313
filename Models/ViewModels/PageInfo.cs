using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Intex313.Models.ViewModels
{
    public class PageInfo
    {
        public int TotalNumItems { get; set; }
        public int ItemsPerPage { get; set; }
        public int CurrentPage { get; set; }
        public int NumButtons { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalNumItems / ItemsPerPage);
    }
}
