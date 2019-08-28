using System.ComponentModel;

namespace AD_Users_Publisher.Services
{
    public enum ProgramDataSources
    {
        Invalid = 0,

        [Description("crmsalesforce")]
        Salesforce,

        [Description("clearcare")]
        ClearCare
    }
}