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
using timrlink.net.Core.API;
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
            var taskTokenDictionary = tasks.ToTokenDictionary();
            var addedTasks = new Dictionary<string, Core.API.Task>();
            
            Logger.LogInformation($"Found {tasks.Count} existing timr tasks.");

            var csvEntries = ParseFile();
            Logger.LogInformation($"CSV contains {csvEntries.Count} entries.");

            var csvTasks = new List<Core.API.Task>();

            foreach (var entry in csvEntries)
            {
                var taskTokens = entry.Task.Split("|");
                var parentTaskTokens = taskTokens.SkipLast(1).ToList();
                await TaskService.AddTaskTreeRecursive(null, parentTaskTokens, taskTokenDictionary, addedTasks, bookable: false);

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
                task.descriptionRequired = entry.DescriptionRequired;
                task.address = entry.Address;
                task.city = entry.City;
                task.zipCode = entry.ZipCode;
                task.state = entry.State;
                task.country = entry.Country;

                // We only set latitude and longitude if we find both
                if (entry.Latitude != null && entry.Longitude != null)
                {
                    task.latitude = entry.Latitude;
                    task.latitudeSpecified = true;
                    task.longitude = entry.Longitude;
                    task.longitudeSpecified = true;
                }

                if (entry.BudgetPlanningType != null) {
                    task.budgetPlanningType = entry.BudgetPlanningType;
                    task.budgetPlanningTypeSpecified = true;
                    task.budgetPlanningTypeInherited = entry.BudgetPlanningTypeInherited;
                    task.budgetPlanningTypeInheritedSpecified = true;
                    task.hoursPlanned = entry.HoursPlanned;
                    task.hoursPlannedSpecified = entry.HoursPlanned.HasValue;
                    task.hourlyRate = entry.HourlyRate;
                    task.hourlyRateSpecified = entry.HourlyRate.HasValue;
                    task.budgetPlanned = entry.BudgetPlanned;
                    task.budgetPlannedSpecified = entry.BudgetPlanned.HasValue;
                }

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

            await TaskService.SynchronizeTasksByExternalId(taskTokenDictionary, addedTasks, csvTasks, updateTasks: updateTasks);
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

            [Optional] 
            public bool DescriptionRequired { get; set; }
            
            [Optional] 
            public string Address { get; set; }
            
            [Optional] 
            public string City { get; set; }
            
            [Optional] 
            public string ZipCode { get; set; }
            
            [Optional] 
            public string State { get; set; }
            
            [Optional] 
            public string Country { get; set; }
            
            [Optional] 
            public double? Latitude { get; set; }
            
            [Optional] 
            public double? Longitude { get; set; }
            
            [Optional]
            public BudgetPlanningType? BudgetPlanningType { get; set; }
            
            [Optional] 
            public bool BudgetPlanningTypeInherited { get; set; }
            
            [Optional] 
            public decimal? HoursPlanned { get; set; }
            
            [Optional] 
            public decimal? HourlyRate { get; set; }

            [Optional] 
            public decimal? BudgetPlanned { get; set; }
        }
    }
}
