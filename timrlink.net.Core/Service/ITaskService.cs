using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace timrlink.net.Core.Service
{
    public interface ITaskService
    {
        Task<IDictionary<string, API.Task>> GetExistingTasksAsync(Func<API.Task, string> externalIdLookup = null);

        Task AddTask(API.Task task);

        Task UpdateTask(API.Task task);

        Task SynchronizeTasks(IDictionary<string, API.Task> existingTasks, IList<API.Task> remoteTasks, bool updateTasks = false, bool disableMissingTasks = false, IEqualityComparer<API.Task> equalityComparer = null);
    }
}
