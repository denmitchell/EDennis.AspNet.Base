// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4;
using IdentityServer4.Models;
using System.Collections.Generic;

namespace EDennis.AspNetIdentityServer {
    public static class Config {
        public static IEnumerable<IdentityResource> Ids =>
            new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResource(
                            "roles",
                                    "Your role(s)",
                            new List<string>() { "role" })
            };


        public static IEnumerable<ApiResource> Apis =>
            new List<ApiResource>
            {
                new ApiResource("Hr.PersonApi", "Hr.PersonApi", new List<string>() { "role" })
                {
                    //ApiSecrets = { new Secret("secret".Sha256()) },
                    UserClaims = { "Name", "Email", "role", "user_scope" }, 
                    Scopes = {
                        "Hr.PersonApi.*"
                    },
                    Properties = {
                        { "ApplicationName", "Hr.PersonApi" }
                    }
                }
            };



        public static IEnumerable<Client> Clients =>
            new List<Client>
            {
                // NOTE: Give client broad permissions to API.
                //       Ensure that the APIs use Default Policies and user_scope 
                //         (or other user-based policies) to authorize access to 
                //         specific controllers/actions
                new Client
                {
                    ClientId = "Hr.PersonApi1",
                    ClientSecrets = { new Secret("secret".Sha256()) },

                    AllowedGrantTypes = GrantTypes.Code,
                    RequireConsent = false,
                    RequirePkce = true,

                    AllowedCorsOrigins = new string[] {"https://localhost:44338" },

                    // where to redirect to after login
                    RedirectUris = { "https://localhost:44338/authentication/login-callback" },

                    // where to redirect to after logout
                    PostLogoutRedirectUris = { "https://localhost:44338/authentication/logout-callback" },


                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "roles",
                        "Hr.PersonApi1.*"
                    },

                    AllowOfflineAccess = true,
                    ClientClaimsPrefix = "",
                },
        new Client
        {
            ClientId = "mvc",
            ClientSecrets = { new Secret("secret".Sha256()) },

            AllowedGrantTypes = GrantTypes.Code,

            // where to redirect to after login
            RedirectUris = { "https://localhost:6002/signin-oidc" },

            // where to redirect to after logout
            PostLogoutRedirectUris = { "https://localhost:6002/signout-callback-oidc" },

            AllowedScopes = new List<string>
            {
                IdentityServerConstants.StandardScopes.OpenId,
                IdentityServerConstants.StandardScopes.Profile
            }
        }
            };
    }
}
