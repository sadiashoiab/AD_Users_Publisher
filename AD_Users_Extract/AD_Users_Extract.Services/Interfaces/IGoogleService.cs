using System.Threading.Tasks;
using AD_Users_Extract.Services.Models;

namespace AD_Users_Extract.Services.Interfaces
{
    public interface IGoogleService
    {
        Task<GoogleApiLocation> GeoCode(string streetAddress, string city, string state, string postalCode);
        Task<string> TimeZone(string latitude, string longitude);
    }
}