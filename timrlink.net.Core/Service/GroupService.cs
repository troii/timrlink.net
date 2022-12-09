using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using timrlink.net.Core.API;
using Task = System.Threading.Tasks.Task;

namespace timrlink.net.Core.Service
{
    public class GroupService : IGroupService
    {
        private readonly ILogger<GroupService> logger;
        private readonly TimrSync timrSync;

        public GroupService(ILogger<GroupService> logger, TimrSync timrSync)
        {
            this.logger = logger;
            this.timrSync = timrSync;
        }

        public async Task<IList<Group>> GetGroups()
        {
            var getGroupsResponse = await timrSync.GetGroupsAsync(new GetGroupsRequest1(new GetGroupsRequest())).ConfigureAwait(false);
            
            var groups = getGroupsResponse.GetGroupsResponse1;
            logger.LogDebug($"Total group count: {groups.Count()}");
            
            return groups;
        }

        async Task IGroupService.SetMissingExternalIds(IEnumerable<Group> groups)
        {
            await SetMissingExternalIds(groups, null);
        }

        private async Task SetMissingExternalIds(IEnumerable<Group> groups, string parentExternalId)
        {
            foreach (var group in groups)
            {
                group.parentExternalId = parentExternalId;
                if (String.IsNullOrEmpty(group.externalId))
                {
                    group.externalId = Guid.NewGuid().ToString();
                    await SetExternalId(group);
                }
                
                if (group.subgroups != null)
                {
                    await SetMissingExternalIds(group.subgroups, group.externalId);
                }
            }
        }
        
        IList<Group> IGroupService.FlattenGroups(IEnumerable<API.Group> groups)
        {
            return GroupService.FlattenGroups(groups);
        }

        private static IList<Group> FlattenGroups(IEnumerable<API.Group> groups)
        {
            return groups.SelectMany(group =>
            {
                var list = new List<Group> {group};
                
                if (group.subgroups != null)
                {
                    list.AddRange(FlattenGroups(group.subgroups));
                }

                return list;
            }).ToList();
        }

        public async Task<IList<User>> GetGroupUsers(Group group)
        {
            var getGroupUsersResponse = await timrSync.GetGroupUsersAsync(new GetGroupUsersRequest(group.externalId))
                .ConfigureAwait(false);
            
            var users = getGroupUsersResponse.GetGroupUsersResponse1;
            
            logger.LogDebug($"Total users: {users.Length} in group: {group.name}");
            return users;
        }

        public async Task SetExternalId(Group group)
        {
            var setGroupExternalIdRequest = new SetGroupExternalIdRequest1(
                new SetGroupExternalIdRequest
            {
                name = group.name,
                parentExternalId = group.parentExternalId,
                newExternalGroupId = group.externalId
            });
            
            logger.LogInformation($"Set externalId: {group.externalId} for group with name: {group.name}");
            await timrSync.SetGroupExternalIdAsync(setGroupExternalIdRequest)
                .ConfigureAwait(false);
        }
    }
}