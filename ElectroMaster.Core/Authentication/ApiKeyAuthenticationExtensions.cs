using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectroMaster.Core.Authentication
{
    public static class ApiKeyAuthenticationExtensions
    {
        public static AuthenticationBuilder AddApiKey(this AuthenticationBuilder builder, Action<ApiKeyAuthenticationOptions>? configureOptions = null)
            => AddApiKey(builder, ApiKeyAuthenticationDefaults.AuthenticationScheme, configureOptions);

        public static AuthenticationBuilder AddApiKey(this AuthenticationBuilder builder, string authenticationScheme, Action<ApiKeyAuthenticationOptions>? configureOptions = null)
        {
            return builder.AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(authenticationScheme, configureOptions);
        }
    }
}
