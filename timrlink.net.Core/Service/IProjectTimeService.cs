using System.Collections.Generic;
using System.Threading.Tasks;

namespace timrlink.net.Core.Service
{
    public interface IProjectTimeService
    {
        Task SaveProjectTime(API.ProjectTime projectTime);

        Task SaveProjectTimes(IEnumerable<API.ProjectTime> projectTimes);

        Task<IList<API.ProjectTime>> GetProjectTimes();
    }
}
