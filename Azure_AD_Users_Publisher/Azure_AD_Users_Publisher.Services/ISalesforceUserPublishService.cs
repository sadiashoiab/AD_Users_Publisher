using System.Threading.Tasks;
using Azure_AD_Users_Publisher.Services.Models;

namespace Azure_AD_Users_Publisher.Services
{
    public interface ISalesforceUserService
    {
        int ErrorCount { get; }

        int PublishCount { get; }

        int DeactivationCount { get; }

        Task Publish(SalesforceUser user);
        Task Deactivate(string externalId);
    }
}