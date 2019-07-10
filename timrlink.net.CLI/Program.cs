using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using timrlink.net.Core;
using timrlink.net.Core.API;

namespace timrlink.net.CLI
{
    class Program
    {
        // call with SampleData.csv as argument
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            Application application = new ApplicationImpl(args);
            try
            {
                await application.Run();
            }
            catch (Exception e)
            {
                application.Logger.LogError(e, e.Message);
            }
        }
    }

    class ApplicationImpl : Application
    {
        private readonly string[] args;

        public ApplicationImpl(string[] args)
        {
            this.args = args;
        }

        protected override void SetupConfiguration(IConfigurationBuilder configurationBuilder)
        {
            /*
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string>
            {
                {"timrSync:identifier", "<identifier>"},
                {"timrSync:token", "<token>"}
            });
            */

            configurationBuilder.AddJsonFile("config.json");
        }

        protected override void ConfigureLogger(ILoggingBuilder loggingBuilder, IConfigurationRoot configuration)
        {
            base.ConfigureLogger(loggingBuilder, configuration);

            loggingBuilder.AddSerilog(new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .WriteTo.RollingFile("timrlink.net.{Date}.log")
                .WriteTo.Console(theme: Serilog.Sinks.SystemConsole.Themes.ConsoleTheme.None)
                .CreateLogger());
        }

        public override async System.Threading.Tasks.Task Run()
        {
            Logger.LogInformation("Running...");

            if (args.Length != 1)
            {
                Logger.LogError($"Invalid Argument count: {args.Length}; Required: 1");
                return;
            }

            List<CsvRecord> records;
            try
            {
                records = ParseFile(args[0]);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Could not read passed file!");
                return;
            }

            Logger.LogInformation($"found {records.Count} entries");

            await ImportProjectTimeRecords(records);

            Logger.LogInformation("End.");
        }

        private List<CsvRecord> ParseFile(string filename)
        {
            Logger.LogInformation($"Reading file: {filename}");

            switch (Path.GetExtension(filename))
            {
                case ".csv":
                    return ParseCSV(filename);
                case ".xlsx":
                    return ParseXLSX(filename);
                default:
                    throw new ArgumentException($"Unsupported file type '{filename}' - use .csv or .xlsx!");
            }
        }

        private List<CsvRecord> ParseCSV(string filename)
        {
            using (var fileReader = File.OpenRead(filename))
            using (var textReader = new StreamReader(fileReader))
            using (var csvReader = new CsvReader(textReader, new Configuration { IgnoreBlankLines = true }))
            {
                return csvReader.GetRecords<CsvRecord>().ToList();
            }
        }

        private List<CsvRecord> ParseXLSX(string filename)
        {
            throw new NotImplementedException();
        }

        private async System.Threading.Tasks.Task ImportProjectTimeRecords(List<CsvRecord> records)
        {
            var tasks = await TaskService.GetTaskHierarchy();
            var taskDictionary = await TaskService.CreateExternalIdDictionary(tasks,
                task => task.parentExternalId != null ? task.parentExternalId + "|" + task.name : task.name
            );

            foreach (var record in records)
            {
                if (!taskDictionary.ContainsKey(record.Task))
                {
                    try
                    {
                        await AddTaskTreeRecursive(taskDictionary, null, record.Task.Split("|"));
                    }
                    catch (Exception e)
                    {
                        Logger.LogError(e, $"Failed to add missing Task tree for record: {record}");
                        continue;
                    }
                }

                try
                {
                    var projectTime = new ProjectTime
                    {
                        externalTaskId = record.Task,
                        externalUserId = record.User,
                        startTime = DateTime.ParseExact(record.StartDateTime, "dd.MM.yy H:mm", CultureInfo.InvariantCulture),
                        endTime = DateTime.ParseExact(record.EndDateTime, "dd.MM.yy H:mm", CultureInfo.InvariantCulture),
                        breakTime = (int) TimeSpan.Parse(record.Break).TotalMinutes,
                        description = record.Notes,
                        billable = record.Billable
                    };

                    await ProjectTimeService.SaveProjectTime(projectTime);
                }
                catch (FormatException e)
                {
                    Logger.LogError(e, $"Error parsing record: {record}");
                }
            }
        }

        private async System.Threading.Tasks.Task AddTaskTreeRecursive(IDictionary<string, Task> tasks, string parentPath, IList<string> pathTokens)
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
