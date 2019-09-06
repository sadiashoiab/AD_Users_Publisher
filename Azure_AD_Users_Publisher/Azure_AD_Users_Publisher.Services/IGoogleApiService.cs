﻿using System.Threading.Tasks;
using Azure_AD_Users_Publisher.Services.Models;

namespace Azure_AD_Users_Publisher.Services
{
    public interface IGoogleApiService
    {
        Task<GoogleApiLocation> GeoCode(string streetAddress, string city, string state, string postalCode);
        Task<GoogleApiTimeZone> TimeZone(GoogleApiLocation location);
    }
}
