using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Common.Security;
using Umbraco.Cms.Web.Website.Controllers;

namespace MemorialsGroundsCore.Controller
{
    public class AuthSurfaceController : SurfaceController
    {
        private readonly IMemberService _memberService;
        private readonly IMemberSignInManager _memberSignInManager;
        private readonly IMemberManager _memberManager;

        public AuthSurfaceController(
            IMemberManager memberManager,
            IMemberSignInManager memberSignInManager,
            IUmbracoContextAccessor umbracoContextAccessor,
            IUmbracoDatabaseFactory databaseFactory,
            ServiceContext services,
            AppCaches appCaches,
            IProfilingLogger profilingLogger,
            IPublishedUrlProvider publishedUrlProvider)
            : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
        {
            _memberSignInManager = memberSignInManager;
            _memberService = services.MemberService;
            _memberManager = memberManager;
        }

        public IActionResult GoogleSignIn()
        {
            // Redirect URL after Google authentication
            var redirectUrl = Url.Action(nameof(GoogleCallback), "AuthSurface", null, Request.Scheme);

            // Initiate Google authentication challenge
            return Challenge(new AuthenticationProperties { RedirectUri = redirectUrl }, GoogleDefaults.AuthenticationScheme);
        }

        public async Task<IActionResult> GoogleCallback()
        {
            // Retrieve the authentication result from the Google login
            var authenticateResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

            if (!authenticateResult.Succeeded)
            {
                // Handle authentication failure
                return RedirectToAction("Login");
            }

            // Extract necessary information from the authentication result
            var email = authenticateResult.Principal.FindFirst(ClaimTypes.Email)?.Value;

            // Check if the user already exists in the system
            var existingMember = _memberService.GetByEmail(email);
            if (existingMember == null)
            {
                return CurrentUmbracoPage();
            }

            var user = await _memberManager.FindByEmailAsync(email);

            // Sign in the user
            await _memberSignInManager.SignInAsync(user, false);

            // Redirect user to the dashboard or home page
            string returnUrl = "/cart";
            return Redirect(returnUrl);
        }
    }
}
