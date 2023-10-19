using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using timrlink.net.Core.API;
using timrlink.net.Core.Service;

namespace timrlink.net.CLI.Actions
{
    internal abstract class ProjectTimeImportAction : ImportAction
    {
        protected ITaskService TaskService { get; }
        protected IUserService UserService { get; }
        protected IProjectTimeService ProjectTimeService { get; }

        protected ProjectTimeImportAction(ILogger logger, ITaskService taskService, IUserService userService, IProjectTimeService projectTimeService) : base(logger)
        {
            TaskService = taskService;
            UserService = userService;
            ProjectTimeService = projectTimeService;
        }

        public async System.Threading.Tasks.Task Execute(string filename)
        {
            IList<Core.API.ProjectTime> records;
            try
            {
                records = ParseFile(filename).ToList();
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Could not read passed file!");
                return;
            }

            Logger.LogInformation("found {Count} entries", records.Count);

            await ImportProjectTimeRecords(records);
        }

        protected abstract IEnumerable<Core.API.ProjectTime> ParseFile(string filename);

        private async System.Threading.Tasks.Task ImportProjectTimeRecords(IList<Core.API.ProjectTime> records)
        {
            var users = await UserService.GetUsers();
            var userDictionary = users
                .Where(user => user.externalId != null)
                .GroupBy(user => user.externalId)
                .ToDictionary(group => group.Key, group => group.ToList());

            var tasks = TaskService.FlattenTasks(await TaskService.GetTaskHierarchy());
            var taskTokenDictionary = tasks.ToTokenDictionary();

            foreach (var record in records)
            {
                var externalUserId = record.externalUserId;
                if (!userDictionary.TryGetValue(externalUserId, out var projectTimeUsers))
                {
                    Logger.LogWarning("User with ExternalId '{ExternalUserId}' not found. Skipping {Record} to import.", externalUserId, record);
                    continue;
                }
                else if (projectTimeUsers.Count > 1)
                {
                    Logger.LogError("User with ExternalId '{ExternalUserId}' not unique (logins=[{Logins}]). Skipping {Record} to import.", externalUserId, String.Join(", ", projectTimeUsers.Select(u => u.login)), record);
                    continue;
                }

                await TaskService.AddTaskTreeRecursive(null, record.externalTaskId.Split("|"), taskTokenDictionary, new Dictionary<string, Task>(), bookable: true);
                await ProjectTimeService.SaveProjectTime(record);
            }
        }
    }
}
