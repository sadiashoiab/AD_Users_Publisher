using System.Threading.Tasks;

namespace Azure_AD_Users_Publisher.Services.Interfaces
{
    public interface ITokenService
    {
        Task<string> RetrieveToken();
    }
}
