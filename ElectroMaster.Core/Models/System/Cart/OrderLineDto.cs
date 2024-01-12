using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectroMaster.Core.Models.System.Cart
{
    public class OrderLineDto
    {
        public Guid OrderLineId { get; set; }
        public string ProductReference { get; set; } 
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal PriceExclTax { get; set; }
        public decimal Tax { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total { get; set; }

    }
}
