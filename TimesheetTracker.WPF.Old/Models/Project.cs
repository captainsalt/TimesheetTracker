using System;
using System.Collections.Generic;
using System.Text;

namespace TimesheetTracker.WPF.Models
{
    class Project(string projectName, List<int> hours)
    {
        public string ProjectName { get; } = projectName;
        public List<int> Hours { get; set; } = hours;
    }
}
