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

            var taskDictionary = await TaskService.CreateExternalIdDictionary(tasks,
                task => task.parentExternalId != null ? task.parentExternalId + "|" + task.name : task.name
            );
            
            foreach (var record in records)
            {
                var externalUserId = record.User;
                if (!userDictionary.TryGetValue(externalUserId, out _))
                {
                    Logger.LogError($"User with ExternalId {externalUserId} not found. Skipping ProjectTime to import.");
                    continue;
                }
                
                try
                {
                    await AddTaskTreeRecursive(taskDictionary, null, record.Task.Split("|"));
                }
                catch (Exception e)
                {
                    Logger.LogError(e, $"Failed to add missing Task tree for record: {record}");
                    continue;
                }
                
                var projectTime = record.CreateProjectTime();
                projectTime.externalTaskId = record.Task;
                projectTime.externalUserId = record.User;
                await ProjectTimeService.SaveProjectTime(projectTime);
            }
        }

        protected async System.Threading.Tasks.Task AddTaskTreeRecursive(IDictionary<string, Task> tasks, string parentPath, IList<string> pathTokens)
        {
            if (pathTokens.Count == 0)
            {
                return;
            }

            var name = pathTokens.First();
            var currentPath = parentPath != null ? parentPath + "|" + name : name;
            
            if (!tasks.ContainsKey(currentPath))
            {
                Task task = new Task
                {
                    name = name,
                    externalId = currentPath,
                    parentExternalId = parentPath,
                    bookable = true,
                };
                await TaskService.AddTask(task);
                tasks.Add(currentPath, task);
            }

            await AddTaskTreeRecursive(tasks, currentPath, pathTokens.Skip(1).ToList());
        }
    }
}
