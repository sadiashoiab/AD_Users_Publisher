using System;

namespace Azure_AD_Users_Publisher.Services.Exceptions
{
    public class UnexpectedDataException : ApplicationException
    {
        public UnexpectedDataException(string parameterName) : base($"Unexpected Data: {parameterName}")
        {
        }

        public UnexpectedDataException(string parameterName, string value) : base($"Unexpected Data: {parameterName} with value: {value}")
        {
        }

        public UnexpectedDataException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}