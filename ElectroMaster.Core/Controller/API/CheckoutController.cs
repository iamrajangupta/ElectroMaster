using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Common;
using Umbraco.Commerce.Core.Api;
using ElectroMaster.Core.Models.System.Checkout;
using Umbraco.Commerce.Core;
using Umbraco.Commerce.Core.Models;


namespace ElectroMaster.Core.Controller.API
{
    [ApiController]
    [Route("api/checkout")]
    public class CheckoutController : UmbracoApiController
    {
        private readonly IUmbracoCommerceApi _commerceApi;
        private readonly Guid _storeId = new Guid("1f0f0ae0-dcba-4b1c-8584-018cd87f4959");
        public CheckoutController(UmbracoHelper umbracoHelper, ServiceContext services, IUmbracoCommerceApi commerceApi)
        {
            _commerceApi = commerceApi;
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


    }
}