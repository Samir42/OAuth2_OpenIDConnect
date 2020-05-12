// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel;
using IdentityServer4.Test;
using System.Collections.Generic;
using System.Security.Claims;

namespace MARVIN.IDP
{
    public class TestUsers
    {
        public static List<TestUser> Users = new List<TestUser>
        {
            new TestUser{SubjectId = "1", Username = "alice", Password = "alice", 
                Claims = 
                {
                    new Claim(JwtClaimTypes.Name, "Alice Smith"),
                    new Claim(JwtClaimTypes.FamilyName, "Smith"),
                    new Claim(JwtClaimTypes.GivenName, "Smith"),
                    new Claim(JwtClaimTypes.Role, "PayingUser"),
                    new Claim(JwtClaimTypes.Address, "Sabunchu qes"),
                    new Claim("subscriptionlevel","PayingUser"),
                    new Claim("country","az"),
                }
            },
            new TestUser{SubjectId = "2", Username = "bob", Password = "bob", 
                Claims = 
                {
                    new Claim(JwtClaimTypes.Name, "Bob Smith"),
                    new Claim(JwtClaimTypes.GivenName, "Bob"),
                    new Claim(JwtClaimTypes.FamilyName, "Smith"),
                    new Claim(JwtClaimTypes.Role, "FreeUser"),
                    new Claim(JwtClaimTypes.Address, "Merdekan qes"),
                    new Claim("subscriptionlevel","FreeUsers"),
                    new Claim("country","tr"),
                }
            }
        };
    }
}