using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Azure_AD_Users_Extract.Services.Models
{
    [ExcludeFromCodeCoverage]
    public class GraphRootObject
    {
        public IList<GraphUser> value { get; set; }
    }
}