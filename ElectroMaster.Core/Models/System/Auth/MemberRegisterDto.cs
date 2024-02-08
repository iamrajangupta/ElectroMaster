using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Umbraco.Cms.Core.Models;
using DataType = System.ComponentModel.DataAnnotations.DataType;

namespace ElectroMaster.Core.Models.System.Auth
{
    public class MemberRegisterDto
    {
        public MemberRegisterDto()
        {
            UsernameIsEmail = true;
            MemberProperties = new List<MemberPropertyModel>();
        }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = null!;


        public List<MemberPropertyModel> MemberProperties { get; set; }

        [Editable(false)]
        public string MemberTypeAlias { get; set; }

        public string? Name { get; set; }

        [Required]
        [StringLength(256)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = null!;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = null!;

        public string? Username { get; set; }

        public bool UsernameIsEmail { get; set; }

        [DisplayName("First Name")]
        [Required(ErrorMessage = "Please enter your first name")]
        [RegularExpression(@"^[A-Za-z]+$", ErrorMessage = "Only alphabetic characters are allowed")]
        public string FirstName { get; set; }

        [DisplayName("Last Name")]
        [Required(ErrorMessage = "Please enter your last name")]
        [RegularExpression(@"^[A-Za-z]+$", ErrorMessage = "Only alphabetic characters are allowed")]
        public string LastName { get; set; }


        [DisplayName("Address Line 1")]
        [Required(ErrorMessage = "Please enter your Address Line 1")]
        public string AddressLine1 { get; set; }

        [DisplayName("Address Line 2")]
        public string? AddressLine2 { get; set; }

        [DisplayName("Telephone")]
        [Required(ErrorMessage = "Please enter your telephone no")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Please enter a valid contact number with only digits")]

        public string? Telephone { get; set; }

        [DisplayName("Zip Code")]
        public string? ZipCode { get; set; }
        public string? RedirectUrl { get; set; }

    }

}
