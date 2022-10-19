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
            var getGroupsResponse = await timrSync.GetGroupsAsync(new GetGroupsRequest1(new API.GetGroupsRequest())).ConfigureAwait(false);
            
            var groups = getGroupsResponse.GetGroupsResponse1;
            logger.LogDebug($"Total user count: {groups.Length}");
            
            return groups;
        }

        IList<API.Group> IGroupService.FlattenGroups(IEnumerable<API.Group> groups)
        {
            return GroupService.FlattenGroups(groups);
        }

        internal static IList<API.Group> FlattenGroups(IEnumerable<API.Group> groups)
        {
            return groups.SelectMany(group =>
            {
                var list = new List<API.Group> { group };
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

        public async Task UpdateGroup(Group group)
        {
            logger.LogInformation($"Updating Group(Name={group.name}, ExternalId={group.externalId})");
            timrSync.UpdateGroupAsync(new UpdateGroupRequest(group)).ConfigureAwait(false);
        }
    }
}