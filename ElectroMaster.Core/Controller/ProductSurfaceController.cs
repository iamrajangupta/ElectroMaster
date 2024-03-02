using Iconnect.Umbraco.Utils.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Website.Controllers;

namespace ElectroMaster.Core.Controller
{
    public class ProductSurfaceController : SurfaceController
    {
        private readonly IContentManagementService _contentManagementService;
        public ProductSurfaceController(IContentManagementService contentManagementService, IUmbracoContextAccessor umbracoContextAccessor, IUmbracoDatabaseFactory databaseFactory, ServiceContext services, AppCaches appCaches, IProfilingLogger profilingLogger, IPublishedUrlProvider publishedUrlProvider) : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
        {
            _contentManagementService = contentManagementService;
        }

        [HttpPost]
        public IActionResult SearchProduct(string productName, string contentTypeAlias)
        {

            if (string.IsNullOrEmpty(productName))
            {
                var product = _contentManagementService.SearchContent(contentTypeAlias, productName);
            }
            ModelState.AddModelError("orderId", "Invalid order Id");
            return BadRequest(ModelState);

        }

    }
}
