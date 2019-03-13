using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using timrlink.net.Core;
using timrlink.net.Core.API;

namespace timrlink.net.SampleCSVDotNetCore
{
    class Program
    {
        // call with SampleData.csv as argument
        static void Main(string[] args)
        {
            Application application = new ApplicationImpl(args);
            try
            {
                application.Run();
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

        protected override void ConfigureLogger(ILoggerFactory loggerFactory)
        {
            base.ConfigureLogger(loggerFactory);

            loggerFactory.AddConsole(LogLevel.Debug, false);
            loggerFactory.AddSerilog(new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.RollingFile("timrlink.net.{Date}.log")
                .CreateLogger());
        }

        public override void Run()
        {
            Logger.LogInformation("Running...");

            if (args.Length != 1)
            {
                Logger.LogError($"Invalid Argument count: {args.Length}; Required: 1");
                return;
            }

            string filename = args[0];
            List<CsvRecord> records;

            try
            {
                Logger.LogInformation($"Reading file: {filename}");

                using (var fileReader = File.OpenRead(filename))
                using (var textReader = new StreamReader(fileReader))
                using (var csvReader = new CsvReader(textReader, new Configuration { IgnoreBlankLines = true }))
                {
                    records = csvReader.GetRecords<CsvRecord>().ToList();
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Could not read passed file '{filename}'.");
                return;
            }

            Logger.LogInformation($"found {records.Count} entries");

            var tasks = TaskService.GetExistingTasks(
                task => task.ParentExternalId != null ? task.ParentExternalId + "|" + task.Name : task.Name
            );

            foreach (var record in records)
            {
                if (!tasks.ContainsKey(record.Task))
                {
                    try
                    {
                        AddTaskTreeRecursive(tasks, null, record.Task.Split("|"));
                    }
                    catch (Exception e)
                    {
                        Logger.LogError(e, $"Failed to add missing Task tree for record: {record}");
                        continue;
                    }
                }

                ProjectTime projectTime;
                try
                {
                    projectTime = new ProjectTime
                    {
                        ExternalTaskId = record.Task,
                        ExternalUserId = record.User,
                        StartTime = DateTime.ParseExact(record.StartDateTime, "dd.MM.yy HH:mm", CultureInfo.InvariantCulture),
                        EndTime = DateTime.ParseExact(record.EndDateTime, "dd.MM.yy HH:mm", CultureInfo.InvariantCulture),
                        BreakTime = (int) TimeSpan.Parse(record.Break).TotalMinutes,
                        Description = record.Notes,
                        Billable = record.Billable
                    };
                }
                catch (Exception e)
                {
                    Logger.LogError(e, $"Error parsing record: {record}");
                    continue;
                }

                ProjectTimeService.SaveProjectTime(projectTime);
            }

            Logger.LogInformation("End.");
        }

        private void AddTaskTreeRecursive(IDictionary<string, Task> tasks, string parentPath, IList<string> pathTokens)
        {
            if (pathTokens.Count == 0) return;

            var name = pathTokens.First();
            var currentPath = parentPath != null ? parentPath + "|" + name : name;

            if (!tasks.ContainsKey(currentPath))
            {
                Task task = new Task
                {
                    Name = name,
                    ExternalId = currentPath,
                    ParentExternalId = parentPath,
                    Bookable = true,
                };
                TaskService.AddTask(task);
                tasks.Add(currentPath, task);
            }

            AddTaskTreeRecursive(tasks, currentPath, pathTokens.Skip(1).ToList());
        }
    }
}
