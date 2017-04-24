using System.Collections.Generic;
using timrlink.net.Core.API;

namespace timrlink.net.Core.Service
{
    public interface IProjectTimeService
    {
        void SaveProjectTime(ProjectTime projectTime);

        void SaveProjectTimes(IEnumerable<ProjectTime> projectTimes);
    }
}
