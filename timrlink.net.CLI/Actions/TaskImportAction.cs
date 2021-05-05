using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using Microsoft.Extensions.Logging;
using timrlink.net.Core.Service;
using Task = System.Threading.Tasks.Task;

namespace timrlink.net.CLI.Actions
{
    internal class TaskImportAction : ImportAction
    {
        private readonly string filename;
        private readonly bool updateTasks;
        private ITaskService TaskService { get; }

        public TaskImportAction(ILoggerFactory loggerFactory, string filename, bool updateTasks, ITaskService taskService)
            : base(filename, loggerFactory.CreateLogger<TaskImportAction>())
        {
            this.filename = filename;
            this.updateTasks = updateTasks;
            TaskService = taskService;
        }

        public override async Task Execute()
        {
            var tasks = TaskService.FlattenTasks(await TaskService.GetTaskHierarchy());
            var taskUuidDictionary = tasks.ToDictionary(task => task.uuid);
            var taskTokenDictionary = tasks.ToDictionary(task => Tokenize(task, taskUuidDictionary));
            Logger.LogInformation($"Found {tasks.Count} existing timr tasks.");

            var csvEntries = ParseFile();
            Logger.LogInformation($"CSV contains {csvEntries.Count} entries.");

            async Task AddTaskTreeRecursive(string parentPath, IList<string> pathTokens)
            {
                if (pathTokens.Count == 0) return;

                var name = pathTokens.First();
                var currentPath = parentPath != null ? parentPath + "|" + name : name;

                if (!taskTokenDictionary.TryGetValue(currentPath, out var task))
                {
                    var newTask = new Core.API.Task
                    {
                        name = name,
                        parentExternalId = parentPath,
                        externalId = currentPath,
                        bookable = false,
                    };
                    await TaskService.AddTask(newTask);
                    taskTokenDictionary.Add(currentPath, newTask);
                }
                else if (task.externalId != currentPath)
                {
                    task.externalId = currentPath;
                    await TaskService.UpdateTask(task);
                }

                await AddTaskTreeRecursive(currentPath, pathTokens.Skip(1).ToList());
            }

            var csvTasks = new List<Core.API.Task>();

            foreach (var entry in csvEntries)
            {
                var taskTokens = entry.Task.Split("|");
                var parentTaskTokens = taskTokens.SkipLast(1).ToList();
                await AddTaskTreeRecursive(null, parentTaskTokens);

                var parentExternalId = String.Join("|", parentTaskTokens);
                var task = taskTokenDictionary.TryGetValue(entry.Task, out var existingTask) ? existingTask.Clone() : new Core.API.Task();
                task.name = taskTokens.Last();
                task.externalId = entry.Task;
                task.parentExternalId = parentExternalId;
                task.bookable = entry.Bookable;
                task.billable = entry.Billable;
                task.description = entry.Description;
                task.start = entry.Start;
                task.startSpecified = entry.Start.HasValue;
                task.end = entry.End;
                task.endSpecified = entry.End.HasValue;
                task.customField1 = entry.CustomField1;
                task.customField2 = entry.CustomField2;
                task.customField3 = entry.CustomField3;
                
                csvTasks.Add(task);
                
                if (string.IsNullOrEmpty(entry.Subtasks) == false)
                {
                    var subtaskParentExternalId = entry.Task;
                    
                    var uniqueSubtasks = entry.SubtasksSplitted.Distinct().ToList();
                    if (entry.SubtasksSplitted.Length != uniqueSubtasks.Count)
                    {
                        Logger.LogWarning($"Duplicate subtask specified: '{entry.Subtasks}'");
                    }
                
                    foreach (var subtaskName in uniqueSubtasks)
                    {
                        var externalId = subtaskParentExternalId + "|" + subtaskName;
                        var subtask = taskTokenDictionary.TryGetValue(externalId, out var existingSubtask) ? existingSubtask.Clone() : new Core.API.Task();
                        subtask.parentExternalId = subtaskParentExternalId;
                        subtask.externalId = externalId;
                        subtask.name = subtaskName;
                        subtask.bookable = true;
                        subtask.billable = task.billable;

                        csvTasks.Add(subtask);
                    }
                }
            }

            await TaskService.SynchronizeTasksByExternalId(taskTokenDictionary, csvTasks, updateTasks: updateTasks);
        }

        private string Tokenize(Core.API.Task task, Dictionary<string, Core.API.Task> taskUuidDictionary)
        {
            var pathTokens = new List<string>();

            while (task != null)
            {
                pathTokens.Add(task.name);
                task = String.IsNullOrEmpty(task.parentUuid) ? null : taskUuidDictionary[task.parentUuid];
            }

            pathTokens.Reverse();
            return String.Join('|', pathTokens);
        }

        private IList<CSVEntry> ParseFile()
        {
            using (var fileReader = File.OpenRead(filename))
            using (var textReader = new StreamReader(fileReader))
            using (var csvReader = new CsvReader(textReader, new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = true, IgnoreBlankLines = true, Delimiter = ";" }))
            {
                csvReader.Configuration.CultureInfo = new CultureInfo("de");
                csvReader.Configuration.BadDataFound = context => Logger.LogWarning($"Bad Entry found: '{context.RawRecord}");
                csvReader.Configuration.ReadingExceptionOccurred = ex =>
                {
                    Logger.LogError(ex, $"Exception when parsing '{ex.ReadingContext.RawRecord}'");
                    return true;
                };

                return csvReader.GetRecords<CSVEntry>().ToImmutableList();
            }
        }

        private class CSVEntry
        {
            public string Task { get; set; }
            public bool Bookable { get; set; }
            public bool Billable { get; set; }
            public string Description { get; set; }
            public DateTime? Start { get; set; }
            public DateTime? End { get; set; }

            [Optional]
            public string CustomField1 { get; set; }

            [Optional]
            public string CustomField2 { get; set; }

            [Optional]
            public string CustomField3 { get; set; }
            
            [Optional]
            public string Subtasks { get; set; }

            [Ignore]
            public string[] SubtasksSplitted => Subtasks.Split(",");
        }
    }
}
