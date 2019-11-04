using System.Collections.Generic;
using System.Threading.Tasks;
using Azure_AD_Users_Publisher.Services.Models;

namespace Azure_AD_Users_Publisher.Services
{
    public interface ISalesforceTimeZoneManagerService
    {
        Task<IList<SalesforceSupportedTimeZone>> GetSupportedTimeZones();
    }
}