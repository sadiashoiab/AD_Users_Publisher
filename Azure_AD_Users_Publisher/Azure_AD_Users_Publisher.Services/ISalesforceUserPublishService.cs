using System.Threading.Tasks;
using Azure_AD_Users_Shared.Models;

namespace Azure_AD_Users_Publisher.Services
{
    public interface ISalesforceUserPublishService
    {
        Task Publish(AzureActiveDirectoryUser user);
    }
}