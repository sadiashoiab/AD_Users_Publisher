using System.Threading.Tasks;

namespace AD_Users_Extract.Services.Interfaces
{
    public interface IGoogleService
    {
        Task<string> Geocode(string streetAddress, string city, string state, string postalCode);
        Task<string> TimeZone(string latitude, string longitude);
    }
}