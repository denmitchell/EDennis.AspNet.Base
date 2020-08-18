using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace EDennis.NetStandard.Base {

    /// <summary>
    /// Produces a static cache of Linq property accessor components
    /// (ParameterExpression and MemberExpression) for string properties of
    /// any class.
    /// Adapted from https://nejcskofic.github.io/2017/03/20/dynamic-query-expressions-with-entity-framework/
    /// </summary>
    /// <typeparam name="T">Class with one or more string properties</typeparam>
    public static class StringPropertyAccessorComponentCache<T> where T : class {
        private static readonly IDictionary<string, MemberExpression> _cache;
        private static readonly ParameterExpression _parameterExpression;
        static StringPropertyAccessorComponentCache() {
            var storage = new Dictionary<string, MemberExpression>();

            var t = typeof(T);
            _parameterExpression = Expression.Parameter(t, "e");
            foreach (var property in t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p=>p.PropertyType == typeof(string))) {
                var memberExpression = Expression.MakeMemberAccess(_parameterExpression, property);
                storage[property.Name] = memberExpression;
            }

            _cache = storage;
        }

        public static (ParameterExpression,MemberExpression) Get(string propertyName) {
            return _cache.TryGetValue(propertyName, out MemberExpression result) ? (_parameterExpression,result) : (default,default);
        }

    }
}
