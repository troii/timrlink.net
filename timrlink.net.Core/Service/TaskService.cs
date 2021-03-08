using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
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

        public async Task<IList<API.Task>> GetTaskHierarchy()
        {
            var getTasksResponse = await timrSync.GetTasksAsync(new API.GetTasksRequest1(new API.GetTasksRequest())).ConfigureAwait(false);
            var rootTaskArray = getTasksResponse.GetTasksResponse1;
            logger.LogDebug($"Root task count: {rootTaskArray.Length}");

            // Update parent-ids which are not set in the SOAP-API response
            void UpdateIds(IEnumerable<API.Task> tasks, string parentExternalId, string parentUuid)
            {
                if (tasks != null)
                {
                    Parallel.ForEach(tasks, task =>
                    {
                        task.parentExternalId = parentExternalId;
                        task.parentUuid = parentUuid;
                        UpdateIds(task.subtasks, task.externalId, task.uuid);
                    });
                }
            }

            UpdateIds(rootTaskArray, null, null);

            return rootTaskArray.ToList();
        }

        IList<API.Task> ITaskService.FlattenTasks(IEnumerable<API.Task> tasks)
        {
            return TaskService.FlattenTasks(tasks);
        }

        internal static IList<API.Task> FlattenTasks(IEnumerable<API.Task> tasks)
        {
            return tasks.SelectMany(task =>
            {
                var list = new List<API.Task> { task };
                if (task.subtasks != null)
                {
                    list.AddRange(FlattenTasks(task.subtasks));
                }

                return list;
            }).ToList();
        }

        public async Task<IDictionary<string, API.Task>> CreateExternalIdDictionary(IEnumerable<API.Task> tasks, Func<API.Task, string> externalIdLookup = null)
        {
            IDictionary<string, API.Task> taskDictionary = new Dictionary<string, API.Task>();
            await AddTaskIDs(taskDictionary, tasks, null, externalIdLookup).ConfigureAwait(false);
            return taskDictionary;
        }

        public async Task AddTask(API.Task task)
        {
            logger.LogInformation($"Adding Task(Name={task.name}, ExternalId={task.externalId} UUID={task.uuid} parentUUID={task.parentUuid})");
            await timrSync.AddTaskAsync(new API.AddTaskRequest(task)).ConfigureAwait(false);
        }

        public async Task UpdateTask(API.Task task)
        {
            logger.LogInformation($"Updating Task(Name={task.name}, ExternalId={task.externalId})");
            await timrSync.UpdateTaskAsync(new API.UpdateTaskRequest(task)).ConfigureAwait(false);
        }

        public async Task SynchronizeTasksByExternalId(IDictionary<string, API.Task> existingTasks, IList<API.Task> remoteTasks, bool updateTasks = false, bool disableMissingTasks = false, IEqualityComparer<API.Task> equalityComparer = null)
        {
            if (equalityComparer == null)
            {
                equalityComparer = new DefaultTaskEqualityComparer(loggerFactory);
            }

            var taskDictionary = remoteTasks.ToDictionary(task => task.externalId);
            if (disableMissingTasks)
            {
                foreach (var existingTask in existingTasks)
                {
                    if (!taskDictionary.ContainsKey(existingTask.Key))
                    {
                        var task = existingTask.Value.Clone();
                        task.end = DateTime.Today.AddDays(-1);
                        task.endSpecified = true;
                        taskDictionary.Add(task.externalId, task);
                    }
                }
            }

            foreach (API.Task task in taskDictionary.Values)
            {
                try
                {
                    await AddOrUpdateTask(existingTasks, task, updateTasks, equalityComparer).ConfigureAwait(false);
                }
                catch (FaultException e)
                {
                    logger.LogError($"Failed synchronizing Task(Name={task.name}, ExternalId={task.externalId}): {e.Message}");
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
                catch (FaultException e)
                {
                    logger.LogError($"Failed SetTaskExternalId Task(Name={task.name}, ExternalId={externalId}): {e.Message}");
                }
                catch (Exception e)
                {
                    logger.LogError(e, $"Failed SetTaskExternalId Task(Name={task.name}, ExternalId={externalId})");
                }
            }
        }
    }
}
