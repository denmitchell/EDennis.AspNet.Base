using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.Json;

namespace EDennis.AspNet.Base.Security {
    public class DomainUserExpansion : DomainUser {

        public string OrganizationName { get; set; }
        public string ApplicationName { get; set; }
        
        //.HasConversion(new DictionaryConverter())
        public Dictionary<string,string[]> Claims { get; set; }
        //.HasConversion(new DictionaryConverter())
        public Dictionary<string,string[]> Roles { get; set; }

    }


    public class DictionaryConverter : ValueConverter<Dictionary<string, string[]>, string> {
        public DictionaryConverter(ConverterMappingHints mappingHints = default) :
            base(Serialize, Deserialize, mappingHints){ }

        static readonly Expression<Func<Dictionary<string, string[]>, string>> Serialize = x =>
            JsonSerializer.Serialize(x, default);

        static readonly Expression<Func<string, Dictionary<string, string[]>>> Deserialize = x =>
            JsonSerializer.Deserialize<Dictionary<string,string[]>>(x, default);
    }
}
