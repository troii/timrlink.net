using System.Collections.Generic;
using timrlink.net.Core.API;

namespace timrlink.net.Core.Service
{
    public interface IWorkTimeService
    {
        void SaveWorkTime(WorkTime WorkTime);

        void SaveWorkTimes(IEnumerable<WorkTime> WorkTimes);
    }
}
