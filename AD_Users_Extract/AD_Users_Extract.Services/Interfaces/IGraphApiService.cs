using System.Threading.Tasks;

namespace AD_Users_Extract.Services.Interfaces
{
    public interface IGraphApiService
    {
        Task<string> AcquireToken(string clientId, string appKey, string tenant);
        Task<string> RetrieveData(string url, string token);
    }
}