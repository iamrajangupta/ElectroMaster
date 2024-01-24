using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectroMaster.Core.Models.System.Checkout
{
    public class UpdateOrderPaymentMethodDto
    {
        public Guid PaymentMethod { get; set; }

        public Guid OrderId { get; set; } = Guid.Empty;

    }
}
