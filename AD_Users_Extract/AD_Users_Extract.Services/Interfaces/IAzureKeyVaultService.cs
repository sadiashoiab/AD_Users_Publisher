using System.Threading.Tasks;

namespace AD_Users_Extract.Services.Interfaces
{
    public interface IAzureKeyVaultService
    {
        Task<string> GetSecret(string name);
    }
}