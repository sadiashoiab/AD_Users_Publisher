using System.Threading.Tasks;

namespace TestBed.Services.Interfaces
{
    public interface IGraphApiService
    {
        Task<string> AcquireToken(string clientId, string appKey, string tenant);
        Task<string> RetrieveData(string url, string token);
    }
}