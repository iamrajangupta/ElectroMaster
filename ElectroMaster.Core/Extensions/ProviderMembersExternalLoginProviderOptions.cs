using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Security;

namespace ElectroMaster.Core.Extensions
{
    public class ProviderMembersExternalLoginProviderOptions : IConfigureNamedOptions<MemberExternalLoginProviderOptions>
    {
        public const string SchemeName = "OpenIdConnect";

        public void Configure(string? name, MemberExternalLoginProviderOptions options)
        {
            if (name != Constants.Security.MemberExternalAuthenticationTypePrefix + SchemeName)
            {
                return;
            }

            Configure(options);
        }

        public void Configure(MemberExternalLoginProviderOptions options)
        {
            options.AutoLinkOptions = new MemberExternalSignInAutoLinkOptions(
                autoLinkExternalAccount: true,
                defaultCulture: null,
                defaultIsApproved: true,
                defaultMemberTypeAlias: Constants.Security.DefaultMemberTypeAlias
            )
            {
                OnAutoLinking = (autoLinkUser, loginInfo) =>
                {
                    // Customize the member before linking
                },
                OnExternalLogin = (user, loginInfo) =>
                {
                    // Customize the member before saving
                    return true;
                }
            };
        }
    }
}
