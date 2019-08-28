using System.Collections.Generic;

namespace Azure_AD_Users_Extract.Services.Models
{
    public class GraphRootObject
    {
        public IList<GraphUser> value { get; set; }
    }
}