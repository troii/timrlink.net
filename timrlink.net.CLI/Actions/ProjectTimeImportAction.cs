using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using timrlink.net.Core.API;
using timrlink.net.Core.Service;

namespace timrlink.net.CLI.Actions
{
    public abstract class ProjectTimeImportAction : ImportAction
    {
        protected ITaskService TaskService { get; }
        protected IProjectTimeService ProjectTimeService { get; }

        protected ProjectTimeImportAction(ILogger logger, string filename, ITaskService taskService, IProjectTimeService projectTimeService) : base(filename, logger)
        {
            TaskService = taskService;
            ProjectTimeService = projectTimeService;
        }

        public sealed override async System.Threading.Tasks.Task Execute()
        {
            IList<ProjectTime> records;
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

        protected abstract IEnumerable<ProjectTime> ParseFile();

        private async System.Threading.Tasks.Task ImportProjectTimeRecords(IList<ProjectTime> records)
        {
            var tasks = await TaskService.GetTaskHierarchy();
            var taskDictionary = await TaskService.CreateExternalIdDictionary(tasks,
                task => task.parentExternalId != null ? task.parentExternalId + "|" + task.name : task.name
            );

            foreach (var record in records)
            {
                if (!taskDictionary.ContainsKey(record.externalTaskId))
                {
                    try
                    {
                        await AddTaskTreeRecursive(taskDictionary, null, record.externalTaskId.Split("|"));
                    }
                    catch (Exception e)
                    {
                        Logger.LogError(e, $"Failed to add missing Task tree for record: {record}");
                        continue;
                    }
                }

                await ProjectTimeService.SaveProjectTime(record);
            }
        }

        protected async System.Threading.Tasks.Task AddTaskTreeRecursive(IDictionary<string, Task> tasks, string parentPath, IList<string> pathTokens)
        {
            if (pathTokens.Count == 0) return;

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
