// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4;
using IdentityServer4.Models;
using System.Collections.Generic;

namespace MARVIN.IDP
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> Ids =>
            new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                //To return given_name, family_name and so on Claims
                new IdentityResources.Profile(),
                new IdentityResources.Address(),
                //The roles has not defined like OpenId and Profile scope. So we crate it.
                new IdentityResource(
                     "roles",
                    "Your role(s)",
                    new List<string>(){"role"}), //claim types,
                new IdentityResource(
                    "country",
                    "The country you're living in",
                    new List<string>(){"country"}
                    ),
                new IdentityResource(
                    "subscriptionlevel",
                    "Your subscription level",
                    new List<string>(){ "subscriptionlevel" }
                    )
            };

        public static IEnumerable<ApiResource> Apis =>
            new ApiResource[]
            {
                new ApiResource("imagegalleryapi","Image Gallery API",
                    //We need to include user claims to work at API level.
                    //Without this , we are not able to work claims. e.g. Atrributtes
                    new List<string>{ "role"})
                {
                    ApiSecrets={new Secret("apisecret".Sha256())}
                }
            };

        public static IEnumerable<Client> Clients =>
            new Client[]
            {
                new Client()
                {
                    AccessTokenType = AccessTokenType.Reference,
                    AccessTokenLifetime=120,
                    //It's requirement to support refresh tokens.
                    AllowOfflineAccess=true,
                    //If user change the address claim, user will not be realize when refrest token has not expired. Maybe it 
                    // will take 30 days.
                    //But if we set this to true, user will realize.
                    UpdateAccessTokenClaimsOnRefresh = true,
                    ClientName = "Image app via OpenID Connect",
                    ClientId = "imageappclient",
                    // Whenever i will create Angular app i have to set this to Implicit
                    AllowedGrantTypes = GrantTypes.Code,
                    RequirePkce = true,
                    RedirectUris = new List<string>()
                    {
                        "https://localhost:44346/signin-oidc"
                    },
                    PostLogoutRedirectUris = new List<string>()
                    {
                        "https://localhost:44346/signout-callback-oidc"
                    },
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Address,
                        "roles",
                        "imagegalleryapi",
                        "country",
                        "subscriptionlevel"
                    },
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    }
                }
            };
    }
}