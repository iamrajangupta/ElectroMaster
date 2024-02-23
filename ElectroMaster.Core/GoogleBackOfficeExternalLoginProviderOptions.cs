using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Web.BackOffice.Security;

namespace MyCustomUmbracoProject.ExternalUserLogin.GoogleAuthentication;

public class GoogleBackOfficeExternalLoginProviderOptions : IConfigureNamedOptions<BackOfficeExternalLoginProviderOptions>
{
    public const string SchemeName = "Google";

    public void Configure(string? name, BackOfficeExternalLoginProviderOptions options)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (name != Constants.Security.BackOfficeExternalAuthenticationTypePrefix + SchemeName)
        {
            return;
        }

        Configure(options);
    }

    public void Configure(BackOfficeExternalLoginProviderOptions options)
    {
        // Customize the login button
        options.Icon = "icon-google-fill";

        // The following options are only relevant if you
        // want to configure auto-linking on the authentication.
        options.AutoLinkOptions = new ExternalSignInAutoLinkOptions(
            // Set to true to enable auto-linking
            autoLinkExternalAccount: true,

            // [OPTIONAL]
            // Default: "Editor"
            // Specify User Group.
            defaultUserGroups: new[] { Constants.Security.EditorGroupAlias },

            // [OPTIONAL]
            // Default: The culture specified in appsettings.json.
            // Specify the default culture to create the User as.
            // It can be dynamically assigned in the OnAutoLinking callback.
            defaultCulture: null,

            // [OPTIONAL]
            // Enable the ability to link/unlink manually from within
            // the Umbraco backoffice.
            // Set this to false if you don't want the user to unlink
            // from this external login provider.
            allowManualLinking: true
        )
        {
            // [OPTIONAL] Callback
            OnAutoLinking = (user, loginInfo) =>
            {
                // You can customize the user before it's linked.
                // i.e. Modify the user's groups based on the Claims returned
                // in the externalLogin info
                var extClaim = externalLogin
                    .Principal
                    .FindFirst("MyClaim");
                user.Claims.Add(new IdentityUserClaim<string>
                {
                    ClaimType = extClaim.Type,
                    ClaimValue = extClaim.Value,
                    UserId = user.Id
                });
            },
            OnExternalLogin = (user, loginInfo) =>
            {
                // You can customize the user before it's saved whenever they have
                // logged in with the external provider.
                // i.e. Sync the user's name based on the Claims returned
                // in the externalLogin info
                var extClaim = externalLogin
                    .Principal
                    .FindFirst("MyClaim");
                user.Claims.Add(new IdentityUserClaim<string>
                {
                    ClaimType = extClaim.Type,
                    ClaimValue = extClaim.Value,
                    UserId = user.Id
                });
                return true;
            }
        };

        // [OPTIONAL]
        // Disable the ability for users to login with a username/password.
        // If set to true, it will disable username/password login
        // even if there are other external login providers installed.
        options.DenyLocalLogin = false;

        // [OPTIONAL]
        // Choose to automatically redirect to the external login provider
        // effectively removing the login button.
        options.AutoRedirectLoginToExternalProvider = false;
    }
}