using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Common;
using Umbraco.Cms.Web.Common.PublishedModels;
using Umbraco.Cms.Core.Models;

using Umbraco.Commerce.Core.Api;

namespace ElectroMaster.Core.Controller.API
{
    [ApiController]
    [Route("api/product")]
    public class ProductsController : UmbracoApiController
    {
        private readonly UmbracoHelper _umbracoHelper;
        private readonly ServiceContext _services;
        private readonly IUmbracoCommerceApi _commerceApi;

        public ProductsController(UmbracoHelper umbracoHelper, ServiceContext services, IUmbracoCommerceApi commerceApi)
        {
            _umbracoHelper = umbracoHelper;
            _services = services;
            _commerceApi = commerceApi;
        }

        [HttpGet]
        public IActionResult GetProduct()
        {
            IEnumerable<IPublishedContent> productItems = _umbracoHelper.ContentAtRoot()
                .DescendantsOrSelf<Product>().Where(x => x.IsVisible())
                .OrderByDescending(prodItem => prodItem.CreateDate)
                .ToList();

            var json = productItems.Select(prodItem => new
            {
                ProductId = prodItem.Id,
                Title = prodItem.Value<string>("productName"),
                Date = prodItem.CreateDate.ToString("dd MMM yyyy"),
                Guid = prodItem.Key,
                Stock = prodItem.Value<int>("stock"),
                ProductDetail = prodItem.Value<string>("productDetail"),
               
                ImageUrl = prodItem.Value<MediaWithCrops<Image>>("image").Url().ToString(),
                Url = prodItem.Url()
            });

            return new JsonResult(json);

        }

        [HttpGet("{id}")]

        public IActionResult GetProductById(int id)
        {
            IEnumerable<IPublishedContent> productItem = _umbracoHelper.ContentAtRoot()
                .DescendantsOrSelf<Product>().Where(x => x.IsVisible() && x.Id == id)
                .OrderByDescending(a => a.CreateDate)
                .ToList();


            var json = productItem.Select(prodItem => new
            {
                ProductId = prodItem.Id,
                Title = prodItem.Value<string>("productName"),
                Stock = prodItem.Value<int>("stock"),
                ProductDetail = prodItem.Value<string>("productDetail"),
                Date = prodItem.CreateDate.ToString("dd MMM yyyy"),
                Guid = prodItem.Key,
                ImageUrl = prodItem.Value<MediaWithCrops<Image>>("image").Url().ToString(),
                Url = prodItem.Url()
            });

            return new JsonResult(json);
        }

        [HttpDelete]
        public string DeleteProduct(int id)
        {
            IEnumerable<IPublishedContent> productItem = _umbracoHelper.ContentAtRoot()
            .DescendantsOrSelf<Product>().Where(x => x.IsVisible() && x.Id == id)
            .OrderByDescending(a => a.CreateDate)
            .ToList();

            if (productItem.Any())
            {
                var contentService = _services.ContentService;
                var content = contentService.GetById(id);
                if (content != null)
                {
                    contentService.Delete(content);
                    return "Product deleted successfully..!!";
                }
            }

            return "Product Not Found";


        }
    }
}
