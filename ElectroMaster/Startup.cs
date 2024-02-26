using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ElectroMaster.Core.Extensions;
using Microsoft.AspNetCore.Authentication.Google;

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
            // Add Google authentication
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = GoogleDefaults.AuthenticationScheme;
            })
            .AddGoogle(options =>
            {
                options.CallbackPath = "/umbraco-google-signin";
                options.ClientId = "750763726057-r58rm7r2oi1ffuogh4hsoe91tol818nj.apps.googleusercontent.com"; // Replace with your client id generated while creating OAuth client ID
                options.ClientSecret = "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"; // Replace with your client secret generated while creating OAuth client ID
                options.SaveTokens = true; // Ensure this is set to true to save tokens
            });

            // Other services configuration...

            services.AddUmbraco(_env, _config)
                .AddBackOffice()
                .AddWebsite()
                .AddGoogleAuthentication() // Add Google authentication
                .AddProviderMemberAuthentication() // Add external login provider for members
                .AddComposers()
                .Build();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Other middleware configuration...

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
