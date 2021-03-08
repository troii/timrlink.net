using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using timrlink.net.Core.API;
using timrlink.net.Core.Service;

namespace timrlink.net.CLI.Actions
{
    internal abstract class ProjectTimeImportAction : ImportAction
    {
        protected ITaskService TaskService { get; }
        protected IUserService UserService { get; }
        protected IProjectTimeService ProjectTimeService { get; }

        protected ProjectTimeImportAction(ILogger logger, string filename, ITaskService taskService, IUserService userService, IProjectTimeService projectTimeService) : base(filename, logger)
        {
            TaskService = taskService;
            UserService = userService;
            ProjectTimeService = projectTimeService;
        }

        public sealed override async System.Threading.Tasks.Task Execute()
        {
            IList<ProjectTimeEntry> records;
            try
            {
                records = ParseFile().ToList();
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Could not read passed file!");
                return;
            }

            Logger.LogInformation($"found {records.Count} entries");

            await ImportProjectTimeRecords(records);
        }

        protected abstract IEnumerable<ProjectTimeEntry> ParseFile();

        private async System.Threading.Tasks.Task ImportProjectTimeRecords(IList<ProjectTimeEntry> records)
        {
            var users = await UserService.GetUsers();
            var userDictionary = users
                .Where(user => user.externalId != null)
                .ToDictionary(user => user.externalId);
            var tasks = await TaskService.GetTaskHierarchy();

            var taskDictionary = new Dictionary<string, Task>();
            CreatePathDictionary(taskDictionary, tasks);
            
            foreach (var record in records)
            {
                var externalId = record.User;
                if (userDictionary.TryGetValue(externalId, out var user))
                {
                    
                }
                else
                {
                    Logger.LogError($"User with ExternalId {externalId} not found. Skipping ProjectTime to import.");
                    continue;
                }
                
                string uuid;
                
                if (taskDictionary.TryGetValue(record.Task, out var task))
                {
                    uuid = task.uuid;
                }
                else
                {
                    try
                    {
                        uuid = await AddTaskTreeRecursive(taskDictionary, null, null, record.Task.Split("|"));
                    }
                    catch (Exception e)
                    {
                        Logger.LogError(e, $"Failed to add missing Task tree for record: {record}");
                        continue;
                    }
                }

                var projectTime = record.CreateProjectTime();
                projectTime.taskUuid = uuid;
                projectTime.userUuid = user.uuid;
                await ProjectTimeService.SaveProjectTime(projectTime);
            }
        }

        private void CreatePathDictionary(IDictionary<string, Task> tasks, IList<Task> allTasks)
        {
            foreach (var task in allTasks)
            {
                CreatePathDictionaryRecursive(tasks, task, null);
            }
        }

        private void CreatePathDictionaryRecursive(IDictionary<string, Task> tasks, Task task, string path)
        {
            var currentPath = path != null ? path + "|" + task.name : task.name;
            tasks[currentPath] = task;
            
            if (task.subtasks == null || task.subtasks.Count() == 0)
            {
                return;
            }

            foreach (var subtask in task.subtasks)
            {
                CreatePathDictionaryRecursive(tasks, subtask, currentPath);
            }
        }

        protected async System.Threading.Tasks.Task<String> AddTaskTreeRecursive(IDictionary<string, Task> tasks, string parentUuid, string parentPath, IList<string> pathTokens)
        {
            if (pathTokens.Count == 0)
            {
                return parentUuid;
            }

            var name = pathTokens.First();
            var currentPath = parentPath != null ? parentPath + "|" + name : name;
            
            if (!tasks.TryGetValue(currentPath, out var task))
            {
                task = new Task
                {
                    name = name,
                    uuid = Guid.NewGuid().ToString(),
                    parentUuid =  parentUuid,
                    bookable = true,
                    externalId = Guid.NewGuid().ToString()
                };
                await TaskService.AddTask(task);
                tasks.Add(currentPath, task);
            }

            return await AddTaskTreeRecursive(tasks, task.uuid, currentPath, pathTokens.Skip(1).ToList());
        }
    }
}
