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

        public async Task<IList<ProjectTime>> GetProjectTimes(DateTime? start, DateTime? end, DateTime? lastModified, IEnumerable<ProjectTimeStatus> statuses, string externalUserId, string externalTaskId, bool? includeChildTasks)
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
                includeChildTasks = includeChildTasks,
                includeChildTasksSpecified = includeChildTasks.HasValue
            })).ConfigureAwait(false);

            var projectTimes = projectTimesResponse.GetProjectTimesResponse1;
            logger.LogDebug($"Total projectTimes count: {projectTimes.Length}");

            return projectTimes;
        }

        public async Task<long> SaveProjectTime(ProjectTime projectTime)
        {
            try
            {
                logger.LogInformation($"Saving ProjectTime(ExternalUserId={projectTime.externalUserId}, ExternalTaskId={projectTime.externalTaskId}, Description={projectTime.description}, Start={projectTime.startTime}, End={projectTime.endTime})");
                return (await timrSync.SaveProjectTimeAsync(new SaveProjectTimeRequest(projectTime))
                    .ConfigureAwait(false)).SaveProjectTimeResponse1;
            }
            catch (FaultException e)
            {
                logger.LogError($"Failed saving ProjectTime(ExternalUserId={projectTime.externalUserId}, ExternalTaskId={projectTime.externalTaskId}, Description={projectTime.description}, Start={projectTime.startTime}, End={projectTime.endTime}): {e.Message}");
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Failed saving ProjectTime(ExternalUserId={projectTime.externalUserId}, ExternalTaskId={projectTime.externalTaskId}, Description={projectTime.description}, Start={projectTime.startTime}, End={projectTime.endTime})");
            }
            return -1;
        }

        public async Task SaveProjectTimes(IEnumerable<ProjectTime> projectTimes)
        {
            foreach (var projectTime in projectTimes)
            {
                await SaveProjectTime(projectTime).ConfigureAwait(false);
            }
        }

        public async Task<bool> SetProjectTimeStatus(ProjectTime projectTime, ProjectTimeStatus projectTimeStatus)
        {
            return await SetProjectTimesStatus(new List<long> { projectTime.id }, projectTimeStatus).ConfigureAwait(false);
        }

        public async Task<bool> SetProjectTimeStatus(IEnumerable<ProjectTime> projectTimes, ProjectTimeStatus projectTimeStatus)
        {
            return await SetProjectTimesStatus(projectTimes.Select(p => p.id).ToList(), projectTimeStatus).ConfigureAwait(false);
        }

        private async Task<bool> SetProjectTimesStatus(IEnumerable<long> ids, ProjectTimeStatus status)
        {
            var idsArray = ids.ToArray();
            try
            {
                logger.LogInformation($"SetProjectTimesStatus(ids.Count={idsArray.Count()}, Status={status})");
                var statusResponse = (await timrSync.SetProjectTimesStatusAsync(new SetProjectTimesStatusRequest(
                    new ProjectTimesStatusRequestType()
                    {
                        ids = idsArray,
                        status = status
                    })).ConfigureAwait(false)).SetProjectTimesStatusResponse1;
                
                foreach (var id in idsArray)
                {
                    logger.LogInformation($"Successfully changed Status for ProjectTime(id={id}) to Status={status}");
                }
            }
            catch (FaultException e)
            {
                logger.LogError($"Failed SetProjectTimesStatus(ids.Count={idsArray.Count()}, Status={status}): {e.Message}");
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Failed SetProjectTimesStatus(ids.Count={idsArray.Count()}, Status={status})");
            }
            return false;
        }
    }
}
