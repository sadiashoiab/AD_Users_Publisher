using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AD_Users_Publisher.Services
{
    public interface IAzureKeyVaultService
    {
        Task<string> GetSecret(string name);
    }
}
