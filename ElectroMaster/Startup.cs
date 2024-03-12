using Iconnect.Umbraco.Utils.Interfaces;
using Iconnect.Umbraco.Utils.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
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

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<ICacheService, CacheService>();
            services.AddTransient<IChatGptService, ChatGptService>();
            services.AddTransient<IEmailService, EmailService>();
            services.AddTransient<IContentManagementService, ContentManagementService>();
            services.AddTransient<IMemberRegistrationService, MemberRegistrationService>();


            // Add authentication
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie() // Add cookie authentication
            .AddGoogle("Google", options =>
            {

                IConfigurationSection googleAuthNSection = _config.GetSection("Authentication:Google");
                options.ClientId = googleAuthNSection["ClientId"];
                options.ClientSecret = googleAuthNSection["ClientSecret"];
                options.CallbackPath = googleAuthNSection["CallbackPath"];
            })
            .AddFacebook("Facebook", options =>
            {
               
                IConfigurationSection facebookAuthNSection = _config.GetSection("Authentication:Facebook");
                options.AppId = facebookAuthNSection["AppId"];
                options.AppSecret = facebookAuthNSection["AppSecret"];
            });

            // Add MVC
            services.AddControllers();

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
