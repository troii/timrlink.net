using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using timrlink.net.CLI.Extensions;
using timrlink.net.Core.API;
using timrlink.net.Core.Service;
using Task = System.Threading.Tasks.Task;

namespace timrlink.net.CLI.Actions
{
    /// <summary>
    /// Fetch project times and write to database
    /// </summary>
    public class ProjectTimeDatabaseExportAction
    {
        private readonly DatabaseContext context;
        private readonly ILogger<ProjectTimeDatabaseExportAction> logger;
        private readonly IProjectTimeService projectTimeService;
        private readonly ITaskService taskService;
        private readonly IUserService userService;
        private readonly string from;
        private readonly string to;
        private const string dateFormatToParse = "yyyy-MM-dd";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="loggerFactory">Logger</param>
        /// <param name="context">Database context</param>
        /// <param name="from">From date in the format: 'YYYY-MM-DD' example: 2022-10-24</param>
        /// <param name="to">To date in the format: 'YYYY-MM-DD' example: 2022-10-25</param>
        /// <param name="userService">User service</param>
        /// <param name="taskService">Task service</param>
        /// <param name="projectTimeService">Project time service</param>
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

        private static DateTime? TryParse(string text, string format) => 
            DateTime.TryParseExact(text, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) ? date : (DateTime?) null;

        /// <summary>
        /// Fetches project times in the given timespan (from, to) - to is inclusive so project times on this day are also included
        /// Project times that are not found anymore (can be deleted or moved) are marked as with a deleted timestamp
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public async Task Execute()
        {
            logger.LogDebug("ProjectTimeDatabaseExportAction started");
            
            var fromDate = TryParse(from, dateFormatToParse);
            var toDate = TryParse(to, dateFormatToParse);
            
            DateSpan? dateSpan;
            
            if (fromDate.HasValue && toDate.HasValue)
            {
                if (fromDate.Value > toDate.Value)
                {
                    throw new ArgumentException("From date is after to date. Aborting.");
                }
                
                dateSpan = new DateSpan(fromDate.Value, toDate.Value);
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
            
            var importTime = DateTime.Now;

            if (dateSpan.HasValue)
            {
                logger.LogInformation("Export project times from: {From} to: {To}", dateSpan.Value.From, dateSpan.Value.To);
                projectTimes = await projectTimeService.GetProjectTimes(dateSpan.Value.From, dateSpan.Value.To);
            }
            else
            {
                var metadata = await context.GetMetadata(Metadata.KEY_LAST_PROJECTTIME_IMPORT);
                DateTime? lastProjectTimeImport = null;
                if (metadata != null)
                {
                    lastProjectTimeImport = TryParse(metadata.Value, "o");
                }

                logger.LogInformation("Export project times with modifications since: {LastProjectTimeImport}", lastProjectTimeImport);
                projectTimes = await projectTimeService.GetProjectTimes(null, null, lastProjectTimeImport);
            }
            
            
            var projectTimeUuids = projectTimes.Select(projectTime => Guid.Parse(projectTime.uuid)).ToList();

            if (projectTimes.Count > 0)
            {
                logger.LogInformation("Exporting {ProjectTimeCount} project times...", projectTimes.Count);

                var userList = await userService.GetUsers();
                var userDict = userList.ToDictionary(u => u.uuid);

                var taskHierarchy = await taskService.GetTaskHierarchy();
                var taskList = taskService.FlattenTasks(taskHierarchy);
                // var taskDict = taskList.ToDictionary(t => t.uuid); // timr 4.16.x is currently affected by a bug returning Tasks duplicated
                var taskDict = taskList.GroupBy(t => t.uuid).ToDictionary(g => g.Key, g => g.First());
                logger.LogDebug("Found {TaskCount} unique tasks", taskDict.Count);
                
                var dbEntities = projectTimes.Select(pt =>
                {
                    var user = userDict.GetValueOrDefault(pt.userUuid);

                    var projectTime = context.ProjectTimes
                        .FirstOrDefault(projectTime => projectTime.UUID.ToString() == pt.uuid) ?? new ProjectTime();
                    
                    projectTime.UUID = Guid.Parse(pt.uuid);
                    projectTime.User = user != null ? $"{user.lastname} {user.firstname}" : pt.userUuid;
                    projectTime.UserUUID = Guid.Parse(pt.userUuid);
                    projectTime.UserExternalId = pt.externalUserId;
                    projectTime.StartTime = pt.GetStartTimeOffset();
                    projectTime.EndTime = pt.GetEndTimeOffset();
                    projectTime.Duration = pt.duration;
                    projectTime.BreakTime = pt.breakTime;
                    projectTime.Changed = pt.changed;
                    projectTime.Closed = pt.closed;
                    projectTime.StartPosition = LatLon(pt.startPosition);
                    projectTime.EndPosition = LatLon(pt.endPosition);
                    projectTime.LastModifiedTime = pt.GetLastModifiedOffset();
                    projectTime.Task =
                        JsonConvert.SerializeObject(BuildTaskPath(pt.taskUuid, taskDict).Select(task => task.name)
                            .ToArray());
                    projectTime.TaskUUID = Guid.Parse(pt.taskUuid);
                    projectTime.TaskExternalId = pt.externalTaskId;
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
            IEnumerable<Core.API.Task> ReversedTaskPath()
            {
                for (var task = tasks.GetValueOrDefault(taskUuid); task != null; task = task.parentUuid != null ? tasks.GetValueOrDefault(task.parentUuid) : null)
                {
                    yield return task;
                }
            }

            return ReversedTaskPath().Reverse();
        }

        private string LatLon(Position position)
        {
            return position != null ? $"{position.latitude},{position.longitude}" : null;
        }
    }
}