using System.Threading.Tasks;

namespace Azure_AD_Users_Publisher.Services.Interfaces
{
    public interface ITokenService
    {
        Task<string> RetrieveToken();
    }

    // note: instead of creating a service resolver for ITokenService, just creating empty interfaces to keep it simple for now
    public interface IHISCTokenService : ITokenService
    {
    }

    public interface ISalesforceTokenService : ITokenService
    {
    }
}
