using System.Threading.Tasks;
using Azure_AD_Users_Publisher.Services.Models;
using Azure_AD_Users_Shared.Models;

namespace Azure_AD_Users_Publisher.Services
{
    public interface ITimeZoneService
    {
        Task<string> RetrieveTimeZone(SalesforceUser user);
    }
}