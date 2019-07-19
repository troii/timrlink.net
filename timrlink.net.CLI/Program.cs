using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using timrlink.net.CLI.Actions;
using timrlink.net.Core;

namespace timrlink.net.CLI
{
    internal class Program
    {
        private static async Task<int> Main(string[] args)
        {
            Application application = new ApplicationImpl(args);
            try
            {
                return await application.Run();
            }
            catch (Exception e)
            {
                application.Logger.LogError(e, e.Message);
                return 1;
            }
        }
    }

    internal class ApplicationImpl : Application
    {
        private readonly string[] args;

        public ApplicationImpl(string[] args)
        {
            this.args = args;
        }

        protected override void SetupConfiguration(IConfigurationBuilder configurationBuilder)
        {
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

        public override async Task<int> Run()
        {
            var projectTimeCommand = new Command("projecttime");
            projectTimeCommand.AddAlias("pt");
            projectTimeCommand.AddArgument(new Argument<string>("filename"));
            projectTimeCommand.Handler = CommandHandler.Create<string>(ImportProjectTime);

            var rootCommand = new RootCommand
            {
                projectTimeCommand
            };

            return await rootCommand.InvokeAsync(args);
        }

        private async Task ImportProjectTime(string filename)
        {
            ImportAction action;
            switch (Path.GetExtension(filename))
            {
                case ".csv":
                    action = new ProjectTimeCSVImportAction(LoggerFactory, filename, TaskService, ProjectTimeService);
                    break;
                case ".xlsx":
                    action = new ProjectTimeXLSXImportAction(LoggerFactory, filename, TaskService, ProjectTimeService);
                    break;
                default:
                    throw new ArgumentException($"Unsupported file type '{filename}' - use .csv or .xlsx!");
            }

            await action.Execute();
        }
    }
}
