using System.Threading.Tasks;

namespace Azure_AD_Users_Extract.Services
{
    public interface IGraphApiService
    {
        Task<string> AcquireToken(string clientId, string appKey, string tenant);
        Task<string> RetrieveData(string url, string token);
    }
}