using System.Collections.Generic;
using System.Threading.Tasks;
using AD_Users_Extract.Services.Models;

namespace AD_Users_Extract.Services.Interfaces
{
    public interface IUserService
    {
        Task<List<GraphUser>> GetUsers(string groupId, string token, int syncDurationInHours = 0);
    }
}
