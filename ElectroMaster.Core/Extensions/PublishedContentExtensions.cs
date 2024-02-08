using Microsoft.AspNetCore.Mvc.RazorPages;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Core.Models;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace ElectroMaster.Core.Extensions
{
    public static class PublishedContentExtensions
    {  
        public static StoreReadOnly GetStore(this IPublishedContent content)
        {
            if (content == null)
            {            
                return null;
            }
            return content.AncestorOrSelf<Home>()?.Store;
        }
       
    }
}
