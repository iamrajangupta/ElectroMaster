using Stripe.Checkout;
using Stripe;

namespace ElectroMaster.Core.Service
{
    public class StripeCheckoutService
    {
        public Session CreateCheckoutSession(string secretKey, decimal amount, string successUrl, string cancelUrl, string currency, string productName, int quantity)
        {
            var options = new SessionCreateOptions
            {
                // Configure your Stripe session options here
                LineItems = new List<SessionLineItemOptions>
            {
                // Add line items for the product(s) to purchase
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        // Populate with the required product details
                        UnitAmount = (long)(amount * 100), // Amount in cents
                        Currency = currency,
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = productName,
                        },
                    },
                    Quantity = quantity,
                },
            },
                PaymentMethodTypes = new List<string> { "card" },
                Mode = "payment",
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
            };

            StripeConfiguration.ApiKey = secretKey;

            var service = new SessionService();
            return service.Create(options);
        }
    }

}
