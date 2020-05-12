using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace OAuth_OpenIdConnect.Controllers
{
    [Authorize]
    public class ImageController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ImageController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory ??
                throw new ArgumentNullException(nameof(httpClientFactory));
        }

        [HttpGet]
        public async Task<string> Index()
        {
            await WriteOutIdentityInformation();

            var httpClient = _httpClientFactory.CreateClient("APIClient");

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                "/api/images/");


            //BearerTokenHandler.SendAsync will be called by this line of code.
            //Setting the bearer token by accessToken
            var response = await httpClient.SendAsync(
                request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                using (var responseStream = await response.Content.ReadAsStreamAsync())
                {
                    return $"Your response is {responseStream}";
                }
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized ||
               response.StatusCode == HttpStatusCode.Forbidden)
            {
                return "Access Denied at API level";
            }

            throw new Exception("Problem accessing the API");
        }


        public async Task Logout()
        {
            var client = _httpClientFactory.CreateClient("IDPClient");
            var discoveryDocumentResponse = await client.GetDiscoveryDocumentAsync();

            if (discoveryDocumentResponse.IsError)
            {
                throw new Exception(discoveryDocumentResponse.Error);
            }

            //Revoke the access token
            var accessTokenRevocationResponse = await client.RevokeTokenAsync(
                new TokenRevocationRequest()
                {
                    Address = discoveryDocumentResponse.RevocationEndpoint,
                    ClientId = "imageappclient",
                    ClientSecret = "secret",
                    Token = await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken)
                });


            if (accessTokenRevocationResponse.IsError)
            {
                throw new Exception(accessTokenRevocationResponse.Error);
            }

            //Revoke the refrest token
            var refreshTokenRevocationResponse = await client.RevokeTokenAsync(
               new TokenRevocationRequest()
               {
                   Address = discoveryDocumentResponse.RevocationEndpoint,
                   ClientId = "imageappclient",
                   ClientSecret = "secret",
                   Token = await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.RefreshToken)
               });

            if (refreshTokenRevocationResponse.IsError)
            {
                throw new Exception(refreshTokenRevocationResponse.Error);
            }

            //It's not enough when it's lonely. Because this just clear cookies. 
            //We need to additionally sign out at IDP level.
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
        }

        //[Authorize(Roles = "PayingUser")]
        [Authorize(Policy = "CanOrderFrame")]
        public async Task<string> OrderFrame()
        {
            var idpClient = _httpClientFactory.CreateClient("IDPClient");


            var metaDataResponse = await idpClient.GetDiscoveryDocumentAsync();

            if (metaDataResponse.IsError)
            {
                throw new Exception(
                    "Problem accessing the discovery endpoint."
                    , metaDataResponse.Exception);
            }


            var accessToken = await HttpContext.
                GetTokenAsync(OpenIdConnectParameterNames.AccessToken);

            var userInfoResponse = await idpClient.GetUserInfoAsync(
                new UserInfoRequest()
                {
                    Address = metaDataResponse.UserInfoEndpoint,
                    Token = accessToken
                });

            if (userInfoResponse.IsError)
            {
                throw new Exception(
                   "Problem accessing the UserInfo endpoint."
                   , userInfoResponse.Exception);
            }

            var address = userInfoResponse.Claims
                .FirstOrDefault(c => c.Type == "address")?.Value;

            return address;
        }

        public async Task WriteOutIdentityInformation()
        {
            var identityToken = await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.IdToken);

            Debug.WriteLine($"Identity token : {identityToken}");

            foreach (var claim in User.Claims)
            {
                Debug.WriteLine($"Claim type : {claim.Type} - Claim value : {claim.Value}");
            }
        }
    }
}
