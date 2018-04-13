using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using timrlink.net.Core.API;

namespace timrlink.net.Core.Service
{
    internal class WorkTimeService : IWorkTimeService
    {
        private readonly ILoggerFactory loggerFactory;
        private readonly ILogger<WorkTimeService> logger;
        private readonly TimrSync timrSync;

        public WorkTimeService(ILoggerFactory loggerFactory, TimrSync timrSync)
        {
            this.loggerFactory = loggerFactory;
            this.timrSync = timrSync;

            this.logger = loggerFactory.CreateLogger<WorkTimeService>();
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
                logger.LogError(new EventId(), e, $"Failed saving WorkTime(ExternalUserId={workTime.ExternalUserId}, ExternalWorkItemId={workTime.ExternalWorkItemId}, Description={workTime.Description}, Start={workTime.StartTime}, End={workTime.EndTime}");
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
