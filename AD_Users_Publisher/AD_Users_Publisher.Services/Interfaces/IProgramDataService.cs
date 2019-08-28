using System.Threading.Tasks;

namespace AD_Users_Publisher.Services.Interfaces
{
    public interface IProgramDataService
    {
        Task<string> RetrieveFranchises(ProgramDataSources sources);
    }
}