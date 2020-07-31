using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Linq.Expressions;
using System.Text.Json;

namespace EDennis.NetStandard.Base {

    /// <summary>
    /// Used to handle serialization/deserialization of JSON strings
    /// stored in an object property (table/view column).  
    /// Specific uses in this library:
    /// <list type="bullet">
    /// <item>AspNetExpandedUsers.ClaimsDictionary (SQL View Column) <=> ExpandedDomainUser.ClaimsDictionary (Object Property)</item>
    /// <item>AspNetExpandedUsers.RolesDictionary (SQL View Column) <=> ExpandedDomainUser.RolesDictionary (Object Property)</item>
    /// </list>
    /// </summary>
    /// <typeparam name="T">The CLR type to convert the JSON string from/to </typeparam>
    public class SerializerConverter<T> : ValueConverter<T, string> 
        where T : class, new() {
        public SerializerConverter(ConverterMappingHints mappingHints = default) :
            base(Serialize, Deserialize, mappingHints){ }

        static readonly Expression<Func<T, string>> Serialize = x =>
            JsonSerializer.Serialize(x, default);

        static readonly Expression<Func<string, T>> Deserialize = x =>
            JsonSerializer.Deserialize<T>(x, default);
    }
}
