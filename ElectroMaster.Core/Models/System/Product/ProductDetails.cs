using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectroMaster.Core.Models.System.Product
{
    public class ProductDetails
    {
        public string Name { get; set; }
        public Dictionary<string, object> Properties { get; set; }
    }
}
