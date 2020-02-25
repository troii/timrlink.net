using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using timrlink.net.Core.API;

namespace timrlink.net.Core.Service
{
    public class UserService : IUserService
    {
        private readonly ILogger<UserService> logger;
        private readonly TimrSync timrSync;

        public UserService(ILogger<UserService> logger, TimrSync timrSync)
        {
            this.logger = logger;
            this.timrSync = timrSync;
        }

        public async Task<List<User>> GetUsers()
        {
            var getUsersResponse = await timrSync.GetUsersAsync(new GetUsersRequest("")).ConfigureAwait(false);
            var users = getUsersResponse.GetUsersResponse1;

            logger.LogDebug($"User count: {users.Length}");
            return users.ToList();
        }
    }
}
