using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;

namespace Azure_AD_Users_Publisher.Services
{
    public interface IMessageProcessor
    {
        Task ProcessMessage(ISubscriptionClient receiver, Message message, CancellationToken cancellationToken);
    }
}