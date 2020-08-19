using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EDennis.NetStandard.Base
{
    public class DefaultPoliciesOptions{
        public const string DEFAULT_CLAIMTYPES_KEY = "Security:DefaultPolicies:ClaimTypes";
        public const string DEFAULT_POLICIES_KEY = "Security:DefaultPolicies:Policies";

        //public DefaultPolicies(list)

        //public void Load(Assembly assembly) {

        //    var models = assembly.GetTypes()
        //        .Where(type => typeof(ControllerBase).IsAssignableFrom(type)) //filter controllers
        //        .SelectMany(type => type.GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public))
        //        .Where(method => method.IsPublic && !method.IsDefined(typeof(NonActionAttribute))
        //            && !method.GetCustomAttributes(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), true).Any())
        //        .Select(x => KeyValuePair.Create(x.DeclaringType.Name, x.Name))
        //        .ToDictionary(x => (x.Key, x.Value));

        //    var project = assembly.GetName().Name;

        //    foreach (var controller in models.Keys) {
        //        foreach (var action in actions) {
        //            Add($"{project}.{controller}.{action}");
        //        }
        //    }

        //    return scopes;
        //}

    }
}
