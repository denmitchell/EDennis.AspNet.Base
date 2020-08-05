using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using M = IdentityServer4.Models;

namespace EDennis.AspNet.Base {
    public static class ApiScopeModelExtensions {

        public static void Patch(this M.ApiScope model, JsonElement partialModel,
            ModelStateDictionary modelState,  bool mergeCollections = true) {

            foreach (var prop in partialModel.EnumerateObject()) {
                try {
                    switch (prop.Name) {
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
