using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;

namespace timrlink.net.Core.Service
{
    internal class WorkTimeService : IWorkTimeService
    {
        private readonly ILogger<WorkTimeService> logger;
        private readonly API.TimrSync timrSync;

        public WorkTimeService(ILogger<WorkTimeService> logger, API.TimrSync timrSync)
        {
            this.logger = logger;
            this.timrSync = timrSync;
        }

        public async Task<IList<API.WorkTime>> GetWorkTimes(DateTime? start = null, DateTime? end = null, List<API.WorkTimeStatus> statuses = null, string externalUserId = null, string externalWorkItemId = null)
        {
            var getWorkTimesResponse = await timrSync.GetWorkTimesAsync(new API.GetWorkTimesRequest(new API.WorkTimeQuery
            {
                start = start,
                startSpecified = start != null,
                end = end,
                endSpecified = end != null,
                statuses = statuses?.ToArray(),
                externalUserId = externalUserId,
                externalWorkItemId = externalWorkItemId
            })).ConfigureAwait(false);
            
            var workTimes = getWorkTimesResponse.GetWorkTimesResponse1;
            logger.LogDebug($"Total workTimes count: {workTimes.Length}");

            return workTimes;
        }

        public async Task SaveWorkTime(API.WorkTime workTime)
        {
            try
            {
                logger.LogInformation($"Saving WorkTime(ExternalUserId={workTime.externalUserId}, ExternalWorkItemId={workTime.externalWorkItemId}, Description={workTime.description}, Start={workTime.startTime}, End={workTime.endTime}, Status={workTime.status})");
                await timrSync.SaveWorkTimeAsync(new API.SaveWorkTimeRequest(workTime)).ConfigureAwait(false);
            }
            catch (FaultException e)
            {
                logger.LogError($"Failed saving WorkTime(ExternalUserId={workTime.externalUserId}, ExternalWorkItemId={workTime.externalWorkItemId}, Description={workTime.description}, Start={workTime.startTime}, End={workTime.endTime}, Status={workTime.status}): {e.Message}");
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Failed saving WorkTime(ExternalUserId={workTime.externalUserId}, ExternalWorkItemId={workTime.externalWorkItemId}, Description={workTime.description}, Start={workTime.startTime}, End={workTime.endTime}, Status={workTime.status})");
            }
        }

        public async Task SaveWorkTimes(IEnumerable<API.WorkTime> workTimes)
        {
            await Task.WhenAll(workTimes.Select(SaveWorkTime)).ConfigureAwait(false);
        }

        public async Task SetWorkTimeStatus(API.WorkTime workTime, API.WorkTimeStatus workTimeStatus)
        {
            await SetWorkTimeStatus(new List<long> { workTime.id }, workTimeStatus).ConfigureAwait(false);
        }

        public async Task SetWorkTimeStatus(IList<API.WorkTime> workTimes, API.WorkTimeStatus workTimeStatus)
        {
            await SetWorkTimeStatus(workTimes.Select(w => w.id).ToList(), workTimeStatus).ConfigureAwait(false);
        }

        private async Task SetWorkTimeStatus(List<long> ids, API.WorkTimeStatus workTimeStatus)
        {
            await timrSync.SetWorkTimesStatusAsync(new API.SetWorkTimesStatusRequest(new API.WorkTimesStatusRequestType
            {
                ids = ids.ToArray(),
                status = workTimeStatus
            })).ConfigureAwait(false);
        }
    }
}
