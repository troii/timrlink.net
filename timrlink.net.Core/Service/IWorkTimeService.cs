using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using timrlink.net.Core.API;

namespace timrlink.net.Core.Service
{
    public interface IWorkTimeService
    {
        Task<IList<WorkTime>> GetWorkTimesAsync(DateTime? start = null, DateTime? end = null, List<WorkTimeStatus> statuses = null, string externalUserId = null, string externalWorkItemId = null);

        IList<WorkTime> GetWorkTimes(DateTime? start = null, DateTime? end = null, List<WorkTimeStatus> statuses = null, string externalUserId = null, string externalWorkItemId = null);

        void SaveWorkTime(WorkTime workTime);

        void SaveWorkTimes(IEnumerable<WorkTime> workTimes);

        void SetWorkTimeStatus(WorkTime workTime, WorkTimeStatus workTimeStatus);

        void SetWorkTimeStatus(IList<WorkTime> workTimes, WorkTimeStatus workTimeStatus);
    }
}
