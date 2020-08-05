using EDennis.NetStandard.Base;
using IdentityModel;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using M = IdentityServer4.Models;

namespace EDennis.AspNetIdentityServer {
    public static class ApiResourceModelExtensions {

        public static M.ApiResource ToModel(this ApiResourceOptions options) {
            var resource = new M.ApiResource {
                Name = options.Name,
                Description = options.Name,
                Scopes = options.Scopes,
                UserClaims = options.UserClaims
            };
            return resource;
        }

        public static void Patch(this M.ApiResource model, JsonElement partialModel,
            ModelStateDictionary modelState, bool mergeCollections = true) {

            foreach (var prop in partialModel.EnumerateObject()) {
                try {
                    switch (prop.Name) {
                        case "AllowedAccessTokenSigningAlgorithms":
                        case "allowedAccessTokenSigningAlgorithms":
                            var allowedAccessTokenSigningAlgorithms = prop.Value.EnumerateArray().Select(e => e.GetString()).ToArray();
                            if (mergeCollections && model.AllowedAccessTokenSigningAlgorithms != null)
                                model.AllowedAccessTokenSigningAlgorithms = model.AllowedAccessTokenSigningAlgorithms.Union(allowedAccessTokenSigningAlgorithms).ToArray();
                            else
                                model.AllowedAccessTokenSigningAlgorithms = allowedAccessTokenSigningAlgorithms.ToArray();
                            break;
                        case "ApiSecrets":
                        case "apiSecrets":
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
                            if (mergeCollections && model.ApiSecrets != null) {
                                var newSecrets = secrets.Where(n => !model.ApiSecrets.Any(e => e.Value == n.Value && e.Expiration == n.Expiration));
                                model.ApiSecrets = model.ApiSecrets.Union(newSecrets).ToArray();
                            } else
                                model.ApiSecrets = secrets.ToArray();
                            break;
                        case "Description":
                        case "description":
                            model.Description = prop.Value.GetString();
                            break;
                        case "DisplayName":
                        case "displayName":
                            model.DisplayName = prop.Value.GetString();
                            break;
                        case "Enabled":
                        case "enabled":
                            model.Enabled = prop.Value.GetBoolean();
                            break;
                        case "Name":
                        case "name":
                            model.Name = prop.Value.GetString();
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
                        case "Scopes":
                        case "scopes":
                            var scopes = prop.Value.EnumerateArray().Select(e => e.GetString()).ToArray();
                            if (mergeCollections && model.Scopes != null)
                                model.Scopes = model.Scopes.Union(scopes).ToArray();
                            else
                                model.Scopes = scopes.ToArray();
                            break;
                        case "ShowInDiscoveryDocument":
                        case "showInDiscoveryDocument":
                            model.ShowInDiscoveryDocument = prop.Value.GetBoolean();
                            break;
                        case "UserClaims":
                        case "userClaims":
                            var userClaims = prop.Value.EnumerateArray().Select(e => e.GetString()).ToArray();
                            if (mergeCollections && model.UserClaims != null)
                                model.UserClaims = model.UserClaims.Union(userClaims).ToArray();
                            else
                                model.UserClaims = userClaims.ToArray();
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