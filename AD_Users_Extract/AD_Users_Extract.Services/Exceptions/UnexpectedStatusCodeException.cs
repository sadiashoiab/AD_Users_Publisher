﻿using System;
using System.Net.Http;

namespace AD_Users_Extract.Services.Exceptions
{
    public class UnexpectedStatusCodeException : ApplicationException
    {
        public UnexpectedStatusCodeException(HttpResponseMessage response) : base($"Unexpected Status Code: {response.StatusCode}, for Method: {response.RequestMessage?.Method}, with Uri: {response.RequestMessage?.RequestUri}")
        {
        }
    }
}