using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using timrlink.net.Core.API;

namespace timrlink.net.Core.Service
{
    internal class ProjectTimeService : IProjectTimeService
    {
        private readonly ILoggerFactory loggerFactory;
        private readonly ILogger<ProjectTimeService> logger;
        private readonly TimrSync timrSync;

        public ProjectTimeService(ILoggerFactory loggerFactory, TimrSync timrSync)
        {
            this.loggerFactory = loggerFactory;
            this.timrSync = timrSync;

            this.logger = loggerFactory.CreateLogger<ProjectTimeService>();
        }

        public void SaveProjectTime(ProjectTime projectTime)
        {
            try
            {
                logger.LogInformation($"Saving ProjectTime(ExternalUserId={projectTime.ExternalUserId}, ExternalTaskId={projectTime.ExternalTaskId}, Description={projectTime.Description}, Start={projectTime.StartTime}, End={projectTime.EndTime}");
                timrSync.SaveProjectTime(new SaveProjectTimeRequest(projectTime));
            }
            catch (Exception e)
            {
                logger.LogError(new EventId(), e, $"Failed saving ProjectTime(ExternalUserId={projectTime.ExternalUserId}, ExternalTaskId={projectTime.ExternalTaskId}, Description={projectTime.Description}, Start={projectTime.StartTime}, End={projectTime.EndTime}");
            }
        }

        public void SaveProjectTimes(IEnumerable<ProjectTime> projectTimes)
        {
            foreach (var projectTime in projectTimes)
            {
                SaveProjectTime(projectTime);
            }
        }
    }
}
