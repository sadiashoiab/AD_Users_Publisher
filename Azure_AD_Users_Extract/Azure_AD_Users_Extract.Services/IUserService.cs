using System.Collections.Generic;
using System.Threading.Tasks;
using Azure_AD_Users_Shared.Models;

namespace Azure_AD_Users_Extract.Services
{
    public interface IUserService
    {
        Task<List<AzureActiveDirectoryUser>> GetUsers(string groupId, string token, int syncDurationInHours = 0);
        Task<List<AzureActiveDirectoryUser>> GetDeactivatedUsers(string token, int syncDurationInHours = 0);
    }
}
