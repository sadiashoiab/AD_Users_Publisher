using System.Threading.Tasks;
using Azure_AD_Users_Publisher.Services.Models;

namespace Azure_AD_Users_Publisher.Services
{
    public interface ISalesforceUserPublishService
    {
        int ErrorCount { get; set; }

        int PublishCount { get; set; }

        int DeactivationCount { get; set; }

        Task Publish(SalesforceUser user);
        Task Deactivate(string externalId);
    }
}