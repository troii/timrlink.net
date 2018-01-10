using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace timrlink.net.Core.Service
{
    public interface ITaskService
    {
        Task<IDictionary<string, API.Task>> GetExistingTasksAsync(Func<API.Task, string> externalIdLookup = null);

        IDictionary<string, API.Task> GetExistingTasks(Func<API.Task, string> externalIdLookup = null);
        
        void AddTask(API.Task task);
        
        void UpdateTask(API.Task task);

        void SynchronizeTasks(IDictionary<string, API.Task> existingTasks, IList<API.Task> remoteTasks, bool updateTasks = false, IEqualityComparer<API.Task> equalityComparer = null);
    }
}
