using ElectroMaster.Core.Authentication;
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
            // Configure CORS
            services.AddCors(options =>
            {
                options.AddPolicy("MyPolicy", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });

          
            // Add MVC
            services.AddControllers();

            services.AddAuthentication(ApiKeyAuthenticationDefaults.AuthenticationScheme)
           .AddApiKey(options =>
           {
               // ?? the api key is read from the appsettings.json here.
               options.ApiKey = _config.GetValue<string>("ApiConnection:ApiKey");
           });

            // Add Umbraco services
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

            // Enable CORS globally
            app.UseCors("MyPolicy");

            // Configure Swagger UI for ElectroMaster API
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                if (env.IsDevelopment())
                {
                    c.SwaggerEndpoint("/swagger/default/swagger.json", "Default API");
                }
                else
                {
                    c.SwaggerEndpoint("/swagger/default/swagger.json", "Default API");
                }
            });
        
            // Use Umbraco middleware and configure endpoints
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

            // Other middleware or configurations as needed
        }
    }
}
