using System.Threading.Tasks;
using Azure_AD_Users_Publisher.Services.Models;

namespace Azure_AD_Users_Publisher.Services.Interfaces
{
    public interface ITimeZoneService
    {
        Task<string> RetrieveTimeZone(SalesforceUser user);
    }
}