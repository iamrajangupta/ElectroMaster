using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectroMaster.Core.Models.System.Cart
{
    public class AddToCartDto
    {
        public string ProductReference { get; set; }
        public string ProductVariantReference { get; set; }
        public string ProductName { get; set; }
        public int ProductCount { get; set; }

    }
}
