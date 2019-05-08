using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace timrlink.net.Core.Service
{
    public interface IWorkTimeService
    {
        Task<IList<API.WorkItem>> GetWorkItemsAsync();

        Task<IList<API.WorkTime>> GetWorkTimesAsync(DateTime? start = null, DateTime? end = null, List<API.WorkTimeStatus> statuses = null, string externalUserId = null, string externalWorkItemId = null);

        Task SaveWorkTime(API.WorkTime workTime);

        Task SaveWorkTimes(IEnumerable<API.WorkTime> workTimes);

        Task SetWorkTimeStatus(API.WorkTime workTime, API.WorkTimeStatus workTimeStatus);

        Task SetWorkTimeStatus(IList<API.WorkTime> workTimes, API.WorkTimeStatus workTimeStatus);
    }
}
