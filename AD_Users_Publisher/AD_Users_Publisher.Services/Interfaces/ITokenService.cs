using System.Threading.Tasks;

namespace AD_Users_Publisher.Services.Interfaces
{
    public interface ITokenService
    {
        Task<string> RetrieveToken();
    }
}
