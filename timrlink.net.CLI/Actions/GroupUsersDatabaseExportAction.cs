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
        private readonly IGroupService groupService;

        public GroupUsersDatabaseExportAction(ILoggerFactory loggerFactory, DatabaseContext context, IGroupService groupService)
        {
            logger = loggerFactory.CreateLogger<ProjectTimeDatabaseExportAction>();
            this.context = context;
            this.groupService = groupService;
        }

        public async Task Execute()
        {
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

                await context.Group.AddOrUpdate(databaseGroup);
                await context.SaveChangesAsync();

                logger.LogInformation($"Created or updated Group with Name: {databaseGroup.Name}");
                
                groupDictionary.Remove(databaseGroup.Id);
                
                var groupUsers = await groupService.GetGroupUsers(group);
                
                var userDictionary = context.GroupUsers.Where(gu => gu.GroupId == databaseGroup.Id)
                    .ToDictionary(g => g.UserUUID, g => g);
                
                foreach (var user in groupUsers)
                {
                    var groupUser = context.GroupUsers.FirstOrDefault(gu => gu.GroupId == databaseGroup.Id && gu.UserUUID == user.uuid) ?? new GroupUsers();

                    groupUser.GroupId = databaseGroup.Id;
                    groupUser.UserUUID = user.uuid;

                    userDictionary.Remove(user.uuid);

                    await context.GroupUsers.AddOrUpdate(groupUser);
                    logger.LogInformation($"Created or updated GroupUsers with GroupID: {groupUser.GroupId} UserUUID: {groupUser.UserUUID}");
                }
                
                // Remove users that were removed from group in timr
                foreach (var userGroupToDelete in userDictionary.Values)
                {
                    context.Remove(userGroupToDelete);
                    logger.LogInformation($"Removed Group with GroupID: {userGroupToDelete.GroupId} UserUUID: {userGroupToDelete.UserUUID}");
                }
                
                await context.SaveChangesAsync();
            }

            // Remove groups that were deleted in timr related GroupUsers will be deleted automatically by EntityFramework
            foreach (var groupToDelete in groupDictionary.Values)
            {
                context.Remove(groupToDelete);
                logger.LogInformation($"Removed Group with Name: {groupToDelete.Name}");
            }

            await context.SaveChangesAsync();
        }
    }
}