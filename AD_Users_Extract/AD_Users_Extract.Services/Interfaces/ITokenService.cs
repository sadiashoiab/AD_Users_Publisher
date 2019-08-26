using System.Threading.Tasks;

namespace TestBed.Services.Interfaces
{
    public interface ITokenService
    {
        Task<string> RetrieveToken(TokenEnum tokenEnum);
    }
}
