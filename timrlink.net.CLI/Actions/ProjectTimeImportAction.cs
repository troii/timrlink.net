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
            IList<Core.API.ProjectTime> records;
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

        protected abstract IEnumerable<Core.API.ProjectTime> ParseFile();

        private async System.Threading.Tasks.Task ImportProjectTimeRecords(IList<Core.API.ProjectTime> records)
        {
            var users = await UserService.GetUsers();
            var userDictionary = users
                .Where(user => user.externalId != null)
                .GroupBy(user => user.externalId)
                .ToDictionary(group => group.Key, group => group.ToList());
            
            var tasks = await TaskService.GetTaskHierarchy();
            var taskDictionary = await TaskService.CreateExternalIdDictionary(tasks,
                task => task.parentExternalId != null ? task.parentExternalId + "|" + task.name : task.name
            );
            
            foreach (var record in records)
            {
                var externalUserId = record.externalUserId;
                if (!userDictionary.TryGetValue(externalUserId, out var projectTimeUsers))
                {
                    Logger.LogWarning($"User with ExternalId '{externalUserId}' not found. Skipping {record} to import.");
                    continue;
                }
                else if (projectTimeUsers.Count > 1)
                {
                    Logger.LogError($"User with ExternalId '{externalUserId}' not unique (logins=[{String.Join(", ", projectTimeUsers.Select(u => u.login))}]). Skipping {record} to import.");
                    continue;
                }

                if (!taskDictionary.ContainsKey(record.externalTaskId))
                {
                    try
                    {
                        await AddTaskTreeRecursive(taskDictionary, null, record.externalTaskId.Split("|"));
                    }
                    catch (Exception e)
                    {
                        Logger.LogError(e, $"Failed to add missing Task tree for {record}");
                        continue;
                    }
                }

                await ProjectTimeService.SaveProjectTime(record);
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
