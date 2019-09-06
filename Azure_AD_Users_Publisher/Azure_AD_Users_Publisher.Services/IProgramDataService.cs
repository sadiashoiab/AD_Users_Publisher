using System.Threading.Tasks;

namespace Azure_AD_Users_Publisher.Services
{
    public interface IProgramDataService
    {
        Task<int[]> RetrieveFranchises(ProgramDataSources source, string bearerToken, bool cache = true);
    }
}