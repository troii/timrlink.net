using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using timrlink.net.Core.API;
using timrlink.net.Core.Service;
using Task = System.Threading.Tasks.Task;

namespace timrlink.net.CLI.Actions
{
    public class ProjectTimeDatabaseExportAction
    {
        private readonly DatabaseContext context;
        private readonly ILogger<ProjectTimeDatabaseExportAction> logger;
        private readonly IProjectTimeService projectTimeService;
        private readonly ITaskService taskService;
        private readonly IUserService userService;
        private readonly string from;
        private readonly string to;
        private static string dateFormatToParse = "yyyy-MM-dd";

        public ProjectTimeDatabaseExportAction(ILoggerFactory loggerFactory, DatabaseContext context, string from, string to, IUserService userService, ITaskService taskService, IProjectTimeService projectTimeService)
        {
            logger = loggerFactory.CreateLogger<ProjectTimeDatabaseExportAction>();
            this.context = context;
            this.from = from;
            this.to = to;
            this.userService = userService;
            this.taskService = taskService;
            this.projectTimeService = projectTimeService;
        }

        public static DateTime? TryParse(string text, string format) => 
            DateTime.TryParseExact(text, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) ? date : (DateTime?) null;

        public async Task Execute()
        {
            logger.LogDebug("ProjectTimeDatabaseExportAction started");
            
            DateTime? fromDate = TryParse(from, dateFormatToParse);
            DateTime? toDate = TryParse(to, dateFormatToParse);
            
            DateSpan? dateSpan;
            
            if (fromDate.HasValue && toDate.HasValue)
            {
                if (fromDate.Value > toDate.Value)
                {
                    throw new ArgumentException("From date is after to date. Aborting.");
                }
                
                dateSpan = new DateSpan
                {
                    From = fromDate.Value,
                    To = toDate.Value
                };
            }
            else if (fromDate == null && toDate != null)
            {
                throw new ArgumentException("To date specified but no from date specified");
            }
            else if (fromDate != null)
            {
                throw new ArgumentException("From date specified but no to date specified");
            }
            else
            {
                dateSpan = null;
            }
            
            IList<Core.API.ProjectTime> projectTimes;
            // We don't migrate when in memory database is used, otherwise unit tests would fail.
            if (context.Database.IsRelational())
            {
                var pendingMigrations = (await context.Database.GetPendingMigrationsAsync()).ToList();
                if (pendingMigrations.Any())
                {
                    logger.LogInformation($"Running Database Migration... ({string.Join(", ", pendingMigrations)})");
                    context.Database.Migrate();
                }
            }
            
            var importTime = DateTime.Now;

            if (dateSpan.HasValue)
            {
                logger.LogInformation($"Export project times from: {dateSpan.Value.From} to: {dateSpan.Value.To}");
                projectTimes = await projectTimeService.GetProjectTimes(dateSpan.Value.From, dateSpan.Value.To, null);
            }
            else
            {
                var metadata = await context.GetMetadata(Metadata.KEY_LAST_PROJECTTIME_IMPORT);
                DateTime? lastProjectTimeImport = null;
                if (metadata != null)
                {
                    lastProjectTimeImport = TryParse(metadata.Value, "o");
                }

                logger.LogInformation($"Export project times with modifications since: {lastProjectTimeImport}");
                projectTimes = await projectTimeService.GetProjectTimes(null, null, lastProjectTimeImport);
            }
            
            
            var projectTimeUuids = projectTimes.Select(projectTime => Guid.Parse(projectTime.uuid)).ToList();

            if (projectTimes.Count > 0)
            {
                logger.LogInformation($"Exporting {projectTimes.Count} project times...");

                var userList = await userService.GetUsers();
                var userDict = userList.ToDictionary(u => u.uuid);

                var taskHierarchy = await taskService.GetTaskHierarchy();
                var taskList = taskService.FlattenTasks(taskHierarchy);
                // var taskDict = taskList.ToDictionary(t => t.uuid); // timr 4.16.x is currently affected by a bug returning Tasks duplicated
                var taskDict = taskList.GroupBy(t => t.uuid).ToDictionary(g => g.Key, g => g.First());
                logger.LogDebug($"Found {taskDict.Count} unique tasks");
                
                var dbEntities = projectTimes.Select(pt =>
                {
                    var user = userDict.GetValueOrDefault(pt.userUuid);

                    var projectTime = context.ProjectTimes
                        .FirstOrDefault(projectTime => projectTime.UUID.ToString() == pt.uuid) ?? new ProjectTime();
                    
                    projectTime.UUID = Guid.Parse(pt.uuid);
                    projectTime.User = user != null ? $"{user.lastname} {user.firstname}" : pt.userUuid;
                    projectTime.StartTime = pt.GetStartTimeOffset().DateTime;
                    projectTime.EndTime = pt.GetEndTimeOffset().DateTime;
                    projectTime.Duration = pt.duration;
                    projectTime.BreakTime = pt.breakTime;
                    projectTime.Changed = pt.changed;
                    projectTime.Closed = pt.closed;
                    projectTime.StartPosition = LatLon(pt.startPosition);
                    projectTime.EndPosition = LatLon(pt.endPosition);
                    projectTime.LastModifiedTime = pt.lastModifiedTime;
                    projectTime.Task =
                        JsonConvert.SerializeObject(BuildTaskPath(pt.taskUuid, taskDict).Select(task => task.name)
                            .ToArray());
                    projectTime.Description = pt.description;
                    projectTime.Billable = pt.billable;
                    projectTime.Deleted = null;

                    return projectTime;
                });
                
                await context.ProjectTimes.AddOrUpdateRange(dbEntities);    
            }
            
            if (dateSpan.HasValue)
            {
                // Flag records that are not found anymore Deleted. We add 1 day to To DateTime so project times on this
                // day are included
                var projectTimesInDatabase = context.ProjectTimes.Where(projectTime =>
                        projectTime.StartTime >= dateSpan.Value.From && projectTime.StartTime < dateSpan.Value.To.AddDays(1))
                    
                    .ToList();
                
                foreach (var projectTime in projectTimesInDatabase)
                {
                    if (!projectTimeUuids.Contains(projectTime.UUID))
                    {
                        projectTime.Deleted = importTime;
                    }
                }
            }
            else 
            {
                await context.SetMetadata(new Metadata(Metadata.KEY_LAST_PROJECTTIME_IMPORT, importTime.ToString("o", CultureInfo.InvariantCulture)));
            }
            
            await context.SaveChangesAsync();
            logger.LogDebug("ProjectTimeDatabaseExportAction finished");
        }

        private IEnumerable<Core.API.Task> BuildTaskPath(string taskUuid, Dictionary<string, Core.API.Task> tasks)
        {
            for (var task = tasks.GetValueOrDefault(taskUuid); task != null; task = task.parentUuid != null ? tasks.GetValueOrDefault(task.parentUuid) : null)
            {
                yield return task;
            }
        }

        private string LatLon(Position position)
        {
            return position != null ? $"{position.latitude},{position.longitude}" : null;
        }
    }
}