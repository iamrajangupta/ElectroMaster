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
using Iconnect.Umbraco.Utils.Interfaces;
using Iconnect.Umbraco.Utils.Models;

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
        private readonly IMemberRegistrationService _memberRegistrationService;
        private readonly IEmailService _emailService;


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
       IMemberRegistrationService memberRegistrationService,
       IEmailService emailService, // Add IEmailService here
       ILogger<AuthSurfaceController> logger)
       : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
        {
            _memberManager = memberManager;
            _memberService = memberService;
            _scopeProvider = scopeProvider;
            _authenticationService = authenticationService;
            _memberTypeService = memberTypeService;
            _configuration = configuration;
            _memberRegistrationService = memberRegistrationService;
            _emailService = emailService; // Initialize _emailService here
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateUmbracoFormRouteString]
        public async Task<IActionResult> HandleRegisterMember([Bind(Prefix = "registerModel")] MemberRegisterDto model)
        {

            if (_memberService.GetByEmail(model.Email) != null)
            {
                TempData["emailTaken"] = "emailTaken";
                return CurrentUmbracoPage();
            }

            var registerModel = new RegisterMemberModelBase
            {
                UserName = model.Email,
                Email = model.Email,
                Password = model.Password,
                ConfirmPassword = model.ConfirmPassword,
                MemberTypeAlias = "Member",
                Name = model.FirstName + " " + model.LastName,
                MemberProperties = new List<MemberPropModel>
                {
                    new MemberPropModel { Alias = "firstName", Value = model.FirstName },
                    new MemberPropModel { Alias = "lastName", Value = model.LastName },
                    new MemberPropModel { Alias = "addressLine1", Value = model.AddressLine1 },
                    new MemberPropModel { Alias = "addressLine2", Value = model.AddressLine2 },
                    new MemberPropModel { Alias = "telephone", Value = model.Telephone },
                    new MemberPropModel { Alias = "zipCode", Value = model.ZipCode }
                }
            };

            var registerSuccess = await _memberRegistrationService.RegisterMemberAsync(registerModel);

            if (registerSuccess)
            {
                return CurrentUmbracoPage(); 
            }
            else
            {
                TempData["FormErrors"] = "Failed to register member.";
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
