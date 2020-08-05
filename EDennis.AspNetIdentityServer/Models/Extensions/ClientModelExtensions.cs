using IdentityModel;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using M = IdentityServer4.Models;

namespace EDennis.AspNetIdentityServer {
    public static class ClientModelExtensions {

        public static void Patch(this M.Client model, JsonElement partialModel,
            ModelStateDictionary modelState,  bool mergeCollections = true) {

            foreach (var prop in partialModel.EnumerateObject()) {
                try {
                    switch (prop.Name) {
                        case "AbsoluteRefreshTokenLifetime":
                        case "absoluteRefreshTokenLifetime":
                            model.AbsoluteRefreshTokenLifetime = prop.Value.GetInt32();
                            break;
                        case "AccessTokenLifetime":
                        case "accessTokenLifetime":
                            model.AccessTokenLifetime = prop.Value.GetInt32();
                            break;
                        case "AccessTokenType":
                        case "accessTokenType":
                            model.AccessTokenType = (M.AccessTokenType)Enum.Parse(typeof(M.AccessTokenType), prop.Value.GetString());
                            break;
                        case "AllowAccessTokensViaBrowser":
                        case "allowAccessTokensViaBrowser":
                            model.AllowAccessTokensViaBrowser = prop.Value.GetBoolean();
                            break;
                        case "AllowedCorsOrigins":
                        case "allowedCorsOrigins":
                            var allowedCorsOrigins = prop.Value.EnumerateArray().Select(e => e.GetString()).ToArray();
                            if (mergeCollections && model.AllowedCorsOrigins != null)
                                model.AllowedCorsOrigins = model.AllowedCorsOrigins.Union(allowedCorsOrigins).ToArray();
                            else
                                model.AllowedCorsOrigins = allowedCorsOrigins.ToArray();
                            break;
                        case "AllowedGrantTypes":
                        case "allowedGrantTypes":
                            var allowedGrantTypes = prop.Value.EnumerateArray().Select(e => e.GetString()).ToArray();
                            if (mergeCollections && model.AllowedGrantTypes != null)
                                model.AllowedGrantTypes = model.AllowedGrantTypes.Union(allowedGrantTypes).ToArray();
                            else
                                model.AllowedGrantTypes = allowedGrantTypes.ToArray();
                            break;
                        case "AllowedScopes":
                        case "allowedScopes":
                            var allowedScopes = prop.Value.EnumerateArray().Select(e => e.GetString()).ToArray();
                            if (mergeCollections && model.AllowedScopes != null)
                                model.AllowedScopes = model.AllowedScopes.Union(allowedScopes).ToArray();
                            else
                                model.AllowedScopes = allowedScopes.ToArray();
                            break;
                        case "AllowOfflineAccess":
                        case "allowOfflineAccess":
                            model.AllowOfflineAccess = prop.Value.GetBoolean();
                            break;
                        case "AllowPlainTextPkce":
                        case "allowPlainTextPkce":
                            model.AllowPlainTextPkce = prop.Value.GetBoolean();
                            break;
                        case "AllowRememberConsent":
                        case "allowRememberConsent":
                            model.AllowRememberConsent = prop.Value.GetBoolean();
                            break;
                        case "AlwaysIncludeUserClaimsInIdToken":
                        case "alwaysIncludeUserClaimsInIdToken":
                            model.AlwaysIncludeUserClaimsInIdToken = prop.Value.GetBoolean();
                            break;
                        case "AlwaysSendClientClaims":
                        case "alwaysSendClientClaims":
                            model.AlwaysSendClientClaims = prop.Value.GetBoolean();
                            break;
                        case "AuthorizationCodeLifetime":
                        case "authorizationCodeLifetime":
                            model.AuthorizationCodeLifetime = prop.Value.GetInt32();
                            break;
                        case "BackChannelLogoutSessionRequired":
                        case "backChannelLogoutSessionRequired":
                            model.BackChannelLogoutSessionRequired = prop.Value.GetBoolean();
                            break;
                        case "BackChannelLogoutUri":
                        case "backChannelLogoutUri":
                            model.BackChannelLogoutUri = prop.Value.GetString();
                            break;
                        case "Claims":
                        case "claims":
                            var claims = prop.Value.EnumerateArray().Select(e => {
                                var claim = new M.ClientClaim();
                                foreach (var prop2 in e.EnumerateObject()) {
                                    switch (prop2.Name) {
                                        case "Type":
                                        case "type":
                                            claim.Type = prop2.Value.GetString();
                                            break;
                                        case "Value":
                                        case "value":
                                            claim.Value = prop2.Value.GetString();
                                            break;
                                        default:
                                            break;
                                    }
                                }
                                return claim;
                            });
                            if (mergeCollections && model.Claims != null) {
                                var newClaims = claims.Where(n => !model.Claims.Any(e => e.Type == n.Type && e.Value == n.Value));
                                model.Claims = model.Claims.Union(newClaims).ToArray();
                            } else
                                model.Claims = claims.ToArray();
                            break;
                        case "ClientClaimsPrefix":
                        case "clientClaimsPrefix":
                            model.ClientClaimsPrefix = prop.Value.GetString();
                            break;
                        case "ClientId":
                        case "clientId":
                            model.ClientId = prop.Value.GetString();
                            break;
                        case "ClientName":
                        case "clientName":
                            model.ClientName = prop.Value.GetString();
                            break;
                        case "ClientSecrets":
                        case "clientSecrets":
                            var secrets = prop.Value.EnumerateArray().Select(e => {
                                var secret = new M.Secret {
                                    Type = "SharedSecret",
                                    Expiration = DateTime.MaxValue
                                };
                                foreach (var prop2 in e.EnumerateObject()) {
                                    switch (prop2.Name) {
                                        case "Value":
                                        case "value":
                                            secret.Value = prop2.Value.GetString().ToSha256();
                                            break;
                                        case "Type":
                                        case "type":
                                            secret.Type = prop2.Value.GetString();
                                            break;
                                        case "Description":
                                        case "description":
                                            secret.Description = prop2.Value.GetString();
                                            break;
                                        case "Expiration":
                                        case "expiration":
                                            secret.Expiration = prop2.Value.GetDateTime();
                                            break;
                                        default:
                                            break;
                                    }
                                }
                                return secret;
                            });
                            if (mergeCollections && model.ClientSecrets != null) {
                                var newSecrets = secrets.Where(n => !model.ClientSecrets.Any(e => e.Value == n.Value && e.Expiration == n.Expiration));
                                model.ClientSecrets = model.ClientSecrets.Union(secrets).ToArray();
                            } else
                                model.ClientSecrets = secrets.ToArray();
                            break;
                        case "ClientUri":
                        case "clientUri":
                            model.ClientUri = prop.Value.GetString();
                            break;
                        case "ConsentLifetime":
                        case "consentLifetime":
                            model.ConsentLifetime = prop.Value.GetInt32();
                            break;
                        case "Description":
                        case "description":
                            model.Description = prop.Value.GetString();
                            break;
                        case "DeviceCodeLifetime":
                        case "deviceCodeLifetime":
                            model.DeviceCodeLifetime = prop.Value.GetInt32();
                            break;
                        case "Enabled":
                        case "enabled":
                            model.Enabled = prop.Value.GetBoolean();
                            break;
                        case "EnableLocalLogin":
                        case "enableLocalLogin":
                            model.EnableLocalLogin = prop.Value.GetBoolean();
                            break;
                        case "FrontChannelLogoutSessionRequired":
                        case "frontChannelLogoutSessionRequired":
                            model.FrontChannelLogoutSessionRequired = prop.Value.GetBoolean();
                            break;
                        case "FrontChannelLogoutUri":
                        case "frontChannelLogoutUri":
                            model.FrontChannelLogoutUri = prop.Value.GetString();
                            break;
                        case "IdentityProviderRestrictions":
                        case "identityProviderRestrictions":
                            var identityProviderRestrictions = prop.Value.EnumerateArray().Select(e => e.GetString()).ToArray();
                            if (mergeCollections && model.IdentityProviderRestrictions != null)
                                model.IdentityProviderRestrictions = model.IdentityProviderRestrictions.Union(identityProviderRestrictions).ToArray();
                            else
                                model.IdentityProviderRestrictions = identityProviderRestrictions.ToArray();
                            break;
                        case "IdentityTokenLifetime":
                        case "identityTokenLifetime":
                            model.IdentityTokenLifetime = prop.Value.GetInt32();
                            break;
                        case "IncludeJwtId":
                        case "includeJwtId":
                            model.IncludeJwtId = prop.Value.GetBoolean();
                            break;
                        case "LogoUri":
                        case "logoUri":
                            model.LogoUri = prop.Value.GetString();
                            break;
                        case "PairWiseSubjectSalt":
                        case "pairWiseSubjectSalt":
                            model.PairWiseSubjectSalt = prop.Value.GetString();
                            break;
                        case "PostLogoutRedirectUris":
                        case "postLogoutRedirectUris":
                            var postLogoutRedirectUris = prop.Value.EnumerateArray().Select(e => e.GetString()).ToArray();
                            if (mergeCollections && model.PostLogoutRedirectUris != null)
                                model.PostLogoutRedirectUris = model.PostLogoutRedirectUris.Union(postLogoutRedirectUris).ToArray();
                            else
                                model.PostLogoutRedirectUris = postLogoutRedirectUris.ToArray();
                            break;
                        case "Properties":
                        case "properties":
                            var properties = new Dictionary<string, string>();
                            prop.Value.EnumerateObject().ToList().ForEach(e => {
                                properties.Add(e.Name, e.Value.GetString());
                            });
                            if (mergeCollections && model.Properties != null)
                                foreach (var entry in properties)
                                    if (model.Properties.ContainsKey(entry.Key))
                                        model.Properties[entry.Key] = entry.Value;
                                    else
                                        model.Properties.Add(entry.Key, entry.Value);
                            else
                                model.Properties = properties;
                            break;
                        case "ProtocolType":
                        case "protocolType":
                            model.ProtocolType = prop.Value.GetString();
                            break;
                        case "RedirectUris":
                        case "redirectUris":
                            var redirectUris = prop.Value.EnumerateArray().Select(e => e.GetString()).ToArray();
                            if (mergeCollections && model.RedirectUris != null)
                                model.RedirectUris = model.RedirectUris.Union(redirectUris).ToArray();
                            else
                                model.RedirectUris = redirectUris.ToArray();
                            break;
                        case "RefreshTokenExpiration":
                        case "refreshTokenExpiration":
                            model.RefreshTokenExpiration = (M.TokenExpiration)Enum.Parse(typeof(M.TokenExpiration), prop.Value.GetString());
                            break;
                        case "RefreshTokenUsage":
                        case "refreshTokenUsage":
                            model.RefreshTokenUsage = (M.TokenUsage)Enum.Parse(typeof(M.TokenUsage), prop.Value.GetString());
                            break;
                        case "RequireClientSecret":
                        case "requireClientSecret":
                            model.RequireClientSecret = prop.Value.GetBoolean();
                            break;
                        case "RequireConsent":
                        case "requireConsent":
                            model.RequireConsent = prop.Value.GetBoolean();
                            break;
                        case "RequirePkce":
                        case "requirePkce":
                            model.RequirePkce = prop.Value.GetBoolean();
                            break;
                        case "RequireRequestObject":
                        case "requireRequestObject":
                            model.RequireRequestObject = prop.Value.GetBoolean();
                            break;
                        case "SlidingRefreshTokenLifetime":
                        case "slidingRefreshTokenLifetime":
                            model.SlidingRefreshTokenLifetime = prop.Value.GetInt32();
                            break;
                        case "UpdateAccessTokenClaimsOnRefresh":
                        case "updateAccessTokenClaimsOnRefresh":
                            model.UpdateAccessTokenClaimsOnRefresh = prop.Value.GetBoolean();
                            break;
                        case "UserCodeType":
                        case "userCodeType":
                            model.UserCodeType = prop.Value.GetString();
                            break;
                        case "UserSsoLifetime":
                        case "userSsoLifetime":
                            model.UserSsoLifetime = prop.Value.GetInt32();
                            break;
                        default:
                            break;
                    }
                } catch (InvalidOperationException ex) {
                    modelState.AddModelError(prop.Name, ex.Message);
                }
            }

        }
    }
}
