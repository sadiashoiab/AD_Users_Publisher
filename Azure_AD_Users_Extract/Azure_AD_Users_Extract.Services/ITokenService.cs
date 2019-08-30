using System.Threading.Tasks;

namespace Azure_AD_Users_Extract.Services
{
    public interface ITokenService
    {
        Task<string> RetrieveToken(TokenEnum tokenEnum);
    }
}
