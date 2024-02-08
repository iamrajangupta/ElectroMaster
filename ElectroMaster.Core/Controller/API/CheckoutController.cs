using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Common;
using Umbraco.Commerce.Core.Api;
using ElectroMaster.Core.Models.System.Checkout;
using Umbraco.Commerce.Core;
using Umbraco.Commerce.Core.Models;
using Umbraco.Commerce.Core.Services;
using Umbraco.Commerce.Extensions;


namespace ElectroMaster.Core.Controller.API
{
    [ApiController]
    [Route("api/checkout")]
    public class CheckoutController : UmbracoApiController
    {
        private readonly IUmbracoCommerceApi _commerceApi;
        public CheckoutController(UmbracoHelper umbracoHelper, ServiceContext services, IUmbracoCommerceApi commerceApi, IPaymentService paymentService)
        {
             _commerceApi = commerceApi;        
        }

        Order updatedOrder = null;

        [HttpPost("updateInformation")]
        public IActionResult UpdateOrderInformation(UpdateOrderInformationDto model)
        {
            try
            {

                _commerceApi.Uow.Execute(uow =>
                {
                    model.BillingAddress.Country = new Guid("5ec6dc87-7b28-4abf-b0c0-018cd87f4a68");
                    model.ShippingAddress.Country = new Guid("5ec6dc87-7b28-4abf-b0c0-018cd87f4a68");

                    var order = _commerceApi.GetOrder(model.OrderId)
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


        [HttpPost("confirmOrder")]
        public IActionResult ConfirmOrder(ConfirmOrder model)
        {
            try
            {
                _commerceApi.Uow.Execute(uow =>
                {
                    var paymentMethod = new Guid("9559a36d-d7cf-45e7-99ed-018cd87f4ce8");
                    var shippingMethod = new Guid("f3a27666-021a-48fe-90ed-018cd87f4de3");
                    var status = new Guid("580181af-ad08-410f-b277-018cd87f4b7e");


                    var order = _commerceApi.GetOrder(model.OrderId)
                                            .AsWritable(uow)
                                            .SetPaymentMethod(paymentMethod)
                                            .SetShippingMethod(shippingMethod); 
                                            
                    order.InitializeTransaction();
                    order.Finalize(order.TotalPrice, Guid.NewGuid().ToString("N"), PaymentStatus.Authorized);                 
                    order.SetOrderStatus(status);
                    _commerceApi.SaveOrder(order);
                    uow.Complete();
                });

            }
            catch (System.ComponentModel.DataAnnotations.ValidationException ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok(updatedOrder);
        }
    }
}