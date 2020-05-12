using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.AccessTokenValidation;
using ImageGalleryAPI.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ImageGalleryAPI
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                   .AddJsonOptions(opts => opts.JsonSerializerOptions.PropertyNamingPolicy = null);

            services.AddScoped<IAuthorizationHandler, MustOwnImageHandler>();
            services.AddHttpContextAccessor();

            services.AddAuthorization(authorizationOptions =>
            {
                authorizationOptions.AddPolicy(
                        "MustOwnImage",
                            policyBuilder =>
                            {
                                policyBuilder.RequireAuthenticatedUser();
                                policyBuilder.AddRequirements(
                                    new MustOwnImageRequirement());
                            }
                    );
            });

            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(options =>
                {
                    //it's the address of IDP. it'll read the metadata of the IDP
                    options.Authority = "https://localhost:44365/";
                    options.ApiName = "imagegalleryapi";
                    options.ApiSecret = "apisecret";
                });

            services.AddAuthorization();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(options =>
            {
                options.MapControllers();
            });
        }
    }
}
