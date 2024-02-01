using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectroMaster.Core.Authentication
{
    public class ApiKeyAuthenticationDefaults
    {
        // 👇 This string identifies the authentication scheme for api keys. It can be anything you want.
        public const string AuthenticationScheme = "ApiKey";

        // 👇 This is the header from which the api key should be read
        public const string HeaderName = "apiKey";
    }
}
