using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;
using Umbraco.Commerce.Extensions;

namespace ElectroMaster
{
    public class Startup
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;

        public Startup(IWebHostEnvironment webHostEnvironment, IConfiguration config)
        {
            _env = webHostEnvironment ?? throw new ArgumentNullException(nameof(webHostEnvironment));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            // Configure Swagger for ElectroMaster API
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "ElectroMaster API", Version = "v1" });
            });

            // Configure Swagger for Umbraco Commerce API
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("storefront", new OpenApiInfo { Title = "Storefront API", Version = "v1" });
            });

            services.AddUmbraco(_env, _config)
                .AddBackOffice()
                .AddWebsite()
                .AddDeliveryApi()
                .AddUmbracoCommerce(builder =>
                {
                    builder.AddStorefrontApi();
                })
                .AddComposers()
                .Build();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Configure Swagger UI for ElectroMaster API
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "ElectroMaster API");
                c.SwaggerEndpoint("/umbraco/swagger/storefront/swagger.json", "Storefront API");
                c.SwaggerEndpoint("/umbraco/swagger/default/swagger.json", "Default API");
                c.RoutePrefix = "swagger";
                c.DocExpansion(DocExpansion.None);
            });

            app.UseUmbraco()
                .WithMiddleware(u =>
                {
                    u.UseBackOffice();
                    u.UseWebsite();
                })
                .WithEndpoints(u =>
                {
                    u.UseInstallerEndpoints();
                    u.UseBackOfficeEndpoints();
                    u.UseWebsiteEndpoints();
                });
        }
    }
}
