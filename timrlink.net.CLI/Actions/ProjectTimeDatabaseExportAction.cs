using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using timrlink.net.Core.API;
using timrlink.net.Core.Service;
using Task = System.Threading.Tasks.Task;

namespace timrlink.net.CLI.Actions
{
    internal class ProjectTimeDatabaseExportAction
    {
        private readonly DatabaseContext context;
        private readonly ILogger<ProjectTimeDatabaseExportAction> logger;
        private readonly IProjectTimeService projectTimeService;
        private readonly ITaskService taskService;
        private readonly IUserService userService;

        public ProjectTimeDatabaseExportAction(ILoggerFactory loggerFactory, string connectionString, IUserService userService, ITaskService taskService, IProjectTimeService projectTimeService)
        {
            logger = loggerFactory.CreateLogger<ProjectTimeDatabaseExportAction>();
            context = new DatabaseContext(connectionString);
            this.userService = userService;
            this.taskService = taskService;
            this.projectTimeService = projectTimeService;
        }

        public async Task Execute()
        {
            logger.LogDebug("ProjectTimeDatabaseExportAction started");

            await context.Database.EnsureCreatedAsync();

            DateTime? lastProjectTimeImport = null;
            var metadata = await context.GetMetadata(Metadata.KEY_LAST_PROJECTTIME_IMPORT);
            if (DateTime.TryParseExact(metadata?.Value, "o", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                lastProjectTimeImport = date;
            }

            logger.LogInformation("Export project times with modifications since: " + lastProjectTimeImport);

            var importTime = DateTime.Now;
            var projectTimes = await projectTimeService.GetProjectTimes(lastModified: lastProjectTimeImport);

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

                    return new ProjectTime
                    {
                        UUID = Guid.Parse(pt.uuid),
                        User = user != null ? $"{user.lastname} {user.firstname}" : pt.userUuid,
                        StartTime = pt.startTime,
                        EndTime = pt.endTime,
                        Duration = pt.duration,
                        BreakTime = pt.breakTime,
                        Changed = pt.changed,
                        Closed = pt.closed,
                        StartPosition = LatLon(pt.startPosition),
                        EndPosition = LatLon(pt.endPosition),
                        LastModifiedTime = pt.lastModifiedTime,
                        Task = JsonConvert.SerializeObject(BuildTaskPath(pt.taskUuid, taskDict).Select(task => task.name).ToArray()),
                        Description = pt.description,
                        Billable = pt.billable
                    };
                });

                await context.ProjectTimes.AddOrUpdateRange(dbEntities);
                await context.SaveChangesAsync();
            }

            await context.SetMetadata(new Metadata(Metadata.KEY_LAST_PROJECTTIME_IMPORT, importTime.ToString("o", CultureInfo.InvariantCulture)));

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
