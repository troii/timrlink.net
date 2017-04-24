using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace timrlink.net.Core.Service
{
    internal class TaskService : ITaskService
    {
        private readonly ILoggerFactory loggerFactory;
        private readonly ILogger<TaskService> logger;
        private readonly API.TimrSync timrSync;

        public TaskService(ILoggerFactory loggerFactory, API.TimrSync timrSync)
        {
            this.loggerFactory = loggerFactory;
            this.timrSync = timrSync;

            this.logger = loggerFactory.CreateLogger<TaskService>();
        }

        public Task<IDictionary<string, API.Task>> GetExistingTasksAsync(Func<API.Task, string> externalIdLookup = null)
        {
            return Task.Run(() => GetExistingTasks(externalIdLookup));
        }

        public IDictionary<string, API.Task> GetExistingTasks(Func<API.Task, string> externalIdLookup = null)
        {
            var existingTasks = timrSync.GetTasks(new API.GetTasksRequest("")).Tasks;

            IDictionary<string, API.Task> taskDictionary = new Dictionary<string, API.Task>();
            AddTaskIDs(taskDictionary, existingTasks, null, externalIdLookup);
            logger.LogDebug($"Total task count: {existingTasks.Count}");

            return taskDictionary;
        }

        public void SynchronizeTasks(IDictionary<string, API.Task> existingTasks, IList<API.Task> remoteTasks, bool updateTasks = false, IEqualityComparer<API.Task> equalityComparer = null)
        {
            if (equalityComparer == null)
            {
                equalityComparer = new DefaultTaskEqualityComparer(loggerFactory);
            }

            foreach (API.Task task in remoteTasks)
            {
                try
                {
                    UpdateOrAddTask(existingTasks, task, updateTasks, equalityComparer);
                }
                catch (Exception e)
                {
                    logger.LogError(new EventId(), e, $"Failed synchronizing Task(Name={task.Name}, ExternalId={task.ExternalId})");
                }
            }
        }

        protected void UpdateOrAddTask(IDictionary<string, API.Task> existingTaskIDs, API.Task task, bool updateTask, IEqualityComparer<API.Task> equalityComparer)
        {
            logger.LogDebug($"Checking Task(Name={task.Name}, ExternalId={task.ExternalId})");

            API.Task existingTask;
            if (existingTaskIDs.TryGetValue(task.ExternalId, out existingTask))
            {
                if (updateTask && !equalityComparer.Equals(task, existingTask))
                {
                    logger.LogInformation($"Updating Task(Name={task.Name}, ExternalId={task.ExternalId})");
                    timrSync.UpdateTask(new API.UpdateTaskRequest(task));
                    existingTaskIDs[task.ExternalId] = task;
                }
            }
            else
            {
                logger.LogInformation($"Adding Task(Name={task.Name}, ExternalId={task.ExternalId})");
                timrSync.AddTask(new API.AddTaskRequest(task));
                existingTaskIDs.Add(task.ExternalId, task);
                if (task.Subtasks != null)
                {
                    foreach (API.Task subtask in task.Subtasks)
                    {
                        timrSync.AddTask(new API.AddTaskRequest(subtask));
                        existingTaskIDs.Add(subtask.ExternalId, subtask);
                    }
                }
            }
        }

        private void AddTaskIDs(IDictionary<string, API.Task> existingTaskIDs, IEnumerable<API.Task> existingTasks, string parentExternalId, Func<API.Task, string> externalIdLookup)
        {
            foreach (API.Task task in existingTasks)
            {
                task.ParentExternalId = parentExternalId;

                if (String.IsNullOrEmpty(task.ExternalId) && externalIdLookup != null)
                {
                    UpdateTaskWithoutExternalId(task, externalIdLookup);
                }

                if (!String.IsNullOrEmpty(task.ExternalId))
                {
                    if (existingTaskIDs.ContainsKey(task.ExternalId))
                    {
                        logger.LogError($"Duplicate ExternalId: Task(Name={task.Name}, ExternalId={task.ExternalId})");
                    }
                    else
                    {
                        existingTaskIDs.Add(task.ExternalId, task);
                        if (task.Subtasks != null)
                        {
                            AddTaskIDs(existingTaskIDs, task.Subtasks, task.ExternalId, externalIdLookup);
                        }
                    }
                }
            }
        }

        private void UpdateTaskWithoutExternalId(API.Task task, Func<API.Task, string> externalIdLookup)
        {
            var externalId = externalIdLookup(task);
            if (!String.IsNullOrEmpty(externalId))
            {
                logger.LogInformation($"Updating previous untracked Task(Name={task.Name}) with ExternalId: {externalId}");
                try
                {
                    timrSync.SetTaskExternalId(new API.SetTaskExternalIdRequest1
                    {
                        SetTaskExternalIdRequest = new API.SetTaskExternalIdRequest
                        {
                            Name = task.Name,
                            NewExternalTaskId = externalId,
                            ParentExternalId = task.ParentExternalId
                        }
                    });
                    task.ExternalId = externalId;
                }
                catch (Exception e)
                {
                    logger.LogError(new EventId(), e, $"Failed SetTaskExternalId Task(Name={task.Name}, ExternalId={externalId})");
                }
            }
        }
    }
}