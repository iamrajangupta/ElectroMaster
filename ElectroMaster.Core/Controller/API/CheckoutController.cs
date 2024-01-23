using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Common;
using Umbraco.Commerce.Core.Api;
using ElectroMaster.Core.Models.System.Checkout;
using Umbraco.Commerce.Core;
using Umbraco.Commerce.Core.Models;
using Umbraco.Commerce.Core.Services;
using Stripe.Checkout;
using Stripe;
using System.Globalization;
using Umbraco.Commerce.Extensions;


namespace ElectroMaster.Core.Controller.API
{
    [ApiController]
    [Route("api/checkout")]
    public class CheckoutController : UmbracoApiController
    {
        private readonly IUmbracoCommerceApi _commerceApi;
        private readonly IPaymentService _paymentService;
        private readonly Guid _storeId = new Guid("1f0f0ae0-dcba-4b1c-8584-018cd87f4959");
        public CheckoutController(UmbracoHelper umbracoHelper, ServiceContext services, IUmbracoCommerceApi commerceApi, IPaymentService paymentService)
        {
            _commerceApi = commerceApi;

            _paymentService = paymentService;
        }

        Order updatedOrder = null;

        [HttpPost("updateinfromation")]
        public IActionResult UpdateOrderInformation(UpdateOrderInformationDto model)
        {
            try
            {

                _commerceApi.Uow.Execute(uow =>
                {
                    var order = _commerceApi.GetOrCreateCurrentOrder(_storeId)
                        .AsWritable(uow)
                        .SetProperties(new Dictionary<string, string>
                        {
                            { Constants.Properties.Customer.EmailPropertyAlias, model.Email },
                            { "marketingOptIn", model.MarketingOptIn ? "1" : "0" },
                            { Constants.Properties.Customer.FirstNamePropertyAlias, model.BillingAddress.FirstName },
                            { Constants.Properties.Customer.LastNamePropertyAlias, model.BillingAddress.LastName },
                            { "billingAddressLine1", model.BillingAddress.Line1 },
                            { "billingAddressLine2", model.BillingAddress.Line2 },
                            { "billingCity", model.BillingAddress.City },
                            { "billingZipCode", model.BillingAddress.ZipCode },
                            { "billingTelephone", model.BillingAddress.Telephone.ToString() },

                            { "shippingSameAsBilling", model.ShippingSameAsBilling ? "1" : "0" },
                            { "shippingFirstName", model.ShippingSameAsBilling ? model.BillingAddress.FirstName : model.ShippingAddress.FirstName },
                            { "shippingLastName", model.ShippingSameAsBilling ? model.BillingAddress.LastName : model.ShippingAddress.LastName },
                            { "shippingAddressLine1", model.ShippingSameAsBilling ? model.BillingAddress.Line1 : model.ShippingAddress.Line1 },
                            { "shippingAddressLine2", model.ShippingSameAsBilling ? model.BillingAddress.Line2 : model.ShippingAddress.Line2 },
                            { "shippingCity", model.ShippingSameAsBilling ? model.BillingAddress.City : model.ShippingAddress.City },
                            { "shippingZipCode", model.ShippingSameAsBilling ? model.BillingAddress.ZipCode : model.ShippingAddress.ZipCode },
                            { "shippingTelephone", model.ShippingSameAsBilling ? model.BillingAddress.Telephone.ToString() : model.ShippingAddress.Telephone.ToString() },

                            { "comments", model.Comments }
                        })
                        .SetPaymentCountryRegion(model.BillingAddress.Country, null)
                        .SetShippingCountryRegion(model.ShippingSameAsBilling ? model.BillingAddress.Country : model.ShippingAddress.Country, null);

                    _commerceApi.SaveOrder(order);

                    uow.Complete();
                });

            }
            catch (System.ComponentModel.DataAnnotations.ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok();
        }

        [HttpGet("shippingmethods")]
        public IActionResult GetShippingMethods()
        {
            try
            {
                var shippingMethods = _commerceApi.GetShippingMethods(_storeId);

                if (shippingMethods != null && shippingMethods.Any())
                {
                    return Ok(shippingMethods);
                }
                else
                {
                    return NotFound("No shipping methods found for the specified store.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpPost("shippingmethods")]
        public IActionResult UpdateOrderShippingMethod(UpdateOrderShippingMethodDto model)
        {
            try
            {
                _commerceApi.Uow.Execute(uow =>
                {
                    var order = _commerceApi.GetOrCreateCurrentOrder(_storeId)
                        .AsWritable(uow)
                        .SetShippingMethod(model.ShippingMethod);

                    _commerceApi.SaveOrder(order);
                    updatedOrder = order;
                    uow.Complete();
                });

            }
            catch (System.ComponentModel.DataAnnotations.ValidationException ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok(updatedOrder);
        }


        [HttpGet("paymentmethods")]
        public IActionResult GetPaymentMethods()
        {
            try
            {

                var paymentMethods = _commerceApi.GetPaymentMethods(_storeId);

                if (paymentMethods != null && paymentMethods.Any())
                {
                    return Ok(paymentMethods);
                }
                else
                {
                    return NotFound("No payment methods found for the specified store.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("paymentmethods")]

        public IActionResult UpdateOrderPaymentMethod(UpdateOrderPaymentMethodDto model)
        {
            try
            {

                _commerceApi.Uow.Execute(uow =>
                {
                    var order = _commerceApi.GetOrCreateCurrentOrder(_storeId)
                        .AsWritable(uow)

                        .SetPaymentMethod(model.PaymentMethod);

                    _commerceApi.SaveOrder(order);

                    updatedOrder = order;

                    uow.Complete();
                });

                return Ok(updatedOrder);
            }
            catch (System.ComponentModel.DataAnnotations.ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("CreateCheckoutSession")]
        public IActionResult CreateCheckoutSession([FromBody] Guid orderID)
        {
            try
            {
                var Order = CommerceApi.Instance.GetOrder(orderID);

                if (Order != null)
                {
                    var price = Order.TotalPrice.WithoutAdjustments.Formatted().ToString();
                    var orderNo = Order.OrderNumber;

                    decimal amountDecimal;
                    if (decimal.TryParse(price.Substring(1), NumberStyles.Currency, CultureInfo.CurrentCulture, out amountDecimal))
                    {
                        // Convert to cents (integer)
                        long amountInCents = (long)(amountDecimal * 100);


                        var options = new SessionCreateOptions
                        {
                            LineItems = new List<SessionLineItemOptions>
                            {
                                new SessionLineItemOptions
                                {
                                    PriceData = new SessionLineItemPriceDataOptions
                                    {
                                        UnitAmount = amountInCents,
                                        Currency = "GBP",
                                        ProductData = new SessionLineItemPriceDataProductDataOptions
                                        {
                                            Name = orderNo,
                                        },
                                    },
                                    Quantity = 1,
                                },
                            },
                            PaymentMethodTypes = new List<string> { "card" },
                            Mode = "payment",
                            SuccessUrl = $"{Request.Scheme}://{Request.Host}/cart/success?session_id={{CHECKOUT_SESSION_ID}}",
                            CancelUrl = $"{Request.Scheme}://{Request.Host}/cart/"
                        };

                        StripeConfiguration.ApiKey = "sk_test_51NN9SASCduWSbaBPQNIs7V75kRLkaLOnIQEWGBXpYv7b8yc64Yz8ljMx6fZ8tFjQCkuAV69sNWDYfbDbmkgMFLVS00FCtszGCz";
                        var service = new SessionService();
                        var session = service.Create(options);

                        // Return the session URL
                        return Ok(new { SessionUrl = session.Url });
                    }
                    else
                    {
                        // Handle parsing error
                        return BadRequest(new { ErrorMessage = "Failed to parse amount", ErrorDetails = "Invalid amount format" });
                    }
                }
                return Ok();
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                return BadRequest(new { ErrorMessage = "An error occurred", ErrorDetails = ex.Message });
            }
        }

        [HttpPost("/cart/success")]
        public IActionResult PaymentSuccess([FromBody] PaymentIntent paymentIntent)
        {
            try
            {
                // Verify the payment intent using your Stripe secret key
                var service = new PaymentIntentService();
                var retrievedIntent = service.Get(paymentIntent.Id);

                if (retrievedIntent.Status == "succeeded")
                {
                    // Payment was successful
                    // Update order status, send push notification, etc.
                    UpdateOrderStatus(retrievedIntent.Metadata["orderID"]);
                    SendPushNotification();

                    return Ok(new { Message = "Payment successful" });
                }
                else
                {
                    // Handle unsuccessful payment
                    return BadRequest(new { ErrorMessage = "Payment failed", ErrorDetails = "Payment intent not succeeded" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { ErrorMessage = "An error occurred", ErrorDetails = ex.Message });
            }
        }

        private void UpdateOrderStatus(string orderID)
        {
            // Add logic to update the order status in your database
            // For example, set the order status to "paid"
            // This depends on your database structure and design
        }

        private void SendPushNotification()
        {
            // Add logic to send a push notification to the Flutter app
            // This depends on your implementation using Firebase Cloud Messaging (FCM) or another push notification service
        }

    }
}