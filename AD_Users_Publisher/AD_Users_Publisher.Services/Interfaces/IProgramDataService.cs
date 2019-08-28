using System.Threading.Tasks;

namespace AD_Users_Publisher.Services.Interfaces
{
    public interface IProgramDataService
    {
        Task<int[]> RetrieveFranchises(ProgramDataSources source, string bearerToken);
    }
}