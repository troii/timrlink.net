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
        private string dateFormatToParse = "yyyy-MM-dd";

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

        public static DateTime? TryParse(string text) =>
            DateTime.TryParse(text, out var date) ? date : (DateTime?) null;
        
        public async Task Execute()
        {
            logger.LogDebug("ProjectTimeDatabaseExportAction started");
            
            DateTime? fromDate = TryParse(from);
            DateTime? toDate = TryParse(to);
            
            if (fromDate == null && toDate != null)
            {
                throw new ArgumentException("To date specified but no from date specified");
            }
            
            if (fromDate != null && toDate == null)
            {
                throw new ArgumentException("From date specified but no to date specified");
            }

            DateSpan? dateSpan = null;
            
            if (fromDate.HasValue && toDate.HasValue)
            {
                dateSpan = new DateSpan
                {
                    from = fromDate.Value,
                    to = toDate.Value
                };
            } 

            if (dateSpan != null && (dateSpan.from > dateSpan.to))
            {
                throw new ArgumentException("From date is after to date. Aborting.");
            }

            await context.Database.EnsureCreatedAsync();

            IList<Core.API.ProjectTime> projectTimes;
            
            var importTime = DateTime.Now;

            if (dateSpan == null)
            {
                var metadata = await context.GetMetadata(Metadata.KEY_LAST_PROJECTTIME_IMPORT);
                var lastProjectTimeImport = TryParse(metadata.Value);
                
                projectTimes = await projectTimeService.GetProjectTimes(null, null, lastProjectTimeImport);
            }
            else
            {
                logger.LogInformation("Export project times from: " + dateSpan.from + " to: " + dateSpan.to);
                projectTimes = await projectTimeService.GetProjectTimes(dateSpan.from, dateSpan.to, null);
            }
            
            var projectTimeUuids = projectTimes.Select(projectTime => Guid.Parse(projectTime.uuid));

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
                    projectTime.StartTime = pt.startTime;
                    projectTime.EndTime = pt.endTime;
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

            if (dateSpan != null)
            {
                // Flag records that are not found anymore Deleted.
                var projectTimesInDatabase = context.ProjectTimes.Where(projectTime =>
                        projectTime.StartTime >= dateSpan.from && projectTime.EndTime <= dateSpan.to)
                    .ToList();

                foreach (var projectTime in projectTimesInDatabase)
                {
                    if (!projectTimeUuids.Contains(projectTime.UUID))
                    {
                        projectTime.Deleted = importTime;
                    }
                }

                await context.SaveChangesAsync();
            }
            else 
            {
                await context.SetMetadata(new Metadata(Metadata.KEY_LAST_PROJECTTIME_IMPORT, importTime.ToString("o", CultureInfo.InvariantCulture)));
            }
            
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