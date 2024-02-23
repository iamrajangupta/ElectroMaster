using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyCustomUmbracoProject.ExternalUserLogin.GoogleAuthentication;

namespace ElectroMaster.Core.Extensions;

public static class GoogleAuthenticationExtensions
{
    public static IUmbracoBuilder AddGoogleAuthentication(this IUmbracoBuilder builder)
    {
        // Register ProviderBackOfficeExternalLoginProviderOptions here rather than require it in startup
        builder.Services.ConfigureOptions<GoogleBackOfficeExternalLoginProviderOptions>();

        builder.AddBackOfficeExternalLogins(logins =>
        {
            logins.AddBackOfficeLogin(
                backOfficeAuthenticationBuilder =>
                {
                    // The scheme must be set with this method to work for the back office
                    var schemeName =
                        backOfficeAuthenticationBuilder.SchemeForBackOffice(GoogleBackOfficeExternalLoginProviderOptions
                            .SchemeName);

                    ArgumentNullException.ThrowIfNull(schemeName);

                    backOfficeAuthenticationBuilder.AddGoogle(
                        schemeName,
                        options =>
                        {
                            // Callback path: Represents the URL to which the browser should be redirected to.
                            // The default value is '/signin-google'.
                            // The value here should match what you have configured in you external login provider.
                            // The value needs to be unique.
                            options.CallbackPath = "/umbraco-google-signin";
                            options.ClientId = "750763726057-r58rm7r2oi1ffuogh4hsoe91tol818nj.apps.googleusercontent.com"; // Replace with your client id generated while creating OAuth client ID
                            options.ClientSecret = "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"; // Replace with your client secret generated while creating OAuth client ID

                        });



                });
        });
        return builder;
    }

}