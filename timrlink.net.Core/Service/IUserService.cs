using System.Collections.Generic;
using System.Threading.Tasks;
using timrlink.net.Core.API;

namespace timrlink.net.Core.Service
{
    public interface IUserService
    {
        Task<List<User>> GetUsers();
    }
}
