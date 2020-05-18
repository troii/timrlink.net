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
            var tasks = await TaskService.GetTaskHierarchy();
            var taskDictionary = await TaskService.CreateExternalIdDictionary(tasks);
            Logger.LogInformation($"Found {tasks.Count} existing timr root tasks.");

            var csvEntries = ParseFile();
            Logger.LogInformation($"CSV contains {csvEntries.Count} entries.");

            var csvTasks = csvEntries.Select(entry =>
            {
                var asyncTask = Task.Run(async () =>
                {
                    var taskTokens = entry.Task.Split("|");
                    var parentTaskTokens = taskTokens.SkipLast(1).ToList();
                    await AddTaskTreeRecursive(taskDictionary, null, parentTaskTokens);

                    Core.API.Task task;
                    if (taskDictionary.TryGetValue(entry.Task, out var existingTask))
                    {
                        task = existingTask.Clone();
                    }
                    else
                    {
                        task = new Core.API.Task
                        {
                            externalId = entry.Task,
                            billable = true,
                            bookable = true
                        };
                    }

                    task.name = taskTokens.Last();
                    task.parentExternalId = String.Join("|", parentTaskTokens);
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

                    return task;
                });
                asyncTask.Wait();
                return asyncTask.Result;
            }).ToList();

            await TaskService.SynchronizeTasksByExternalId(taskDictionary, csvTasks, updateTasks: updateTasks);
        }

        async Task AddTaskTreeRecursive(IDictionary<string, Core.API.Task> tasks, string parentPath, IList<string> pathTokens)
        {
            if (pathTokens.Count == 0) return;

            var name = pathTokens.First();
            var currentPath = parentPath != null ? parentPath + "|" + name : name;

            if (!tasks.ContainsKey(currentPath))
            {
                var task = new Core.API.Task
                {
                    name = name,
                    externalId = currentPath,
                    parentExternalId = parentPath,
                    bookable = false,
                };
                await TaskService.AddTask(task);
                tasks.Add(currentPath, task);
            }

            await AddTaskTreeRecursive(tasks, currentPath, pathTokens.Skip(1).ToList());
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
        }
    }
}
