using System.Threading.Tasks;
using Azure_AD_Users_Publisher.Services.Models;
using Azure_AD_Users_Shared.Models;

namespace Azure_AD_Users_Publisher.Services
{
    public interface ISalesforceUserPublishService
    {
        int ErrorCount { get; set; }

        int PublishCount { get; set; }

        Task Publish(SalesforceUser user);
        Task DeactivateUser(AzureActiveDirectoryUser user);
    }
}