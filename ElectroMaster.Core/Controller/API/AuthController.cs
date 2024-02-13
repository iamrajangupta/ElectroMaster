using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Core.Security;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Web.Common.Security;
using Umbraco.Cms.Core.Services;
using ElectroMaster.Core.Models.System.Auth;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace ElectroMaster.Core.Controller.API
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : UmbracoApiController
    {
        private readonly IMemberSignInManager _signInManager;
        private readonly IMemberService _memberService;
        private readonly IMemberManager _memberManager;
      
        private readonly Guid _storeId = new Guid("1f0f0ae0-dcba-4b1c-8584-018cd87f4959");
        public AuthController(IMemberSignInManager signInManager, IMemberService memberService, IMemberManager memberManager)
        {
            _signInManager = signInManager;
            _memberService = memberService;
          
            _memberManager = memberManager;
        }

        [HttpPost("memberLogin")]
        public async Task<IActionResult> Login([FromBody] MemberLoginDto model)
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

        [HttpGet("memberDetail")]
        public async Task<IActionResult> GetMemberByEmail([FromQuery] string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest("Email address is required.");
            }
            var memberEmail = await _memberManager.FindByEmailAsync(email);
            var memberById = await _memberManager.FindByIdAsync(memberEmail.Id);
            IPublishedContent member = _memberManager.AsPublishedMember(memberById);

            if (member == null)
            {
                return NotFound("Member not found.");
            }

            var firstName = member.Value<string>("firstName");
            var lastName = member.Value<string>("lastName");
            var telephone = member.Value<string>("telephone");
            var addressLine1 = member.Value<string>("addressLine1");
            var addressLine2 = member.Value<string>("addressLine2");
            var zipCode = member.Value<string>("zipCode");
           
            var memberDetails = new
            {
                FirstName = firstName,
                LastName = lastName,
                Telephone = telephone,
                AddressLine1 = addressLine1,
                AddressLine2 = addressLine2,
                ZipCode = zipCode
            };

            return Ok(memberDetails);
        }       
    }
}
