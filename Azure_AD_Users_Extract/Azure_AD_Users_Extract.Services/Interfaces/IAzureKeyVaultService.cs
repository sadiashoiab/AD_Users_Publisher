using System.Threading.Tasks;

namespace Azure_AD_Users_Extract.Services.Interfaces
{
    public interface IAzureKeyVaultService
    {
        Task<string> GetSecret(string name);
    }
}