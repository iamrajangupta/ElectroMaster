using ElectroMaster.Core.Models.System.Checkout;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Website.Controllers;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Core;
using ElectroMaster.Core.Extensions;
using Stripe.Checkout;
using Stripe;
using Umbraco.Commerce.Core.Models;
using System.Globalization;
using Umbraco.Commerce.Extensions;


namespace ElectroMaster.Core.Controller
{
    public class CheckoutSurfaceController : SurfaceController
    {
        private readonly IUmbracoCommerceApi _commerceApi;
        private readonly string _stripeSecretKey = "sk_test_51NN9SASCduWSbaBPQNIs7V75kRLkaLOnIQEWGBXpYv7b8yc64Yz8ljMx6fZ8tFjQCkuAV69sNWDYfbDbmkgMFLVS00FCtszGCz";

        public CheckoutSurfaceController(IUmbracoContextAccessor umbracoContextAccessor, IUmbracoDatabaseFactory databaseFactory,
            ServiceContext services, AppCaches appCaches, IProfilingLogger profilingLogger, IPublishedUrlProvider publishedUrlProvider,
            IUmbracoCommerceApi commerceApi)
            : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
        {
            _commerceApi = commerceApi;
        }



        public IActionResult UpdateOrderInformation(UpdateOrderInformationDto model)
        {
            try
            {
                var store = CurrentPage.GetStore();
                _commerceApi.Uow.Execute(uow =>
                {
                    var order = _commerceApi.GetOrCreateCurrentOrder(store.Id)
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
                string returnUrl = "/shipping-method/";
                return Redirect(returnUrl);
            }
            catch (System.ComponentModel.DataAnnotations.ValidationException ex)
            {
                ModelState.AddModelError("", "Failed to update information");

                return CurrentUmbracoPage();
            }

        }

        [HttpPost]
        public IActionResult UpdateOrderShippingMethod(UpdateOrderShippingMethodDto model)
        {
            try
            {
                var store = CurrentPage.GetStore();
                _commerceApi.Uow.Execute(uow =>
                {
                    var order = _commerceApi.GetOrCreateCurrentOrder(store.Id)
                        .AsWritable(uow)
                        .SetShippingMethod(model.ShippingMethod);

                    _commerceApi.SaveOrder(order);

                    uow.Complete();
                });
                string returnUrl = "/payment-method/";
                return Redirect(returnUrl);
            }
            catch (System.ComponentModel.DataAnnotations.ValidationException ex)
            {
                ModelState.AddModelError("", "Failed to set order shipping method");

                return CurrentUmbracoPage();
            }
        }


        public IActionResult UpdateOrderPaymentMethod(UpdateOrderPaymentMethodDto model)
        {
            try
            {
                _commerceApi.Uow.Execute(uow =>
                {
                    var store = CurrentPage.GetStore();
                    var order = _commerceApi.GetOrCreateCurrentOrder(store.Id)
                        .AsWritable(uow)
                        .SetPaymentMethod(model.PaymentMethod);

                    _commerceApi.SaveOrder(order);

                    uow.Complete();
                });
                string returnUrl = "/review-order/";
                return Redirect(returnUrl);
            }
            catch (System.ComponentModel.DataAnnotations.ValidationException ex)
            {
                ModelState.AddModelError("", "Failed to set order payment method");

                return CurrentUmbracoPage();
            }

        }

        [HttpPost]
        public IActionResult CreateCheckoutSession(decimal amount, string productName, string orderId, int quantity)
        {
            try
            {
                var options = new SessionCreateOptions
                {
                    LineItems = new List<SessionLineItemOptions>
                    {
                        new SessionLineItemOptions
                        {
                            PriceData = new SessionLineItemPriceDataOptions
                            {
                                UnitAmount = (long)(amount * 100),
                                Currency = "GBP",
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
                    SuccessUrl = $"{Request.Scheme}://{Request.Host}/cart/success?session_id={{CHECKOUT_SESSION_ID}}&orderId={orderId}",
                    CancelUrl = $"{Request.Scheme}://{Request.Host}/cart/",
                };

                StripeConfiguration.ApiKey = _stripeSecretKey;
                var service = new SessionService();
                var session = service.Create(options);

                return Redirect(session.Url);
            }
            catch (Exception ex)
            {
                return BadRequest(new { ErrorMessage = "An error occurred", ErrorDetails = ex.Message });
            }
        }


        [HttpGet("/cart/success")]
        public IActionResult PaymentSuccess([FromQuery] string session_id, [FromQuery] string orderId)
        {
            try
            {
                StripeConfiguration.ApiKey = _stripeSecretKey;
                var service = new SessionService();
                var session = service.Get(session_id);

                if (session.PaymentStatus == "paid")
                {
                    string transactionId = session.PaymentIntentId;
                    decimal amountAuthorized = (decimal)session.AmountTotal / 100;

                    _commerceApi.Uow.Execute(uow =>
                    {
                        Guid orderIdGuid = new Guid(orderId);

                        var order = _commerceApi.GetOrder(orderIdGuid)
                            .AsWritable(uow);

                        order.InitializeTransaction();
                        PaymentStatus paymentStatus = PaymentStatus.Authorized;

                        order.Finalize(amountAuthorized, transactionId, paymentStatus);
                        _commerceApi.SaveOrder(order);

                        uow.Complete();
                    });

                    string returnUrl = "/order-confirmation/";
                    return Redirect(returnUrl);
                }
                else
                {
                    return BadRequest(new { ErrorMessage = "Payment not successful", ErrorDetails = session.PaymentStatus });
                }
            }
            catch (System.ComponentModel.DataAnnotations.ValidationException ex)
            {
                return BadRequest(new { ErrorMessage = "Failed to set order payment method", ErrorDetails = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { ErrorMessage = "An error occurred", ErrorDetails = ex.Message });
            }
        }



    }
}
