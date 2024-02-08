using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Web.Common.Attributes;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Web.Common.Security;
using Umbraco.Cms.Web.Common.Filters;
using Umbraco.Cms.Core.Services;

namespace ElectroMaster.Core.Controller.API
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : UmbracoApiController
    {
        private readonly IMemberSignInManager _signInManager;
        private readonly IMemberService _memberService;


        public AuthController(IMemberSignInManager signInManager, IMemberService memberService)
        {
            _signInManager = signInManager;
            _memberService = memberService;
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var member = _memberService.GetByUsername(model.Email);
            if (member == null)
            {
                return NotFound("User not found");
            }

            var result = await _signInManager.PasswordSignInAsync(member.Username, model.Password, model.RememberMe, true);
            if (result.Succeeded)
            {
                // Return member details along with success message
                return Ok(new
                {
                    Message = "Login successful",
                    Member = new
                    {
                        Id = member.Id,
                        Username = member.Username,
                    }
                });
            }

            if (result.IsLockedOut)
            {
                return BadRequest("Account is locked out");
            }
            else if (result.IsNotAllowed)
            {
                return BadRequest("Account is not allowed to sign in");
            }

            return Unauthorized("Invalid username or password");
        }


        public class LoginModel
        {
            public string Email { get; set; }
            public string Password { get; set; }
            public bool RememberMe { get; set; }
        }
    }
}
