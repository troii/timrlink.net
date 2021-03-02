using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using timrlink.net.Core.Service;
using MissingFieldException = CsvHelper.MissingFieldException;
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

            async Task<String> AddTaskTreeRecursive(string parentUUID, string path, IList<string> pathTokens)
            {
                if (pathTokens.Count == 0)
                {
                    return parentUUID;
                }

                var name = pathTokens.First();
                var currentUuid = Guid.NewGuid().ToString();
                var currentPath = path != null ? path + "|" + name : name;

                if (!taskTokenDictionary.TryGetValue(currentPath, out var task))
                {
                    var newTask = new Core.API.Task
                    {
                        name = name,
                        parentUuid = parentUUID,
                        uuid = currentUuid,
                        bookable = false,
                    };
                    await TaskService.AddTask(newTask);
                    taskTokenDictionary.Add(currentPath, newTask);
                }
                else
                {
                    currentUuid = task.uuid;
                }
                
                return await AddTaskTreeRecursive(currentUuid, currentPath, pathTokens.Skip(1).ToList());
            }

            foreach (var entry in csvEntries)
            {   
                var taskTokens = entry.Task.Split("|");
                var parentTaskTokens = taskTokens.SkipLast(1).ToList();
                var uuid = await AddTaskTreeRecursive(null, null, parentTaskTokens);

                var task = new Core.API.Task();

                var existingTasks = tasks.Where(task => task.externalId == entry.ExternalId);

                if (existingTasks.Count() > 1)
                {
                    Logger.LogError($"Duplicate Tasks with ExternalId {entry.ExternalId} found. Skipping.");
                    continue;
                }

                var existingCSVTAsks = csvEntries.Where(task => task.ExternalId == entry.ExternalId);
                
                if (existingCSVTAsks.Count() > 1)
                {
                    Logger.LogError($"Duplicate Tasks with ExternalId {entry.ExternalId} in CSV file found. Skipping.");
                    continue;
                }

                if (entry.ExternalId != null && existingTasks.Count() == 1)
                {
                    task = existingTasks.First();
                } 
                else if (taskTokenDictionary.TryGetValue(entry.Task, out var existingTask))
                {
                    task = existingTask.Clone();
                }
                
                task.name = taskTokens.Last();
                task.bookable = entry.Bookable;
                task.billable = entry.Billable;
                task.description = entry.Description;
                task.descriptionRequired = entry.DescriptionRequired;
                task.start = entry.Start;
                task.startSpecified = entry.Start.HasValue;
                task.end = entry.End;
                task.endSpecified = entry.End.HasValue;
                task.customField1 = entry.CustomField1;
                task.customField2 = entry.CustomField2;
                task.customField3 = entry.CustomField3;
                task.parentUuid = uuid;
                if (!String.IsNullOrEmpty(entry.ExternalId))
                {
                    task.externalId = entry.ExternalId;
                }
                if (task.uuid == null)
                {
                    task.uuid = Guid.NewGuid().ToString();
                    await TaskService.AddTask(task);
                }
                else
                {
                    await TaskService.UpdateTask(task);
                }
            }
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

                try
                {
                    return csvReader.GetRecords<CSVEntry>().ToImmutableList();
                }
                catch (HeaderValidationException e)
                {
                    Logger.LogError(e.Message);
                }

                return new List<CSVEntry>();
            }
        }

        private class CSVEntry
        {
            public string Task { get; set; }
            public string ExternalId { get; set; }
            public bool Bookable { get; set; }
            public bool Billable { get; set; }
            public string Description { get; set; }
            public bool DescriptionRequired { get; set; }
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
