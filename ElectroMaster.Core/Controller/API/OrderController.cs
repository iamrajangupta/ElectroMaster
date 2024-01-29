using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Common;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Extensions;
using System.ComponentModel.DataAnnotations;
using ElectroMaster.Core.Models.System.Cart;
using electromaster.core.models.system.cart;

namespace ElectroMaster.Core.Controller.API
{
    [ApiController]
    [Route("api/order")]
    public class OrderController : UmbracoApiController
    {

        private readonly IUmbracoCommerceApi _commerceApi;
        private readonly Guid _storeId = new Guid("1f0f0ae0-dcba-4b1c-8584-018cd87f4959");
        public OrderController(UmbracoHelper umbracoHelper, ServiceContext services, IUmbracoCommerceApi commerceApi)
        {
            _commerceApi = commerceApi;
        }

     
        [HttpGet]
        public IActionResult GetAllOrders()
        {
            try
            {
                var order = _commerceApi.GetOrders;               
                return Ok(new
                {
                    OrderID = order,                  
                });
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

      

    }
}
