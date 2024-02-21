using ElectroMaster.Core.Extensions;
using ElectroMaster.Core.Models.System.Cart;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Common;
using Umbraco.Cms.Web.Website.Controllers;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Extensions;

namespace ElectroMaster.Core.Controller
{
    public class CartSurfaceController : SurfaceController
    {
        private readonly IUmbracoCommerceApi _commerceApi;
        private readonly Guid _storeId = new Guid("1f0f0ae0-dcba-4b1c-8584-018cd87f4959");
        private readonly UmbracoHelper _umbracoHelper;

        public CartSurfaceController(UmbracoHelper umbracoHelper, IUmbracoContextAccessor umbracoContextAccessor, IUmbracoDatabaseFactory databaseFactory,
            ServiceContext services, AppCaches appCaches, IProfilingLogger profilingLogger, IPublishedUrlProvider publishedUrlProvider,
            IUmbracoCommerceApi commerceApi)
            : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
        {
            _commerceApi = commerceApi;
            _umbracoHelper = umbracoHelper;
        }

        [HttpPost]
        public IActionResult AddToCart(AddToCartDto postModel)
        {
            var store = CurrentPage.GetStore();
            postModel.ProductCount = postModel.ProductCount <= 0 ? 1 : postModel.ProductCount;

            try
            {
                _commerceApi.Uow.Execute(uow =>
                {
                    var order = _commerceApi.GetOrCreateCurrentOrder(store.Id).AsWritable(uow);
                    if (order != null)
                    {
                        order.AddProduct(postModel.ProductReference, postModel.ProductVariantReference, postModel.ProductCount);
                        _commerceApi.SaveOrder(order);
                        uow.Complete();
                    }
                });
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("productReference", "Failed to add product to cart");
                return CurrentUmbracoPage();
            }

            TempData["addedProductReference"] = postModel.ProductReference;

            return RedirectToCurrentUmbracoPage();
        }

        [HttpPost]
        public IActionResult RemoveFromCart(RemoveFromCartDto postModel)
        {

            var paginatedContent = PaginateContent("product", 2, 5);

            var kk = paginatedContent.Items.ToList();

            foreach (var item in kk)
            {
                var c = item.Properties.ToList();
                var b = item.Title;
                foreach (var p in c)
                {
                    var s = p.Value;
                }
            } 
            //try
            //{
            //    _commerceApi.Uow.Execute(uow =>
            //    {
            //        var order = _commerceApi.GetOrCreateCurrentOrder(_storeId)
            //            .AsWritable(uow)
            //            .RemoveOrderLine(postModel.OrderLineId);
            //        _commerceApi.SaveOrder(order);

            //        uow.Complete();
            //    });
            //}
            //catch (ValidationException ex)
            //{
            //    ModelState.AddModelError("productReference", "Failed to remove cart item");

            //    return CurrentUmbracoPage();
            //}

            string returnUrl = "/cart";
            return Redirect(returnUrl);
        }

        [HttpPost]
        public PaginationModel<ContentModel> PaginateContent(string contentTypeAlias, int currentPage, int noItemsOnPage)
        {
            contentTypeAlias = "product";
            currentPage = 2;
            noItemsOnPage = 5;


            var contentItems = _umbracoHelper.ContentAtRoot()
                .SelectMany(rootNode => rootNode.Descendants())
                .Where(x => x.ContentType.Alias == contentTypeAlias && x.IsVisible())
                .OrderByDescending(a => a.CreateDate)
                .ToList();

            var paginatedContent = Paginate(contentItems, currentPage, noItemsOnPage);

            return paginatedContent;
        }

        private PaginationModel<ContentModel> Paginate(List<IPublishedContent> items, int page, int pageSize)
        {
            var totalPages = (int)Math.Ceiling((double)items.Count / pageSize);

            if (page >= totalPages)
            {
                page = totalPages;
            }
            else if (page < 1)
            {
                page = 1;
            }

            var paginatedItems = items.Skip((page - 1) * pageSize)
                                       .Take(pageSize)
                                       .Select(contentItem => new ContentModel
                                       {
                                           Title = contentItem.Name,
                                           Properties = contentItem.Properties.ToDictionary(property => property.Alias, property => property.GetValue())
                                       });

            return new PaginationModel<ContentModel>()
            {
                PageSize = pageSize,
                CurrentPage = page,
                TotalPages = totalPages,
                Items = paginatedItems
            };
        }

        public class ContentModel
        {
            public string Title { get; set; }

            public IDictionary<string, object> Properties { get; set; }
        }

        public class PaginationModel<T>
        {
            public int PageSize { get; set; }
            public int TotalPages { get; set; }
            public int CurrentPage { get; set; }
            public string? Url { get; set; }
            public IEnumerable<T>? Items { get; set; }
        }

        public IEnumerable<ContentModel> FilterContent(string contentTypeAlias, string filter)
        {
            contentTypeAlias = "product";
            filter = "SAMSUNG";
            var contentItems = _umbracoHelper.ContentAtRoot()
                .SelectMany(rootNode => rootNode.Descendants())
                .Where(x => x.ContentType.Alias == contentTypeAlias && x.IsVisible())
                .OrderByDescending(a => a.CreateDate)
                .ToList();

            var result = new List<ContentModel>();

            foreach (var contentItem in contentItems)
            {
                var properties = new Dictionary<string, object>();

                foreach (var property in contentItem.Properties)
                {
                    var propertyValue = property.GetValue()?.ToString();
                    if (!string.IsNullOrEmpty(propertyValue) && propertyValue.Contains(filter, StringComparison.OrdinalIgnoreCase))
                    {
                        properties[property.Alias] = propertyValue;
                    }
                }

                if (properties.Count > 0)
                {
                    result.Add(new ContentModel
                    {
                        Title = contentItem.Name,
                        Properties = properties
                    });
                }
            }

            return result;
        }
    }
}
