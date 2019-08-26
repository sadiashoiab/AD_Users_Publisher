using System;
using System.Threading.Tasks;
using AD_Users_Extract.Services.Interfaces;

namespace AD_Users_Extract.Services
{
    public class GoogleService : IGoogleService
    {
        public Task<string> Geocode(string streetAddress, string city, string state, string postalCode)
        {
            throw new NotImplementedException();
        }

        public Task<string> TimeZone(string latitude, string longitude)
        {
            throw new NotImplementedException();
        }
    }
}
