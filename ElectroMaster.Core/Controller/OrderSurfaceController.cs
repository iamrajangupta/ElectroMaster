using ElectroMaster.Core.Models.System.Cart;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Stripe;
using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Website.Controllers;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Core.Models;
using Umbraco.Commerce.Extensions;


namespace ElectroMaster.Core.Controller
{

    public class OrderSurfaceController : SurfaceController
    {
        private readonly IUmbracoCommerceApi _commerceApi;
        private readonly IConfiguration _configuration;
        public OrderSurfaceController(IUmbracoContextAccessor umbracoContextAccessor, IUmbracoCommerceApi commerceApi, IUmbracoDatabaseFactory databaseFactory, IConfiguration configuration, ServiceContext services, AppCaches appCaches, IProfilingLogger profilingLogger, IPublishedUrlProvider publishedUrlProvider) : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
        {
            _commerceApi = commerceApi;
            _configuration = configuration;
        }

        [HttpPost]
        public IActionResult MyOrder(Guid orderId)
        {
            if (orderId == Guid.Empty)
            {

                ModelState.AddModelError("orderId", "Invalid order Id");
                return BadRequest(ModelState);
            }
            else
            {
                TempData["addOrderId"] = orderId;
                string returnUrl = "/my-order/";
                return Redirect(returnUrl);
            }
        }
        [HttpPost]
        public IActionResult CancelOrder(Guid orderId)
        {
            try
            {
                _commerceApi.Uow.Execute(uow =>
                {
                    Guid statusId = new Guid("580181af-ad08-410f-b277-018cd87f4b7e");
                    Guid cancelStatusId = new Guid("fcb245dd-4227-4d99-8ab4-018cd87f4bac");

                    var order = _commerceApi.GetOrder(orderId).AsWritable(uow);
                    if (order == null)
                    {
                        TempData["addOrderId"] = orderId;
                        return CurrentUmbracoPage();
                    }
                    if (order.OrderStatusId == statusId)
                    {
                        var paymentIntent = order.TransactionInfo.TransactionId;

                        // Call the refund method
                        RefundPayment(paymentIntent, order.TotalPrice.Value); // Refund the full amount

                        order.SetOrderStatus(cancelStatusId);
                        _commerceApi.SaveOrder(order);
                        uow.Complete();

                        TempData["addOrderId"] = orderId;
                        TempData["OrderCancelSucess"] = "OrderCancelSuccess";
                        return CurrentUmbracoPage();
                    }
                    else
                    {
                        TempData["addOrderId"] = orderId;
                        TempData["DoNotHavePermission"] = "DoNotHavePermission";
                        return CurrentUmbracoPage();
                    }
                });
                return CurrentUmbracoPage();
            }
            catch (ValidationException ex)
            {
                // Handle validation exceptions
                ModelState.AddModelError("productReference", "Failed to remove cart item");
                return CurrentUmbracoPage();
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                ModelState.AddModelError("generalError", $"An error occurred: {ex.Message}");
                return CurrentUmbracoPage();
            }
        }

        private void RefundPayment(string paymentIntentId, decimal amountToRefund)
        {
            try
            {
                var stripeSecretKey = _configuration["StripeSettings:SecretKey"];
                StripeConfiguration.ApiKey = stripeSecretKey;

               
                var chargeService = new ChargeService();
                var chargeListOptions = new ChargeListOptions
                {
                    PaymentIntent = paymentIntentId,
                    Limit = 1 
                };
                var charges = chargeService.List(chargeListOptions);

                var charge = charges.FirstOrDefault();

                if (charge == null)
                {
                    throw new Exception("No charge associated with the payment intent.");
                }

                
                var refundService = new RefundService();
                var refundOptions = new RefundCreateOptions
                {
                    Charge = charge.Id,
                    Amount = (long)(amountToRefund * 100),
                    Reason = "requested_by_customer",
                };
                var refund = refundService.Create(refundOptions);
            }
            catch (StripeException ex)
            {
                throw new Exception($"Failed to process refund: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred during refund: {ex.Message}");
            }
        }

    }
}
