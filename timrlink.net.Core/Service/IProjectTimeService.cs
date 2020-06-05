using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace timrlink.net.Core.Service
{
    public interface IProjectTimeService
    {
        Task<IList<API.ProjectTime>> GetProjectTimes(DateTime? start = null, DateTime? end = null, DateTime? lastModified = null, IEnumerable<API.ProjectTimeStatus> statuses = null, string externalUserId = null, string externalTaskId = null);

        Task<long> SaveProjectTime(API.ProjectTime projectTime);

        Task SaveProjectTimes(IEnumerable<API.ProjectTime> projectTimes);

        Task<bool> SetProjectTimeStatus(API.ProjectTime projectTime, API.ProjectTimeStatus status);
        
        Task<bool> SetProjectTimeStatus(IEnumerable<API.ProjectTime> projectTimes, API.ProjectTimeStatus status);
    }
}
