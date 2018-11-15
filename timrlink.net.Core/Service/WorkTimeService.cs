using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
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

        public Task<IList<WorkTime>> GetWorkTimesAsync(DateTime? start = null, DateTime? end = null, List<WorkTimeStatus> statuses = null, string externalUserId = null, string externalWorkItemId = null)
        {
            return Task.Run(() => GetWorkTimes(start: start, end: end, statuses: statuses, externalUserId: externalUserId, externalWorkItemId: externalWorkItemId));
        }

        public IList<WorkTime> GetWorkTimes(DateTime? start = null, DateTime? end = null, List<WorkTimeStatus> statuses = null, string externalUserId = null, string externalWorkItemId = null)
        {
            var workTimes = timrSync.GetWorkTimes(new GetWorkTimesRequest(new WorkTimeQuery
            {
                Start = start,
                End = end,
                Statuses = statuses,
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
                logger.LogInformation($"Saving WorkTime(ExternalUserId={workTime.ExternalUserId}, ExternalWorkItemId={workTime.ExternalWorkItemId}, Description={workTime.Description}, Start={workTime.StartTime}, End={workTime.EndTime}, Status={workTime.Status})");
                timrSync.SaveWorkTime(new SaveWorkTimeRequest(workTime));
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Failed saving WorkTime(ExternalUserId={workTime.ExternalUserId}, ExternalWorkItemId={workTime.ExternalWorkItemId}, Description={workTime.Description}, Start={workTime.StartTime}, End={workTime.EndTime}, Status={workTime.Status})");
            }
        }

        public void SaveWorkTimes(IEnumerable<WorkTime> workTimes)
        {
            foreach (var workTime in workTimes)
            {
                SaveWorkTime(workTime);
            }
        }

        public void SetWorkTimeStatus(WorkTime workTime, WorkTimeStatus workTimeStatus)
        {
            SetWorkTimeStatus(new List<long> { workTime.Id }, workTimeStatus);
        }

        public void SetWorkTimeStatus(IList<WorkTime> workTimes, WorkTimeStatus workTimeStatus)
        {
            SetWorkTimeStatus(workTimes.Select(w => w.Id).ToList(), workTimeStatus);
        }

        private void SetWorkTimeStatus(List<long> ids, WorkTimeStatus workTimeStatus)
        {
            try
            {
                timrSync.SetWorkTimesStatus(new SetWorkTimesStatusRequest(new WorkTimesStatusRequestType
                {
                    Ids = ids,
                    Status = workTimeStatus
                }));
            }
            catch (ProtocolException e) when (e.Message == "The one-way operation returned a non-null message with Action=''.")
            {
                // thanks to the wrong generated wsdl (OneWay, but it isn't) whe have to catch this here 😂
            }
        }
    }
}
