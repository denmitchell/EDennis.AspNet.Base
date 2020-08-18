using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace EDennis.NetStandard.Base {
    public static class IQueryableExtensions_DynamicLinqStringSearch {


        /// <summary>
        /// Dynamically constructs a case-insensitive StartsWith condition in a Linq Where.
        /// </summary>
        /// <typeparam name="T">Class with one or more string properties</typeparam>
        /// <param name="source">Queryable to apply condition to</param>
        /// <param name="propertyName">string property to apply condition to</param>
        /// <param name="substring">substring to match</param>
        /// <returns></returns>
        /// <see cref="WhereEndsWith{T}(IQueryable{T}, string, string)"/>
        /// <see cref="WhereContains{T}(IQueryable{T}, string, string)"/>
        /// <see cref="WhereLike{T}(IQueryable{T}, string, string)"/>
        /// <see cref="StringPropertyAccessorComponentCache{T}"/>
        public static IQueryable<T> WhereStartsWith<T>(this IQueryable<T> source, string propertyName, string substring)
            where T : class => WhereLike(source, propertyName, $"{substring}%");



        /// <summary>
        /// Dynamically constructs a case-insensitive EndsWith condition in a Linq Where.
        /// </summary>
        /// <typeparam name="T">Class with one or more string properties</typeparam>
        /// <param name="source">Queryable to apply condition to</param>
        /// <param name="propertyName">string property to apply condition to</param>
        /// <param name="substring">substring to match</param>
        /// <returns></returns>
        /// <returns></returns>
        /// <see cref="WhereStartsWith{T}(IQueryable{T}, string, string)"/>
        /// <see cref="WhereContains{T}(IQueryable{T}, string, string)"/>
        /// <see cref="WhereLike{T}(IQueryable{T}, string, string)"/>
        /// <see cref="StringPropertyAccessorComponentCache{T}"/>
        public static IQueryable<T> WhereEndsWith<T>(this IQueryable<T> source, string propertyName, string substring)
            where T : class => WhereLike(source, propertyName, $"%{substring}");



        /// <summary>
        /// Dynamically constructs a case-insensitive Contains condition in a Linq Where,
        /// where the Contains condition is applied to a substring within a string.
        /// </summary>
        /// <typeparam name="T">Class with one or more string properties</typeparam>
        /// <param name="source">Queryable to apply condition to</param>
        /// <param name="propertyName">string property to apply condition to</param>
        /// <param name="substring">substring to match</param>
        /// <returns></returns>
        /// <see cref="WhereStartsWith{T}(IQueryable{T}, string, string)"/>
        /// <see cref="WhereEndsWith{T}(IQueryable{T}, string, string)"/>
        /// <see cref="WhereLike{T}(IQueryable{T}, string, string)"/>
        /// <see cref="StringPropertyAccessorComponentCache{T}"/>
        public static IQueryable<T> WhereContains<T>(this IQueryable<T> source, string propertyName, string substring)
            where T : class => WhereLike(source, propertyName, $"%{substring}%");



        /// <summary>
        /// Dynamically constructs a EF.Functions.Like clause within a Linq Where.
        /// This method can be used as a case-insensitive replacement for
        /// Contains, StartsWith, and EndsWith methods in System.Linq.Dynamic.Core
        /// </summary>
        /// <typeparam name="T">Class with one or more string properties</typeparam>
        /// <param name="source">Queryable to apply condition to</param>
        /// <param name="propertyName">string property to apply LIKE clause to</param>
        /// <param name="pattern">LIKE pattern</param>
        /// <returns></returns>
        /// <see cref="WhereStartsWith{T}(IQueryable{T}, string, string)"/>
        /// <see cref="WhereEndsWith{T}(IQueryable{T}, string, string)"/>
        /// <see cref="WhereContains{T}(IQueryable{T}, string, string)"/>
        /// <see cref="StringPropertyAccessorComponentCache{T}"/>
        public static IQueryable<T> WhereLike<T>(this IQueryable<T> source, string propertyName, string pattern) 
            where T : class {
           
            var (parameterExpression, memberExpression) = StringPropertyAccessorComponentCache<T>.Get(propertyName);

            if (memberExpression == default)
                throw new ArgumentException($"Unable to compile WhereLike: Property name {propertyName} not found in {typeof(T).Name}.");

            var patternExpression = Expression.Constant(pattern);


            var likeCall = Expression.Call(
                typeof(DbFunctionsExtensions), "Like", Type.EmptyTypes,
                Expression.Constant(EF.Functions), memberExpression, patternExpression);


            var whereExpression = Expression.Lambda(likeCall, parameterExpression);

            var whereCall = Expression.Call(
                typeof(Queryable),
                "Where",
                new Type[] { source.ElementType },
                source.Expression,
                whereExpression
                );

            
            var qry = source.Provider.CreateQuery<T>(whereCall);

            return qry;

        }

    }
}
