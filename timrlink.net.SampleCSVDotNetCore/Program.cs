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

            foreach (var record in records)
            {
                ProjectTime projectTime;
                try
                {
                    projectTime = new ProjectTime
                    {
                        ExternalTaskId = record.Task,
                        ExternalUserId = record.User,
                        StartTime = DateTime.ParseExact($"{record.Date} {record.Start}", "d/M/yy H:mm", CultureInfo.InvariantCulture),
                        EndTime = DateTime.ParseExact($"{record.Date} {record.End}", "d/M/yy H:mm", CultureInfo.InvariantCulture),
                        BreakTime = (int) TimeSpan.Parse(record.Break).TotalMinutes,
                        Description = record.Notes
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
    }
}
