using ElectroMaster.Core.Models.System.Product;
using Iconnect.Umbraco.Utils.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
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
        public IActionResult SearchProduct(string productName)
        {
            var contentTypeAlias = "product";
            var docTypeAlias = "productName";


            var GetSectionUsingContentQuerytest = _contentManagementService.GetSectionUsingContentQuery(contentTypeAlias);
            var GetSectionUsingUmbracoContexttest = _contentManagementService.GetSectionUsingUmbracoContext(contentTypeAlias);
            var GetSectionByDocumentTypeAliastest = _contentManagementService.GetSectionByDocumentTypeAlias(docTypeAlias); 
           


            var products = _contentManagementService.SearchContent(contentTypeAlias, productName).ToList();

            var result = new List<object>();

            foreach (var item in products)
            {
                var name = item.Name;
                var propertyValue = item.Properties.FirstOrDefault(p => p.Key == "productDetail").Value;

                if (propertyValue != null)
                {
                    result.Add(new
                    {
                        Name = name,
                        ProductDetail = propertyValue
                    });
                }
            }

            return Ok(result);
        }
    }
}
