using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using timrlink.net.Core.Service;

namespace timrlink.net.CLI.Actions
{
    internal class GroupExportAction
    {
        private readonly DatabaseContext context;
        private readonly ILogger<ProjectTimeDatabaseExportAction> logger;
        private readonly IProjectTimeService projectTimeService;
        private readonly IUserService userService;
        private readonly IGroupService groupService;

        public GroupExportAction(ILoggerFactory loggerFactory, string connectionString, IUserService userService, IProjectTimeService projectTimeService, IGroupService groupService)
        {
            logger = loggerFactory.CreateLogger<ProjectTimeDatabaseExportAction>();
            context = new DatabaseContext(connectionString);
            this.userService = userService;
            this.groupService = groupService;
            this.projectTimeService = projectTimeService;
        }

        public async Task Execute()
        {
            var groups = await groupService.GetGroups();
            var flatGroups = groupService.FlattenGroups(groups);
            
            var users = await userService.GetUsers();
            
            foreach (var group in flatGroups.Where(g => String.IsNullOrEmpty(g.externalId)))
            {
                group.externalId = Guid.NewGuid().ToString();
                groupService.UpdateGroup(group);
            }

            groups = await groupService.GetGroups();
            flatGroups = groupService.FlattenGroups(groups);
            
            foreach (var group in flatGroups)
            {
                var databaseGroup = context.Group
                    .FirstOrDefault(g => g.ExternalId == group.externalId);

                databaseGroup.Description = group.description;
                databaseGroup.ExternalId = group.externalId;
                databaseGroup.Name = group.name;
                databaseGroup.ParentalExternalId = group.parentExternalId;
                
                var groupUsers = await groupService.GetGroupUsers(group);

                foreach (var user in groupUsers)
                {
                    var groupUser = new GroupUsers
                    {
                        GroupId = group.externalId,
                        UserUUID = user.uuid
                    };

                    context.Update(groupUser);
                }
            }

            await context.SaveChangesAsync();
        }
    }
}