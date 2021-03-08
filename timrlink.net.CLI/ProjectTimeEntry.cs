using System;

namespace timrlink.net.CLI
{
    public class ProjectTimeEntry
    {
        public string User { get; set; }
        public string Task { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public string StartTimeZone { get; set; }
        public string EndTimeZone { get; set; }
        public long duration { get; set; }
        public int  Break { get; set; }
        public string Notes { get; set; }
        public bool Billable { get; set; }
        public bool Changed { get; set; }

        public Core.API.ProjectTime CreateProjectTime()
        {
            var projectTime = new Core.API.ProjectTime()
            {
                startTime = StartDateTime,
                endTime = EndDateTime,
                startTimeZone = StartTimeZone,
                endTimeZone = EndTimeZone,
                duration = duration,
                breakTime = Break,
                description = Notes,
                billable = Billable,
                changed = Changed 
            };
            return projectTime;
        }
    }
}