using System.Net.Http;
using System.Threading.Tasks;

namespace AD_Users_Publisher.Services
{
    public interface ITokenService
    {
        Task<string> RetrieveToken();
    }
}
