using System.Collections.Generic;
using System.Threading.Tasks;
using Azure_AD_Users_Extract.Services.Models;

namespace Azure_AD_Users_Extract.Services.Interfaces
{
    public interface IUserService
    {
        Task<List<GraphUser>> GetUsers(string groupId, string token, int syncDurationInHours = 0);
    }
}
