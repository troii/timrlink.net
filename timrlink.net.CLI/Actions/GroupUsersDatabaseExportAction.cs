using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using timrlink.net.Core.Service;

namespace timrlink.net.CLI.Actions
{
    internal class GroupUsersDatabaseExportAction
    {
        private readonly DatabaseContext context;
        private readonly ILogger<ProjectTimeDatabaseExportAction> logger;
        private readonly IUserService userService;
        private readonly IGroupService groupService;

        public GroupUsersDatabaseExportAction(ILoggerFactory loggerFactory, DatabaseContext context, IUserService userService, IGroupService groupService)
        {
            logger = loggerFactory.CreateLogger<ProjectTimeDatabaseExportAction>();
            this.context = context;
            this.userService = userService;
            this.groupService = groupService;
        }

        public async Task Execute()
        {
            await context.Database.EnsureCreatedAsync();
            
            var groups = await groupService.GetGroups();
            await groupService.SetMissingExternalIds(groups);
            
            var allDatabaseGroups = context.Group.ToList();
            var groupDictionary = allDatabaseGroups.ToDictionary(g => g.Id, g => g);
            
            groups = await groupService.GetGroups();
            var flatGroups = groupService.FlattenGroups(groups);
            
            foreach (var group in flatGroups)
            {
                var databaseGroup = context.Group
                    .FirstOrDefault(g => g.ExternalId == group.externalId) ?? new Group();
                
                databaseGroup.Description = group.description;
                databaseGroup.ExternalId = group.externalId;
                databaseGroup.Name = group.name;
                databaseGroup.ParentalExternalId = group.parentExternalId;

                context.Update(databaseGroup);
                await context.SaveChangesAsync();

                logger.LogInformation($"Created or updated Group with Name: {databaseGroup.Name}");
                
                groupDictionary.Remove(databaseGroup.Id);
                
                var groupUsers = await groupService.GetGroupUsers(group);
                var groupUsersDatabase = new List<GroupUsers>();
                
                foreach (var user in groupUsers)
                {
                    var groupUser = context.GroupUsers.FirstOrDefault(gu => gu.GroupId == databaseGroup.Id && gu.UserUUID == user.uuid) ?? new GroupUsers();

                    groupUser.GroupId = databaseGroup.Id;
                    groupUser.UserUUID = user.uuid;

                    groupUsersDatabase.Add(groupUser);
                    logger.LogInformation($"Created or updated GroupUsers with GroupID: {groupUser.GroupId} UserUUID: {groupUser.UserUUID}");
                }

                await context.GroupUsers.AddOrUpdateRange(groupUsersDatabase);
                await context.SaveChangesAsync();
            }

            // Remove groups that were deleted in timr
            foreach (var groupToDelete in groupDictionary.Values)
            {
                context.Remove(groupToDelete);
                logger.LogInformation($"Removed Group with Name: {groupToDelete.Name}");
            }

            await context.SaveChangesAsync();
        }
    }
}