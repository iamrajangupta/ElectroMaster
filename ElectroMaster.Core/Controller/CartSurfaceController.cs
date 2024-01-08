using ElectroMaster.Core.Models.System;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Common.PublishedModels;
using Umbraco.Cms.Web.Website.Controllers;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Core.Models;
using Umbraco.Commerce.Extensions;

namespace ElectroMaster.Core.Controller
{
	public class CartSurfaceController : SurfaceController
	{
		private readonly IUmbracoCommerceApi _commerceApi;

		public CartSurfaceController(IUmbracoContextAccessor umbracoContextAccessor, IUmbracoDatabaseFactory databaseFactory,
			ServiceContext services, AppCaches appCaches, IProfilingLogger profilingLogger, IPublishedUrlProvider publishedUrlProvider,
			IUmbracoCommerceApi commerceApi)
			: base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
		{
			_commerceApi = commerceApi;
		}

		[HttpPost]
		public IActionResult AddToCart(AddToCartDto postModel)
		{
			try
			{
				_commerceApi.Uow.Execute(uow =>
				{
					var store = CurrentPage.AncestorOrSelf<Home>()?.Store;
					
					var order = _commerceApi.GetOrCreateCurrentOrder(store.Id)
						.AsWritable(uow)
						.AddProduct(postModel.ProductReference, postModel.ProductVariantReference, 1);

					_commerceApi.SaveOrder(order);

					uow.Complete();
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
		
	}
}
