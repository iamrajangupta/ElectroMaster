using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Common;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Extensions;
using System.ComponentModel.DataAnnotations;
using ElectroMaster.Core.Models.System.Cart;
using electromaster.core.models.system.cart;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Models;
using ElectroMaster.Core.Models.System.Checkout;
using Umbraco.Commerce.Core.Models;

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
                return Ok(new { OrderId = orderId });
            }
            catch (ValidationException ex)
            {
                return BadRequest("Validation failed");
            }
        }


        [HttpPut("removeFromCart")]
        public IActionResult RemoveFromCart(RemoveFromCartDto postModel)
        {
            try
            {

                if (postModel == null)
                {
                    return BadRequest("RemoveFromCartDto cannot be null.");
                }

                _commerceApi.Uow.Execute(uow =>
                {
                    var orders = _commerceApi.GetOrder(postModel.OrderId);

                    var order = orders
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

        [HttpGet]
        public IActionResult GetOrders(Guid OrderId)
        {
            try
            {
                var order = _commerceApi.GetOrder(OrderId);

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
                    OrderID = OrderId,
                    OrderLines = orderLines,
                    SubTotal = subTotal,
                });
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        public IActionResult AddItemToOrder(UpdateCartDto postModel)
        {
            try
            {
                if (postModel.OrderId == null)
                {
                    return BadRequest("Order ID cannot be null.");
                }
                _commerceApi.Uow.Execute(uow =>
                {
                    var order = _commerceApi.GetOrder(postModel.OrderId)
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



        [HttpGet("GetOrderbyemail")]
        public IActionResult GetOrdersByEmail([FromQuery] string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest("Email address is required.");
            }

            var orders = _commerceApi.GetAllOrdersForCustomer(_storeId, email)
                            .Where(order => order.IsFinalized)
                            .OrderByDescending(order => order.FinalizedDate)
                            .ToList();

            if (orders.Count == 0)
            {
                return NotFound("No orders found for the provided email address.");
            }

            var orderViewModels = orders.Select(order => new
            {
                TotalQuantity = Convert.ToInt32(order.TotalQuantity),
                OrderDate = order.FinalizedDate,
                OrderPrice = order.TotalPrice.WithoutAdjustments.Formatted().WithTax,
                Status = _commerceApi.GetOrderStatus(order.OrderStatusId).Name,
                OrderId = order.Id
            });

            return Ok(orderViewModels);
        }

    }
}
