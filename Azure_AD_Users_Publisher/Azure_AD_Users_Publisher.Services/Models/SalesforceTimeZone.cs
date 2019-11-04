namespace Azure_AD_Users_Publisher.Services.Models
{
    public class SalesforceTimeZone
    {
        public int offsetSeconds { get; set; }
        public string offset { get; set; }
        public string name { get; set; }
        public string label { get; set; }
        public string key { get; set; }
        public bool isDefault { get; set; }
        public bool isActive { get; set; }
    }
}