using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Common;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Extensions;
using System.ComponentModel.DataAnnotations;
using ElectroMaster.Core.Models.System.Cart;

namespace ElectroMaster.Core.Controller.API
{
    [ApiController]
    [Route("api/cart")]
    public class CartController : UmbracoApiController
    {
        private readonly IUmbracoCommerceApi _commerceApi;    
        private readonly Guid _storeId = new Guid("1f0f0ae0-dcba-4b1c-8584-018cd87f4959");
        public CartController(UmbracoHelper umbracoHelper, ServiceContext services, IUmbracoCommerceApi commerceApi)
        {
            _commerceApi = commerceApi;
        }

        [HttpPost]
        public IActionResult AddToCart(AddToCartDto postModel)
        {
            postModel.ProductCount = postModel.ProductCount <= 0 ? 1 : postModel.ProductCount;            
            try
            {
                Guid orderId = Guid.Empty; // Initialize with an empty Guid
                _commerceApi.Uow.Execute(uow =>
                {
                    var order = _commerceApi.GetOrCreateCurrentOrder(_storeId)
                        .AsWritable(uow)
                        .AddProduct(postModel.ProductReference, postModel.ProductVariantReference, postModel.ProductCount);

                    _commerceApi.SaveOrder(order);
                    orderId = order.Id;

                    uow.Complete();
                });

                // Return the order ID along with the OK response
                return Ok(new { OrderId = orderId });
            }
            catch (ValidationException ex)
            {
                // Handle validation exception if needed
                return BadRequest("Validation failed");
            }
        }


        [HttpDelete]
        public IActionResult RemoveFromCart(RemoveFromCartDto postModel)
        {
            try
            {
                _commerceApi.Uow.Execute(uow =>
                {                   
                    var order = _commerceApi.GetOrder(_storeId)
                        .AsWritable(uow)
                        .RemoveOrderLine(postModel.OrderLineId);

                    _commerceApi.SaveOrder(order);

                    uow.Complete();
                });
            }
            catch (ValidationException ex)
            {
              return BadRequest(ex.Message);
            }         
            return Ok();
        }

        [HttpGet("GetOrders")]
        public IActionResult GetOrders(Guid orderId)
        {
            try
            {
                var order = _commerceApi.GetOrder(orderId);

                if (order == null)
                {
                    return NotFound("Order not found");
                }

                var orderLines = new List<OrderLineDto>();

                foreach (var item in order.OrderLines)
                {
                    var orderLineModel = new OrderLineDto
                    {
                        OrderLineId = item.Id,
                        ProductReference = item.ProductReference,
                        Name = item.Name,
                        PriceExclTax = item.UnitPrice.Base.WithoutTax,
                        Tax = item.UnitPrice.Base.Tax,
                        UnitPrice = item.UnitPrice.WithoutAdjustments,
                        Quantity = (int)item.Quantity,
                        Total = item.TotalPrice.WithoutAdjustments,
                    };

                    orderLines.Add(orderLineModel);
                }

                // Calculate the Subtotal
                var subTotal = order.TotalPrice.WithoutAdjustments.Formatted();

                return Ok(new
                {
                    OrderLines = orderLines,
                    SubTotal = subTotal,
                });
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("AddItemtoOrder")]
        public IActionResult AddItemToOrder(AddToCartDto postModel, Guid orderId)
        {
            try
            {
                _commerceApi.Uow.Execute(uow =>
                {
                    var order = _commerceApi.GetOrder(orderId)
                       .AsWritable(uow)
                       .AddProduct(postModel.ProductReference, postModel.ProductVariantReference, postModel.ProductCount);

                    _commerceApi.SaveOrder(order);

                    uow.Complete();
                });
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok();
        }
    }
}
