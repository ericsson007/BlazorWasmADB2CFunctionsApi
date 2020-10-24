using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorWasm
{
    public class Settings
    {
        public ADB2CSettings AzureAdB2C { get; set; }

        public class ADB2CSettings
        {
            /// <summary>
            /// e.g. e.g. "https://mytestadb2c.b2clogin.com/mytestadb2c.onmicrosoft.com/B2C_1_susi", 
            /// template "https://{domain name prefix}.b2clogin.com/{domain name prefix}.onmicrosoft.com/{policy/userflow that was used to get the token}"
            /// </summary>
            public string Authority { get; set; }

            /// <summary>
            /// e.g. "0c109a26-9e4c-4e20-a1a4-676f34bad4e7",
            /// Application (client) ID fof the AD B2C Application you configured
            /// </summary>
            public string ClientId { get; set; }

            /// <summary>
            /// set to false...
            /// </summary>
            public bool ValidateAuthority { get; set; }

            /// <summary>
            /// e.g. "https://mytestadb2c.onmicrosoft.com/0c109a26-9e4c-4e20-a1a4-676f34bad4e7/Read", 
            /// fully qualified API / Permission name
            /// </summary>
            public string Scope { get; set; }

            /// <summary>
            /// e.g. http://localhost:7071, 
            /// base url of the Function with the web api
            /// </summary>
            public string SecureWebApiEndpoint { get; set; }
        }
    }
}
