using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;

namespace ElectroMaster.Core.Extensions
{
    public static class ProviderMemberAuthenticationExtensions
    {
        public static IUmbracoBuilder AddProviderMemberAuthentication(this IUmbracoBuilder builder)
        {
            builder.Services.ConfigureOptions<ProviderMembersExternalLoginProviderOptions>();

            builder.AddMemberExternalLogins(logins =>
            {
                logins.AddMemberLogin(memberAuthenticationBuilder =>
                {
                    memberAuthenticationBuilder.AddGoogle(
                        memberAuthenticationBuilder.SchemeForMembers(ProviderMembersExternalLoginProviderOptions.SchemeName),
                        options =>
                        {
                            options.CallbackPath = "/umbraco-google-signin";
                            options.ClientId = "750763726057-r58rm7r2oi1ffuogh4hsoe91tol818nj.apps.googleusercontent.com"; // Replace with your client id generated while creating OAuth client ID
                            options.ClientSecret = "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"; // Replace with your client secret generated while creating OAuth client ID
                            options.SaveTokens = true;
                        });
                });
            });

            return builder;
        }
    }
}
