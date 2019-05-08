using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace timrlink.net.Core.Service
{
    internal class TaskService : ITaskService
    {
        private readonly ILogger<TaskService> logger;
        private readonly ILoggerFactory loggerFactory;
        private readonly API.TimrSync timrSync;

        public TaskService(ILogger<TaskService> logger, ILoggerFactory loggerFactory, API.TimrSync timrSync)
        {
            this.logger = logger;
            this.loggerFactory = loggerFactory;
            this.timrSync = timrSync;
        }

        public async Task<IDictionary<string, API.Task>> GetExistingTasksAsync(Func<API.Task, string> externalIdLookup = null)
        {
            var existingTasks = (await timrSync.GetTasksAsync(new API.GetTasksRequest1(new API.GetTasksRequest())).ConfigureAwait(false)).GetTasksResponse1;

            IDictionary<string, API.Task> taskDictionary = new Dictionary<string, API.Task>();
            await AddTaskIDs(taskDictionary, existingTasks, null, externalIdLookup).ConfigureAwait(false);
            logger.LogDebug($"Total task count: {existingTasks.Length}");

            return taskDictionary;
        }

        public async Task AddTask(API.Task task)
        {
            logger.LogInformation($"Adding Task(Name={task.name}, ExternalId={task.externalId})");
            await timrSync.AddTaskAsync(new API.AddTaskRequest(task)).ConfigureAwait(false);
        }

        public async Task UpdateTask(API.Task task)
        {
            logger.LogInformation($"Updating Task(Name={task.name}, ExternalId={task.externalId})");
            await timrSync.UpdateTaskAsync(new API.UpdateTaskRequest(task)).ConfigureAwait(false);
        }

        public async Task SynchronizeTasks(IDictionary<string, API.Task> existingTasks, IList<API.Task> remoteTasks, bool updateTasks = false, IEqualityComparer<API.Task> equalityComparer = null)
        {
            if (equalityComparer == null)
            {
                equalityComparer = new DefaultTaskEqualityComparer(loggerFactory);
            }

            foreach (API.Task task in remoteTasks)
            {
                try
                {
                    await AddOrUpdateTask(existingTasks, task, updateTasks, equalityComparer).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    logger.LogError(e, $"Failed synchronizing Task(Name={task.name}, ExternalId={task.externalId})");
                }
            }
        }

        protected async Task AddOrUpdateTask(IDictionary<string, API.Task> existingTaskIDs, API.Task task, bool updateTask, IEqualityComparer<API.Task> equalityComparer)
        {
            logger.LogDebug($"Checking Task(Name={task.name}, ExternalId={task.externalId})");

            if (existingTaskIDs.TryGetValue(task.externalId, out var existingTask))
            {
                if (updateTask && !equalityComparer.Equals(task, existingTask))
                {
                    logger.LogInformation($"Updating Task(Name={task.name}, ExternalId={task.externalId})");
                    await timrSync.UpdateTaskAsync(new API.UpdateTaskRequest(task)).ConfigureAwait(false);
                    existingTaskIDs[task.externalId] = task;
                }
            }
            else
            {
                logger.LogInformation($"Adding Task(Name={task.name}, ExternalId={task.externalId})");
                await timrSync.AddTaskAsync(new API.AddTaskRequest(task)).ConfigureAwait(false);
                existingTaskIDs.Add(task.externalId, task);
                if (task.subtasks != null)
                {
                    foreach (API.Task subtask in task.subtasks)
                    {
                        await timrSync.AddTaskAsync(new API.AddTaskRequest(subtask)).ConfigureAwait(false);
                        existingTaskIDs.Add(subtask.externalId, subtask);
                    }
                }
            }
        }

        private async Task AddTaskIDs(IDictionary<string, API.Task> existingTaskIDs, IEnumerable<API.Task> existingTasks, string parentExternalId, Func<API.Task, string> externalIdLookup)
        {
            foreach (API.Task task in existingTasks)
            {
                task.parentExternalId = parentExternalId;

                if (String.IsNullOrEmpty(task.externalId) && externalIdLookup != null)
                {
                    await UpdateTaskWithoutExternalId(task, externalIdLookup).ConfigureAwait(false);
                }

                if (!String.IsNullOrEmpty(task.externalId))
                {
                    if (existingTaskIDs.ContainsKey(task.externalId))
                    {
                        logger.LogError($"Duplicate ExternalId: Task(Name={task.name}, ExternalId={task.externalId})");
                    }
                    else
                    {
                        existingTaskIDs.Add(task.externalId, task);
                        if (task.subtasks != null)
                        {
                            await AddTaskIDs(existingTaskIDs, task.subtasks, task.externalId, externalIdLookup).ConfigureAwait(false);
                        }
                    }
                }
            }
        }

        private async Task UpdateTaskWithoutExternalId(API.Task task, Func<API.Task, string> externalIdLookup)
        {
            var externalId = externalIdLookup(task);
            if (!String.IsNullOrEmpty(externalId))
            {
                logger.LogInformation($"Updating previous untracked Task(Name={task.name}) with ExternalId: {externalId}");
                try
                {
                    await timrSync.SetTaskExternalIdAsync(new API.SetTaskExternalIdRequest1(new API.SetTaskExternalIdRequest
                    {
                        name = task.name,
                        newExternalTaskId = externalId,
                        parentExternalId = task.parentExternalId
                    }
                    )).ConfigureAwait(false);
                    task.externalId = externalId;
                }
                catch (Exception e)
                {
                    logger.LogError(e, $"Failed SetTaskExternalId Task(Name={task.name}, ExternalId={externalId})");
                }
            }
        }
    }
}
