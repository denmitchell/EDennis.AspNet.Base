using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Linq.Expressions;
using System.Text.Json;

namespace EDennis.AspNet.Base.Security {
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
