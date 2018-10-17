using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using timrlink.net.Core.API;
using Task = System.Threading.Tasks.Task;

namespace timrlink.net.Core.Service
{
    internal class WorkTimeService : IWorkTimeService
    {
        private readonly ILogger<WorkTimeService> logger;
        private readonly TimrSync timrSync;

        public WorkTimeService(ILogger<WorkTimeService> logger, TimrSync timrSync)
        {
            this.logger = logger;
            this.timrSync = timrSync;
        }

        public Task<IList<WorkTime>> GetWorkTimesAsync(DateTime? start = null, DateTime? end = null, string externalUserId = null, string externalWorkItemId = null)
        {
            return Task.Run(() => GetWorkTimes(start, end, externalUserId, externalWorkItemId));
        }

        public IList<WorkTime> GetWorkTimes(DateTime? start = null, DateTime? end = null, string externalUserId = null, string externalWorkItemId = null)
        {
            var workTimes = timrSync.GetWorkTimes(new GetWorkTimesRequest(new WorkTimeQuery()
            {
                Start = start,
                End = end,
                ExternalUserId = externalUserId,
                ExternalWorkItemId = externalWorkItemId
            })).WorkTimes;

            logger.LogDebug($"Total workTimes count: {workTimes.Count}");

            return workTimes;
        }

        public void SaveWorkTime(WorkTime workTime)
        {
            try
            {
                logger.LogInformation($"Saving WorkTime(ExternalUserId={workTime.ExternalUserId}, ExternalWorkItemId={workTime.ExternalWorkItemId}, Description={workTime.Description}, Start={workTime.StartTime}, End={workTime.EndTime}");
                timrSync.SaveWorkTime(new SaveWorkTimeRequest(workTime));
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Failed saving WorkTime(ExternalUserId={workTime.ExternalUserId}, ExternalWorkItemId={workTime.ExternalWorkItemId}, Description={workTime.Description}, Start={workTime.StartTime}, End={workTime.EndTime}");
            }
        }

        public void SaveWorkTimes(IEnumerable<WorkTime> workTimes)
        {
            foreach (var workTime in workTimes)
            {
                SaveWorkTime(workTime);
            }
        }
    }
}
