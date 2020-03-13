using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
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

        public async Task<IList<ProjectTime>> GetProjectTimes(DateTime? start, DateTime? end, DateTime? lastModified, IEnumerable<ProjectTimeStatus> statuses, string externalUserId, string externalTaskId)
        {
            var projectTimesResponse = await timrSync.GetProjectTimesAsync(new GetProjectTimesRequest(new ProjectTimeQuery
            {
                start = start,
                startSpecified = start != null,
                end = end,
                endSpecified = end != null,
                lastModified = lastModified,
                lastModifiedSpecified = lastModified != null,
                statuses = statuses?.ToArray(),
                externalUserId = externalUserId,
                externalTaskId = externalTaskId,
            })).ConfigureAwait(false);

            var projectTimes = projectTimesResponse.GetProjectTimesResponse1;
            logger.LogDebug($"Total projectTimes count: {projectTimes.Length}");

            return projectTimes;
        }

        public async Task SaveProjectTime(ProjectTime projectTime)
        {
            try
            {
                logger.LogInformation($"Saving ProjectTime(ExternalUserId={projectTime.externalUserId}, ExternalTaskId={projectTime.externalTaskId}, Description={projectTime.description}, Start={projectTime.startTime}, End={projectTime.endTime})");
                await timrSync.SaveProjectTimeAsync(new SaveProjectTimeRequest(projectTime)).ConfigureAwait(false);
            }
            catch (FaultException e)
            {
                logger.LogError($"Failed saving ProjectTime(ExternalUserId={projectTime.externalUserId}, ExternalTaskId={projectTime.externalTaskId}, Description={projectTime.description}, Start={projectTime.startTime}, End={projectTime.endTime}): {e.Message}");
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Failed saving ProjectTime(ExternalUserId={projectTime.externalUserId}, ExternalTaskId={projectTime.externalTaskId}, Description={projectTime.description}, Start={projectTime.startTime}, End={projectTime.endTime})");
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
