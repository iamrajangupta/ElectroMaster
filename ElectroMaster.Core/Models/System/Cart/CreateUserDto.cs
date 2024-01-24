using ElectroMaster.Core.Models.System.Checkout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectroMaster.Core.Models.System.Cart
{
   
    public class CreateUserDto
    {
        public string Email { get; set; }

        public bool MarketingOptIn { get; set; }

        public Guid Country { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

    }
}
