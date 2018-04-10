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

        public void SaveWorkTime(WorkTime WorkTime)
        {
            try
            {
                logger.LogInformation($"Saving WorkTime(ExternalUserId={WorkTime.ExternalUserId}, ExternalWorkItemId={WorkTime.ExternalWorkItemId}, Description={WorkTime.Description}, Start={WorkTime.StartTime}, End={WorkTime.EndTime}");
                timrSync.SaveWorkTime(new SaveWorkTimeRequest(WorkTime));
            }
            catch (Exception e)
            {
                logger.LogError(new EventId(), e, $"Failed saving WorkTime(ExternalUserId={WorkTime.ExternalUserId}, ExternalWorkItemId={WorkTime.ExternalWorkItemId}, Description={WorkTime.Description}, Start={WorkTime.StartTime}, End={WorkTime.EndTime}");
            }
        }

        public void SaveWorkTimes(IEnumerable<WorkTime> WorkTimes)
        {
            foreach (var WorkTime in WorkTimes)
            {
                SaveWorkTime(WorkTime);
            }
        }
    }
}
