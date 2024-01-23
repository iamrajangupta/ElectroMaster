//using Microsoft.AspNetCore.Mvc;
//using Umbraco.Cms.Core.Cache;
//using Umbraco.Cms.Core.Logging;
//using Umbraco.Cms.Core.Routing;
//using Umbraco.Cms.Core.Services;
//using Umbraco.Cms.Core.Web;
//using Umbraco.Cms.Infrastructure.Persistence;
//using Umbraco.Cms.Web.Website.Controllers;
//using Umbraco.Commerce.Core.Api;
//using Umbraco.Commerce.Core.Models;
//using Umbraco.Commerce.Core.PaymentProviders;
//using Umbraco.Commerce.Core.Services;

//namespace ElectroMaster.Core.Controller
//{
//    public class PaymentSurfaceController : SurfaceController
//    {
//        private readonly IUmbracoCommerceApi _commerceApi;
//        private readonly IPaymentProviderService _paymentProviderService;

//        public PaymentSurfaceController(
//            IUmbracoContextAccessor umbracoContextAccessor,
//            IUmbracoDatabaseFactory databaseFactory,
//            ServiceContext services,
//            AppCaches appCaches,
//            IProfilingLogger profilingLogger,
//            IPublishedUrlProvider publishedUrlProvider,
//            IUmbracoCommerceApi commerceApi,
//            IPaymentProviderService paymentProviderService)
//            : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
//        {
//            _commerceApi = commerceApi;
//            _paymentProviderService = paymentProviderService;
//        }

//        public IActionResult FinalizeOrder(int storeId, int orderId, string paymentProviderAlias)
//        {
//            try
//            {
//                // Get the current order
//                OrderReadOnly order = _commerceApi.GetCurrentOrder(storeId);

//                // Get the payment provider
//                IPaymentProvider paymentProvider = _paymentProviderService.GetProvider(paymentProviderAlias);

//                // Process the callback
//                CallbackInfo callbackInfo = paymentProvider.ProcessCallback(order, HttpContext.Request, new Dictionary<string, string>());

//                if (callbackInfo != null)
//                {
//                    // Finalize the order
//                    order.Finalize(callbackInfo.AmountAuthorized, callbackInfo.TransactionId,
//                        callbackInfo.PaymentState, callbackInfo.PaymentType, callbackInfo.PaymentIdentifier);
//                }

//                // Additional steps as needed for your application

//                return RedirectToUmbracoPage("/home"); // Redirect to a success page
//            }
//            catch (Exception ex)
//            {
//                ModelState.AddModelError("", "Failed to finalize order: " + ex.Message);
//                return CurrentUmbracoPage();
//            }
//        }
//    }
//}
