using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using timrlink.net.Core.Service;

namespace timrlink.net.CLI.Actions
{
    internal class ProjectTimeCSVImportAction : ProjectTimeImportAction
    {
        public ProjectTimeCSVImportAction(ILoggerFactory loggerFactory, string filename, ITaskService taskService, IUserService userService, IProjectTimeService projectTimeService)
            : base(loggerFactory.CreateLogger<ProjectTimeCSVImportAction>(), filename, taskService, userService, projectTimeService)
        {
        }

        protected override IEnumerable<ProjectTimeEntry> ParseFile()
        {
            using (var fileReader = File.OpenRead(Filename))
            using (var textReader = new StreamReader(fileReader))
            using (var csvReader = new CsvReader(textReader, new CsvConfiguration(CultureInfo.InvariantCulture) { IgnoreBlankLines = true, Delimiter = ";" }))
            {
                return csvReader.GetRecords<CsvRecord>().Select(record =>
                {
                    try
                    {
                        return new ProjectTimeEntry()
                        {
                            Task = record.Task,
                            User = record.User,
                            StartDateTime = DateTime.ParseExact(record.StartDateTime, "dd.MM.yy H:mm", CultureInfo.InvariantCulture),
                            EndDateTime = DateTime.ParseExact(record.EndDateTime, "dd.MM.yy H:mm", CultureInfo.InvariantCulture),
                            Break = (int) TimeSpan.Parse(record.Break).TotalMinutes,
                            Notes = record.Notes,
                            Billable = record.Billable
                        };
                    }
                    catch (FormatException e)
                    {
                        Logger.LogError(e, $"Error parsing record: {record}");
                        return null;
                    }
                }).Where(record => record != null).ToList();
            }
        }

        private class CsvRecord
        {
            public string User { get; set; }
            public string Task { get; set; }
            public string StartDateTime { get; set; }
            public string EndDateTime { get; set; }
            public string Break { get; set; }
            public string Notes { get; set; }
            public bool Billable { get; set; }

            public override string ToString()
            {
                return $"Record(User={User}, Task={Task}, StartDateTime={StartDateTime}, EndDateTime={EndDateTime}, Break={Break}, Notes={Notes}, Billable={Billable}";
            }
        }
    }
}
