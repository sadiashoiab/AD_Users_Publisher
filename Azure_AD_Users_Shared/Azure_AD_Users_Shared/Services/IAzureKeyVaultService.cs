using System.Threading.Tasks;

namespace Azure_AD_Users_Shared.Services
{
    public interface IAzureKeyVaultService
    {
        Task<string> GetSecret(string name);
    }
}