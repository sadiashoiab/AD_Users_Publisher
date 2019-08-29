using System.Threading.Tasks;
using Azure_AD_Users_Publisher.Services.Models;

namespace Azure_AD_Users_Publisher.Services.Interfaces
{
    public interface ISalesforceUserPublishService
    {
        Task Publish(SalesforceUser user);
    }
}