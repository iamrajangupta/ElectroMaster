using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectroMaster.Core.Models.System.Cart
{
    public class RemoveFromCartDto
    {
        public Guid OrderLineId { get; set; }
    }
}
