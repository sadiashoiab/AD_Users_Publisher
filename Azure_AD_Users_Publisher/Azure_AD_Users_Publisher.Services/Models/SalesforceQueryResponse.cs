using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Azure_AD_Users_Publisher.Services.Models
{
    [ExcludeFromCodeCoverage]
    public class SalesforceQueryResponse
    {
        public int totalSize { get; set; }
        public bool done { get; set; }
        public IList<SalesforceQueryRecord> records { get; set; }
    }
}