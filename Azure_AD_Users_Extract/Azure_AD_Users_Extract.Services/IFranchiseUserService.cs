using System.Collections.Generic;
using System.Threading.Tasks;
using Azure_AD_Users_Shared.Models;

namespace Azure_AD_Users_Extract.Services
{
    public interface IFranchiseUserService
    {
        Task<List<AzureActiveDirectoryUser>> GetFranchiseUsers(string groupId, int syncDurationInHours = 0);
        Task<List<AzureActiveDirectoryUser>> GetFranchiseDeactivatedUsers(int syncDurationInHours = 0);
    }
}