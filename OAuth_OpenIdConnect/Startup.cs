using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;
using IdentityModel;
using OAuth_OpenIdConnect.HttpHandlers;

namespace OAuth_OpenIdConnect
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddHttpContextAccessor();

            services.AddTransient<BearerTokenHandler>();
            //We need this to turn the claim values into original claim values.
            //i.e. sub sometimes turning into nameidentifier.
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services.AddAuthorization(authorizationOptions =>
            {
                authorizationOptions.AddPolicy(
                        "CanOrderFrame",
                            policyBuilder =>
                            {
                                policyBuilder.RequireAuthenticatedUser();
                                policyBuilder.RequireClaim("country", "az");
                                policyBuilder.RequireClaim("subscriptionlevel", "PayingUser");
                            }
                    );
            });

            services.AddHttpClient("IDPClient", client =>
            {
                client.BaseAddress = new Uri("https://localhost:44365/");
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
            });

            services.AddHttpClient("APIClient", client =>
            {
                client.BaseAddress = new Uri("https://localhost:44354/");
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
            }).AddHttpMessageHandler<BearerTokenHandler>();

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
              .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
              .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
              {
                  options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                  //To connect to IDP.
                  options.Authority = "https://localhost:44365/";
                  options.ClientId = "imageappclient";
                  options.ResponseType = "code";
                  //We haven't to add 'openid' and 'profile' manually. These are adding manually by the middleware.
                  //To be clear , i add these to know what is actually going on.

                  //options.Scope.Add("openid");

                  //to access profile realted claims

                  //options.Scope.Add("profile");

                  //to access address realted claims
                  options.Scope.Add("address");
                  options.Scope.Add("roles");
                  options.Scope.Add("imagegalleryapi");
                  options.Scope.Add("country");
                  options.Scope.Add("subscriptionlevel");
                  //middleware will now get refrest token from token endpoint 
                  options.Scope.Add("offline_access");

                  //remove no needed claims we don't need
                  options.ClaimActions.DeleteClaim("sid");//session id
                  options.ClaimActions.DeleteClaim("idp");//identity provider
                  options.ClaimActions.DeleteClaim("s_hash");
                  options.ClaimActions.DeleteClaim("auth_time"); //authentication time
                  options.ClaimActions.DeleteClaim("sid");

                  //ROLE claim has not included by default. So we add it. (To see console window at client level)
                  options.ClaimActions.MapUniqueJsonKey("role", "role");
                  options.ClaimActions.MapUniqueJsonKey("subscriptionlevel", "subscriptionlevel");
                  options.ClaimActions.MapUniqueJsonKey("country", "country");
                  options.SaveTokens = true;
                  options.ClientSecret = "secret";
                  options.GetClaimsFromUserInfoEndpoint = true;
                  options.TokenValidationParameters = new TokenValidationParameters
                  {
                      NameClaimType = JwtClaimTypes.GivenName,
                      RoleClaimType = JwtClaimTypes.Role
                  };
              });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Image}/{action=Index}/{id?}"
                    );
            });
        }
    }
}
