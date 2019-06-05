using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace timrlink.net.Core.Service
{
    public interface ITaskService
    {
        Task<IList<API.Task>> GetTaskHierarchy();

        IList<API.Task> FlattenTasks(IEnumerable<API.Task> tasks);
        
        Task<IDictionary<string, API.Task>> CreateExternalIdDictionary(IEnumerable<API.Task> tasks, Func<API.Task, string> externalIdLookup = null);

        Task AddTask(API.Task task);

        Task UpdateTask(API.Task task);

        Task SynchronizeTasksByExternalId(IDictionary<string, API.Task> existingTasks, IList<API.Task> remoteTasks, bool updateTasks = false, bool disableMissingTasks = false, IEqualityComparer<API.Task> equalityComparer = null);
    }
}
