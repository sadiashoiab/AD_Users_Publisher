using System.Threading.Tasks;

namespace Azure_AD_Users_Publisher.Services.Interfaces
{
    public interface IProgramDataService
    {
        Task<int[]> RetrieveFranchises(ProgramDataSources source, string bearerToken);
    }
}