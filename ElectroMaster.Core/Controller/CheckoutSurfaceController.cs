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
using Umbraco.Commerce.Core.Models;
using Umbraco.Commerce.Extensions;
using UmbracoLibrary.Services;
using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;


namespace ElectroMaster.Core.Controller
{
    public class CheckoutSurfaceController : SurfaceController
    {
        private readonly IUmbracoCommerceApi _commerceApi;
        private readonly IConfiguration _configuration;
    
        public CheckoutSurfaceController(IUmbracoContextAccessor umbracoContextAccessor, IUmbracoDatabaseFactory databaseFactory, IConfiguration configuration,
            ServiceContext services, AppCaches appCaches, IProfilingLogger profilingLogger, IPublishedUrlProvider publishedUrlProvider,
            IUmbracoCommerceApi commerceApi)
            : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
        {
            _commerceApi = commerceApi;
            _configuration = configuration;
        }


        [HttpPost]
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

        [HttpPost]

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
        public IActionResult ApplyDiscountOrGiftCardCode(DiscountOrGiftCardCodeDto model)
        {
            try
            {
                _commerceApi.Uow.Execute(uow =>
                {
                    var store = CurrentPage.GetStore();
                    var order = _commerceApi.GetOrCreateCurrentOrder(store.Id)
                        .AsWritable(uow)
                        .Redeem(model.Code);

                    _commerceApi.SaveOrder(order);

                    uow.Complete();
                });
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("", "Failed to redeem discount code");

                return CurrentUmbracoPage();
            }

            return RedirectToCurrentUmbracoPage();
        }

        [HttpPost]
        public IActionResult RemoveDiscountOrGiftCardCode(string code)
        {
            try
            {
               
                _commerceApi.Uow.Execute(uow =>
                {
                    var store = CurrentPage.GetStore();
                    var order = _commerceApi.GetOrCreateCurrentOrder(store.Id)
                        .AsWritable(uow)
                        .Unredeem(code);

                    _commerceApi.SaveOrder(order);

                    uow.Complete();
                });
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("", "Failed to redeem discount code");

                return CurrentUmbracoPage();
            }

            return RedirectToCurrentUmbracoPage();
        }

        [HttpPost]
        public IActionResult CreateCheckoutSession(decimal amount, string productName, string orderId, int quantity)
        {
            var stripeSecretKey = _configuration["StripeSettings:SecretKey"];
            var stripeCheckout = new StripeCheckoutService();
         
            var successUrl = $"{Request.Scheme}://{Request.Host}/cart/success?session_id={{CHECKOUT_SESSION_ID}}&orderId={orderId}";
            var cancelUrl = $"{Request.Scheme}://{Request.Host}/cart/";
            var currency = "GBP";

            try
            {
                var session = stripeCheckout.CreateCheckoutSession(stripeSecretKey, amount, successUrl, cancelUrl, currency, productName, quantity);
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
                var stripeSecretKey = _configuration["StripeSettings:SecretKey"];
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
