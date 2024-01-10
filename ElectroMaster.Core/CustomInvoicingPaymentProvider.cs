using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Core.PaymentProviders;

namespace ElectroMaster.Core
{
    public class CustomInvoicingPaymentProvider : InvoicingPaymentProvider
    {
        public CustomInvoicingPaymentProvider(UmbracoCommerceContext ctx) : base(ctx)
        {
        }

        public override string GetContinueUrl(PaymentProviderContext context)
        {
            // Ensure a valid ContinueUrl is returned here
            return "/checkout/success"; // Replace with your actual URL
        }
    }
}
