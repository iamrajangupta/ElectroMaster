using ElectroMaster.Core.Extensions;
using ElectroMaster.Core.Models.System.Cart;
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
using Umbraco.Commerce.Extensions;


namespace ElectroMaster.Core.Controller
{

    public class OrderSurfaceController : SurfaceController
    {      
        public OrderSurfaceController(IUmbracoContextAccessor umbracoContextAccessor, IUmbracoDatabaseFactory databaseFactory, ServiceContext services, AppCaches appCaches, IProfilingLogger profilingLogger, IPublishedUrlProvider publishedUrlProvider) : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
        {
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
    }
}
