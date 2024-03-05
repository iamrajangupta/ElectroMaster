using ElectroMaster.Core.Models.System.Auth;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Common.Filters;
using Umbraco.Cms.Web.Common.Security;
using Umbraco.Cms.Web.Website.Controllers;
using Microsoft.AspNetCore.Authentication.Facebook;

namespace MemorialsGroundsCore.Controller
{
    public class AuthSurfaceController : SurfaceController
    {
        private readonly IMemberManager _memberManager;
        private readonly IMemberService _memberService;
        private readonly ICoreScopeProvider _scopeProvider;       
        private readonly IMemberTypeService _memberTypeService;
        private readonly IConfiguration _configuration;
        private readonly IAuthenticationService _authenticationService;


        public AuthSurfaceController(
        IConfiguration configuration,
        IMemberManager memberManager,
        IMemberService memberService,
        ICoreScopeProvider scopeProvider,   
        IMemberTypeService memberTypeService,
        IUmbracoContextAccessor umbracoContextAccessor,
        IUmbracoDatabaseFactory databaseFactory,
        ServiceContext services,
        AppCaches appCaches,
        IProfilingLogger profilingLogger,
        IPublishedUrlProvider publishedUrlProvider,
         IAuthenticationService authenticationService,
        ILogger<AuthSurfaceController> logger)
    : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
        {
            _memberManager = memberManager;
            _memberService = memberService;
            _scopeProvider = scopeProvider;
            _authenticationService = authenticationService;           
            _memberTypeService = memberTypeService;
            _configuration = configuration;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateUmbracoFormRouteString]
        public async Task<IActionResult> HandleRegisterMember([Bind(Prefix = "registerModel")] MemberRegisterDto model)
        {

            using (ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true))
            {
                var memberType = _memberTypeService.Get("Member");

                if (_memberService.GetByEmail(model.Email) != null)
                {
                    TempData["emailTaken"] = "emailTaken";
                    return CurrentUmbracoPage();
                }

                model.Username = model.UsernameIsEmail || model.Username == null ? model.Email : model.Username;
                model.Name = model.FirstName + " " + model.LastName;

                var identityUser = MemberIdentityUser.CreateNew(model.Username, model.Email, model.MemberTypeAlias, true, model.Name);

                IdentityResult identityResult = await _memberManager.CreateAsync(identityUser, model.Password);

                if (identityResult.Succeeded)
                {
                    IMember? member = _memberService.GetByKey(identityUser.Key);

                    if (member == null)
                    {
                        TempData["membernotfound"] = "membernotfound";
                        return CurrentUmbracoPage();
                    }

                    foreach (MemberPropertyModel property in model.MemberProperties.Where(p => p.Value != null))
                    {
                        if (member.Properties.Contains(property.Alias))
                        {
                            member.Properties[property.Alias]?.SetValue(property.Value);
                        }
                    }
                    if (memberType != null)
                    {
                        member.SetValue("firstName", model.FirstName);
                        member.SetValue("lastName", model.LastName);
                        member.SetValue("addressLine1", model.AddressLine1);
                        member.SetValue("addressLine2", model.AddressLine2);
                        member.SetValue("telephone", model.Telephone);
                        member.SetValue("zipCode", model.ZipCode);

                        _memberService.Save(member);
                        return CurrentUmbracoPage();

                    }
                    else
                    {
                        TempData["FormErrors"] = "Failed to retrieve member type.";
                    }
                }
                else
                {
                    // Handle errors related to user registration
                    foreach (IdentityError? error in identityResult.Errors)
                    {
                        ModelState.AddModelError("registerModel", error.Description);
                    }
                }
                return CurrentUmbracoPage();
            }
        }
        public async Task<IActionResult> GoogleSignIn()
        {

            var redirectUrl = Url.Action(nameof(GoogleCallback), "AuthSurface", null, Request.Scheme);
            return Challenge(new AuthenticationProperties { RedirectUri = redirectUrl }, GoogleDefaults.AuthenticationScheme);
        }

        public async Task<IActionResult> GoogleCallback()
        {
            var success = await _authenticationService.SignInWithGoogle(HttpContext, "Member");
            if (!success)
            {              
                return RedirectToAction("/my-account");
            }

            // Redirect user to the dashboard or home page
            string returnUrl = "/my-account";
            return Redirect(returnUrl);
        }

        public async Task<IActionResult> FacebookSignIn()
        {
            var redirectUrl = Url.Action(nameof(FacebookCallback), "AuthSurface", null, Request.Scheme) + "/umbraco-provider-signin-facebook";
            return Challenge(new AuthenticationProperties { RedirectUri = redirectUrl }, FacebookDefaults.AuthenticationScheme);
        }

        public async Task<IActionResult> FacebookCallback()
        {
            var success = await _authenticationService.SignInWithFacebook(HttpContext, "Member");
            if (!success)
            {
                return RedirectToAction("/my-account");
            }

            // Redirect user to the dashboard or home page
            string returnUrl = "/my-account";
            return Redirect(returnUrl);
        }
    }
}



