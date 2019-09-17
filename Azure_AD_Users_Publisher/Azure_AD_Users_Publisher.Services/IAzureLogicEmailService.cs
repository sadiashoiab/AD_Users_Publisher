using System.Threading.Tasks;

namespace Azure_AD_Users_Publisher.Services
{
    public interface IAzureLogicEmailService
    {
        Task SendAlert(string message);
    }
}