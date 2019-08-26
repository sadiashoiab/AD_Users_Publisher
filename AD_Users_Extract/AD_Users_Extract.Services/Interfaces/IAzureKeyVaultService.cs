using System.Threading.Tasks;

namespace TestBed.Services.Interfaces
{
    public interface IAzureKeyVaultService
    {
        Task<string> GetSecret(string name);
    }
}