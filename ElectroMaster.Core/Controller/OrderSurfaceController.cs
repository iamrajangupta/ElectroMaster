using Microsoft.AspNetCore.Mvc;
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
        public OrderSurfaceController(IUmbracoContextAccessor umbracoContextAccessor, IUmbracoCommerceApi commerceApi, IUmbracoDatabaseFactory databaseFactory, ServiceContext services, AppCaches appCaches, IProfilingLogger profilingLogger, IPublishedUrlProvider publishedUrlProvider) : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
        {
            _commerceApi = commerceApi;
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
                    Guid CancelstatusId = new Guid("fcb245dd-4227-4d99-8ab4-018cd87f4bac");

                    var order = _commerceApi.GetOrder(orderId)
                                            .AsWritable(uow);
                    if (order == null)
                    {
                        return CurrentUmbracoPage();
                    }
                    if (order.OrderStatusId == statusId)
                    {
                        order.SetOrderStatus(CancelstatusId);
                        _commerceApi.SaveOrder(order);
                        uow.Complete();
                        TempData["addOrderId"] = orderId;
                        TempData["OrderCancelSucess"] = "OrderCancelSucess";
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

                ModelState.AddModelError("productReference", "Failed to remove cart item");       
                return CurrentUmbracoPage(); 
            }
        }

    }
}
