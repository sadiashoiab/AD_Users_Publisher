using System.Threading.Tasks;

namespace AD_Users_Extract.Services.Interfaces
{
    public interface ITokenService
    {
        Task<string> RetrieveToken(TokenEnum tokenEnum);
    }
}
