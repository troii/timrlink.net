using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using timrlink.net.CLI.Actions;
using timrlink.net.Core;
using timrlink.net.Core.Service;

[assembly: InternalsVisibleTo("timrlink.net.CLI.Test")]
namespace timrlink.net.CLI
{
    public static class Program
    {
        public static Task<int> Main(string[] args)
        {
            return BuildCommandLine()
                .UseHost(builder =>
                {
                    builder.ConfigureDefaults(args);
                    builder.ConfigureServices(services =>
                    {
                        services.AddLogging();
                        services.AddTimrLink();
                    });
                    builder.ConfigureLogging(logging =>
                    {
                        logging.AddSimpleConsole(options =>
                        {
                            options.SingleLine = true;
                            options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffzzz\t";
                        });
                    });
                })
                .Build()
                .InvokeAsync(args);
        }

        private static CommandLineBuilder BuildCommandLine()
        {
            var filenameArgument = new Argument<string>("filename");
            filenameArgument.LegalFilePathsOnly();

            var projectTimeCommand = new Command("projecttime", "Import project times");
            projectTimeCommand.AddAlias("pt");
            projectTimeCommand.AddArgument(filenameArgument);
            projectTimeCommand.Handler = CommandHandler.Create<IHost, string>(ImportProjectTime);

            var updateTasks = new Option<bool>("--update", "Update existing tasks with same externalId");
            updateTasks.AddAlias("-u");
            updateTasks.SetDefaultValue(true);

            var taskCommand = new Command("task", "Import tasks");
            taskCommand.AddAlias("t");
            taskCommand.AddArgument(filenameArgument);
            taskCommand.AddOption(updateTasks);
            taskCommand.Handler = CommandHandler.Create<IHost, string, bool>(ImportTasks);

            var exportProjectTimeCommand = new Command("export-projecttime", "Export Project times");
            exportProjectTimeCommand.AddOption(new Option<string>("connectionstring"));
            exportProjectTimeCommand.AddOption(new Option<string>("from"));
            exportProjectTimeCommand.AddOption(new Option<string>("to"));
            exportProjectTimeCommand.Handler = CommandHandler.Create<IHost, string, string, string>(ExportProjectTime);

            var exportGroupsTimeCommand = new Command("export-groups", "Export Groups (Organizations)");
            exportGroupsTimeCommand.AddOption(new Option<string>("connectionstring"));
            exportGroupsTimeCommand.Handler = CommandHandler.Create<IHost, string>(ExportGroups);

            var rootCommand = new RootCommand("timrlink command line interface")
            {
                projectTimeCommand,
                taskCommand,
                exportProjectTimeCommand,
                exportGroupsTimeCommand,
            };
            rootCommand.Name = "timrlink";
            rootCommand.TreatUnmatchedTokensAsErrors = true;

            return new CommandLineBuilder(rootCommand);
        }

        private static async Task ImportProjectTime(IHost host, string filename)
        {
            var loggerFactory = host.Services.GetService<ILoggerFactory>();
            var taskService = host.Services.GetService<ITaskService>();
            var userService = host.Services.GetService<IUserService>();
            var projectTimeService = host.Services.GetService<IProjectTimeService>();

            ProjectTimeImportAction action = Path.GetExtension(filename) switch
            {
                ".csv" => new ProjectTimeCSVImportAction(loggerFactory.CreateLogger<ProjectTimeCSVImportAction>(), taskService, userService, projectTimeService),
                ".xlsx" => new ProjectTimeXLSXImportAction(loggerFactory.CreateLogger<ProjectTimeXLSXImportAction>(), taskService, userService, projectTimeService),
                _ => throw new ArgumentException($"Unsupported file type '{filename}' - use .csv or .xlsx!")
            };

            await action.Execute(filename);
        }

        private static async Task ImportTasks(IHost host, string filename, bool update)
        {
            var loggerFactory = host.Services.GetService<ILoggerFactory>();
            var taskService = host.Services.GetService<ITaskService>();
            await new TaskImportAction(loggerFactory.CreateLogger<TaskImportAction>(), taskService).Execute(filename, update);
        }

        private static async Task ExportProjectTime(IHost host, string connectionString, string from, string to)
        {
            var logger = host.Services.GetService<ILogger>();
            var loggerFactory = host.Services.GetService<ILoggerFactory>();
            var userService = host.Services.GetService<IUserService>();
            var taskService = host.Services.GetService<ITaskService>();
            var projectTimeService = host.Services.GetService<IProjectTimeService>();

            var context = new DatabaseContext(new DbContextOptionsBuilder()
                .UseSqlServer(connectionString)
                .Options);
            await context.InitializeDatabase( logger);

            await new ProjectTimeDatabaseExportAction(
                loggerFactory.CreateLogger<ProjectTimeDatabaseExportAction>(),
                context,
                userService,
                taskService,
                projectTimeService
            ).Execute(from, to);
        }

        private static async Task ExportGroups(IHost host, string connectionString)
        {
            var logger = host.Services.GetService<ILogger>();
            var loggerFactory = host.Services.GetService<ILoggerFactory>();
            var groupService = host.Services.GetService<IGroupService>();

            var context = new DatabaseContext(new DbContextOptionsBuilder()
                .UseSqlServer(connectionString)
                .Options);
            await context.InitializeDatabase(logger);

            await new GroupUsersDatabaseExportAction(loggerFactory, context, groupService).Execute();
        }
    }
}
