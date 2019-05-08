using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using timrlink.net.Core.API;
using Task = System.Threading.Tasks.Task;

namespace timrlink.net.Core.Service
{
    internal class ProjectTimeService : IProjectTimeService
    {
        private readonly ILogger<ProjectTimeService> logger;
        private readonly TimrSync timrSync;

        public ProjectTimeService(ILogger<ProjectTimeService> logger, TimrSync timrSync)
        {
            this.logger = logger;
            this.timrSync = timrSync;
        }

        public async Task SaveProjectTime(ProjectTime projectTime)
        {
            try
            {
                logger.LogInformation($"Saving ProjectTime(ExternalUserId={projectTime.externalUserId}, ExternalTaskId={projectTime.externalTaskId}, Description={projectTime.description}, Start={projectTime.startTime}, End={projectTime.endTime}");
                await timrSync.SaveProjectTimeAsync(new SaveProjectTimeRequest(projectTime)).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Failed saving ProjectTime(ExternalUserId={projectTime.externalUserId}, ExternalTaskId={projectTime.externalTaskId}, Description={projectTime.description}, Start={projectTime.startTime}, End={projectTime.endTime}");
            }
        }

        public async Task SaveProjectTimes(IEnumerable<ProjectTime> projectTimes)
        {
            foreach (var projectTime in projectTimes)
            {
                await SaveProjectTime(projectTime).ConfigureAwait(false);
            }
        }
    }
}
