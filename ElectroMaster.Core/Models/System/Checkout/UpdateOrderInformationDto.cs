using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectroMaster.Core.Models.System.Checkout
{
    public class UpdateOrderInformationDto
    {
        public Guid OrderId { get; set; }

        public string Email { get; set; }

        public bool MarketingOptIn { get; set; }

        public AddressDto BillingAddress { get; set; }

        public AddressDto ShippingAddress { get; set; }

        public bool ShippingSameAsBilling { get; set; }

        public string Comments { get; set; }
      
    }

}
