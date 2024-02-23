using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Web.BackOffice.Security;

namespace ElectroMaster.Core
{
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
                OnAutoLinking = (BackOfficeIdentityUser autoLinkUser, ExternalLoginInfo loginInfo) =>
                {
                    // Customize the user before it's linked.
                    // Modify the User's groups based on the Claims returned
                    // in the external login info.
                    var extClaims = loginInfo.Principal.Claims;
                    foreach (var claim in extClaims)
                    {
                        autoLinkUser.Claims.Add(new IdentityUserClaim<string>
                        {
                            ClaimType = claim.Type,
                            ClaimValue = claim.Value,
                            UserId = autoLinkUser.Id
                        });
                    }
                    autoLinkUser.IsApproved = true;
                },
                OnExternalLogin = (BackOfficeIdentityUser user,ExternalLoginInfo loginInfo) =>
                {
                    // Customize the User before it is saved whenever they have
                    // logged in with the external provider.
                    // Sync the Users name based on the Claims returned
                    // in the external login info.

                    var extClaims = loginInfo.Principal.Claims;
                    foreach (var claim in extClaims)
                    {
                        user.Claims.Add(new IdentityUserClaim<string>
                        {
                            ClaimType = claim.Type,
                            ClaimValue = claim.Value,
                            UserId = user.Id
                        });
                    }
                    user.IsApproved = true;

                    // Returns a boolean indicating if sign-in should continue or not.
                    return true;
                },
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
}
