using System.Collections.Generic;
using System.Threading.Tasks;
using timrlink.net.Core.API;
using Task = System.Threading.Tasks.Task;

namespace timrlink.net.Core.Service
{
    public interface IGroupService
    {
        Task<IList<Group>> GetGroups();

        Task SetMissingExternalIds(IEnumerable<API.Group> groups);
        
        IList<Group> FlattenGroups(IEnumerable<API.Group> groups);

        Task<IList<User>> GetGroupUsers(Group group);
        
        Task SetExternalId(Group group);
    }
}