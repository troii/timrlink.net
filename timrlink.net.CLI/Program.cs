using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using timrlink.net.CLI.Actions;
using timrlink.net.Core;
using timrlink.net.Core.Service;

[assembly: InternalsVisibleTo("timrlink.net.CLI.Test")]

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
            var filenameArgument = new Argument<string>("filename");
            filenameArgument.LegalFilePathsOnly();

            var projectTimeCommand = new Command("projecttime", "Import project times");
            projectTimeCommand.AddAlias("pt");
            projectTimeCommand.AddArgument(filenameArgument);
            projectTimeCommand.Handler = CommandHandler.Create<string>(ImportProjectTime);

            var updateTasks = new Option("--update", "Update existing tasks with same externalId");
            updateTasks.AddAlias("-u");
            updateTasks.Argument.SetDefaultValue(true);
            updateTasks.Argument.ArgumentType = typeof(bool);

            var taskCommand = new Command("task", "Import tasks");
            taskCommand.AddAlias("t");
            taskCommand.AddArgument(filenameArgument);
            taskCommand.AddOption(updateTasks);
            taskCommand.Handler = CommandHandler.Create<string, bool>(ImportTasks);

            var exportProjectTimeCommand = new Command("export-projecttime", "Export Project times");
            exportProjectTimeCommand.AddOption(new Option<string>("connectionstring"));
            exportProjectTimeCommand.AddOption(new Option<string>("from"));
            exportProjectTimeCommand.AddOption(new Option<string>("to"));
            exportProjectTimeCommand.Handler = CommandHandler.Create<string, string, string>(ExportProjectTime);
            exportProjectTimeCommand.Handler = CommandHandler.Create<string>(ExportProjectTime);
            
            var exportGroupsTimeCommand = new Command("export-groups", "Export Groups (Organizations)");
            exportProjectTimeCommand.AddOption(new Option<string>("connectionstring"));
            exportProjectTimeCommand.Handler = CommandHandler.Create<string>(ExportProjectTime);

            var rootCommand = new RootCommand("timrlink command line interface")
            {
                projectTimeCommand,
                taskCommand,
                exportProjectTimeCommand,
            };
            rootCommand.Name = "timrlink";
            rootCommand.TreatUnmatchedTokensAsErrors = true;

            return await rootCommand.InvokeAsync(args);
        }

        private async Task ImportProjectTime(string filename)
        {
            ImportAction action;
            switch (Path.GetExtension(filename))
            {
                case ".csv":
                    action = new ProjectTimeCSVImportAction(LoggerFactory, filename, TaskService, UserService, ProjectTimeService);
                    break;
                case ".xlsx":
                    action = new ProjectTimeXLSXImportAction(LoggerFactory, filename, TaskService, UserService, ProjectTimeService);
                    break;
                default:
                    throw new ArgumentException($"Unsupported file type '{filename}' - use .csv or .xlsx!");
            }

            await action.Execute();
        }

        private async Task ImportTasks(string filename, bool update)
        {
            await new TaskImportAction(LoggerFactory, filename, update, TaskService).Execute();
        }

        private async Task ExportProjectTime(string connectionString, string from, string to)
        {
            var context = new DatabaseContext(new DbContextOptionsBuilder()
                .UseSqlServer(connectionString)
                .Options);

            var pendingMigrations = (await context.Database.GetPendingMigrationsAsync()).ToList();
            if (pendingMigrations.Any())
            {
                var logger = LoggerFactory.CreateLogger<Program>();
                logger.LogInformation($"Running Database Migration... ({string.Join(", ", pendingMigrations)})");
                await context.Database.MigrateAsync();
            }

            await new ProjectTimeDatabaseExportAction(LoggerFactory, context, from: from, to: to, UserService,
                TaskService, ProjectTimeService).Execute();
        }

        private async Task ExportGroups(string connectionString)
        {
            await new GroupExportAction()
        }
    }
}
