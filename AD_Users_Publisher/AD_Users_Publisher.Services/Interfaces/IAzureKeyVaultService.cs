using System.Threading.Tasks;

namespace AD_Users_Publisher.Services.Interfaces
{
    public interface IAzureKeyVaultService
    {
        Task<string> GetSecret(string name);
    }
}
