using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EDennis.AspNet.Base.Security {
    public class DomainUserClaim : IdentityUserClaim<Guid>, ITemporalEntity {

        public DomainUser User { get; set; }


        public SysStatus SysStatus { get; set; }
        public string SysUser { get; set; }
        public DateTime SysStart { get; set; }
        public DateTime SysEnd { get; set; }


        public void Patch(JsonElement jsonElement) {
            throw new NotImplementedException();
        }

        public void Update(object updated) {
            throw new NotImplementedException();
        }

    }


    public class DomainUserClaimJsonConverter : JsonConverter<DomainUserClaim> {
        public override DomainUserClaim Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, DomainUserClaim value, JsonSerializerOptions options) {
            writer.WriteStartObject();
            {
                writer.WriteNumber("Id", value.Id);
                writer.WriteString("ClaimType", value.ClaimType);
                writer.WriteString("ClaimValue", value.ClaimValue);
                if(value.Properties != null && value.Properties.Length > 0)
                writer.WriteString("Properties", value.Properties);
            }
        }
    }

}
