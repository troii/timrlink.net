using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
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
            
            var exportGroupsTimeCommand = new Command("export-groups", "Export Groups (Organizations)");
            exportGroupsTimeCommand.AddOption(new Option<string>("connectionstring"));
            exportGroupsTimeCommand.Handler = CommandHandler.Create<string>(ExportGroups);

            var rootCommand = new RootCommand("timrlink command line interface")
            {
                projectTimeCommand,
                taskCommand,
                exportProjectTimeCommand,
                exportGroupsTimeCommand,
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
                    action = new ProjectTimeCSVImportAction(LoggerFactory, filename, TaskService, UserService,
                        ProjectTimeService);
                    break;
                case ".xlsx":
                    action = new ProjectTimeXLSXImportAction(LoggerFactory, filename, TaskService, UserService,
                        ProjectTimeService);
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
            await InitializeDatabase(context);

            await new ProjectTimeDatabaseExportAction(LoggerFactory, context, from, to, UserService, TaskService,
                ProjectTimeService).Execute();
        }

        private async Task ExportGroups(string connectionString)
        {
            var context = new DatabaseContext(new DbContextOptionsBuilder()
                .UseSqlServer(connectionString)
                .Options);
            await InitializeDatabase(context);

            await new GroupUsersDatabaseExportAction(LoggerFactory, context, GroupService).Execute();
        }

        private async Task InitializeDatabase(DatabaseContext context)
        {
            var pendingMigrations = (await context.Database.GetPendingMigrationsAsync()).ToList();

            if (pendingMigrations.Any())
            {
                const string efMigrationsHistoryTable = "__EFMigrationsHistory";
                
                bool migrationExists;
                await using (var command = context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = '{efMigrationsHistoryTable}';";
                    await context.Database.OpenConnectionAsync();

                    var result = Convert.ToInt32(await command.ExecuteScalarAsync());
                    migrationExists = result > 0;
                }

                bool tablesExist;
                await using (var command = context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND (TABLE_NAME = 'ProjectTimes' OR TABLE_NAME = 'Metadata');";
                    await context.Database.OpenConnectionAsync();
                    
                    var result = Convert.ToInt32(await command.ExecuteScalarAsync());
                    tablesExist = result > 0;
                }
                
                if (tablesExist && !migrationExists)
                {
                    Logger.LogInformation("Database already has existing tables before running migrations, marking initial migration as already done");

                    // When tables already exist but no migration exists we know that database was created without 
                    // migrations. So we insert the first migration 20221020122606_InitialMigration manually
                    // Then we can switch to migrations managed by Entity Framework
                    var firstMigrationName = pendingMigrations.First();
                    var version = Assembly.GetAssembly(typeof(DbContext)).GetName().Version;
                    if (firstMigrationName != null && version != null)
                    {
                        var versionString = $"{version.Major}.{version.Minor}.{version.Build}";

                        await context.Database.ExecuteSqlRawAsync($"CREATE TABLE {efMigrationsHistoryTable}(MigrationId nvarchar(150) NOT NULL CONSTRAINT PK___EFMigrationsHistory PRIMARY KEY, ProductVersion nvarchar(32) NOT NULL);");
                        await context.Database.ExecuteSqlRawAsync(
                            $"INSERT INTO {efMigrationsHistoryTable}(MigrationId, ProductVersion) VALUES ('{firstMigrationName}', '{versionString}')");
                    }
                }
                
                // Run the remaining migrations
                pendingMigrations = (await context.Database.GetPendingMigrationsAsync()).ToList();
                if (pendingMigrations.Any())
                {
                    Logger.LogInformation("Running Database Migration... ({pendingMigrations})",
                        string.Join(", ", pendingMigrations));
                    await context.Database.MigrateAsync();
                }
            }
        }
    }
}
