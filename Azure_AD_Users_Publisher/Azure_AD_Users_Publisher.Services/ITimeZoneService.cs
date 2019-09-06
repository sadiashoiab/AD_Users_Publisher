using System.Threading.Tasks;
using Azure_AD_Users_Shared.Models;

namespace Azure_AD_Users_Publisher.Services
{
    public interface ITimeZoneService
    {
        Task<string> RetrieveTimeZoneAndPopulateUsersCountryCode(AzureActiveDirectoryUser user);
    }
}