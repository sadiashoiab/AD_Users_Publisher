using System.Threading.Tasks;
using Azure_AD_Users_Shared.Models;

namespace Azure_AD_Users_Publisher.Services
{
    public interface IMessageProcessor
    {
        Task ProcessUser(AzureActiveDirectoryUser user);
    }
}