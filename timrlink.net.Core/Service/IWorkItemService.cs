using System.Collections.Generic;
using System.Threading.Tasks;

namespace timrlink.net.Core.Service
{
    public interface IWorkItemService
    {
        Task<IList<API.WorkItem>> GetWorkItems();
    }
}
