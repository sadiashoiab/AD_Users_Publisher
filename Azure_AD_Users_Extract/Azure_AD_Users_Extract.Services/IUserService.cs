using System.Collections.Generic;
using System.Threading.Tasks;
using Azure_AD_Users_Shared.Models;

namespace Azure_AD_Users_Extract.Services
{
    public interface IUserService
    {
        Task<List<SalesforceUser>> GetUsers(string groupId, string token, int syncDurationInHours = 0);
    }
}
