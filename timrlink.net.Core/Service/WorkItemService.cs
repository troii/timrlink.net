using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace timrlink.net.Core.Service
{
    public class WorkItemService : IWorkItemService
    {
        private readonly ILogger<WorkItemService> logger;
        private readonly API.TimrSync timrSync;

        public WorkItemService(ILogger<WorkItemService> logger, API.TimrSync timrSync)
        {
            this.logger = logger;
            this.timrSync = timrSync;
        }
        
        public async Task<IList<API.WorkItem>> GetWorkItems()
        {
            return (await timrSync.GetWorkItemsAsync(new API.GetWorkItemsRequest("")).ConfigureAwait(false)).GetWorkItemsResponse1;
        }
    }
}
