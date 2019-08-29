namespace Azure_AD_Users_Publisher.Services.Models
{
    public class SalesforcePublishResponse
    {
        public SalesforcePublishError error { get; set; }
        public SalesforcePublishData data { get; set; }
    }
}