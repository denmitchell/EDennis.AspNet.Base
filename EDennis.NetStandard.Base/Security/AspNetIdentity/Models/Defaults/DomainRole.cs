using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EDennis.NetStandard.Base {
    

    [JsonConverter(typeof(DomainRoleJsonConverter))]
    public class DomainRole : DomainRole<DomainUser, DomainOrganization, DomainUserClaim, DomainUserLogin, DomainUserToken, DomainRole, DomainApplication, DomainRoleClaim, DomainUserRole> {
        
        public override void Patch(JsonElement jsonElement, ModelStateDictionary modelState) {
            bool normalizedNameProvided = false;
            foreach (var prop in jsonElement.EnumerateObject()) {
                switch (prop.Name) {
                    case "Id":
                    case "id":
                        Id = prop.Value.GetGuid();
                        break;
                    case "Name":
                    case "name":
                        Name = prop.Value.GetString();
                        break;
                    case "NormalizedName":
                    case "normalizedName":
                        NormalizedName = prop.Value.GetString();
                        normalizedNameProvided = true;
                        break;
                    case "ApplicationId":
                    case "applicationId":
                        ApplicationId = prop.Value.GetGuid();
                        break;
                    case "SysUser":
                    case "sysUser":
                        SysUser = prop.Value.GetString();
                        break;
                    case "SysStatus":
                    case "sysStatus":
                        SysStatus = (SysStatus)Enum.Parse(typeof(SysStatus), prop.Value.GetString());
                        break;
                }
            }
            if (!normalizedNameProvided)
                NormalizedName = Name.ToUpper();
        }

        public override void Update(object updated) {
            var entity = updated as DomainRole;
            Id = entity.Id;
            Name = entity.Name;
            NormalizedName = entity.NormalizedName ?? entity.Name.ToUpper();
            ApplicationId = entity.ApplicationId;
            SysUser = entity.SysUser;
            SysStatus = entity.SysStatus;
        }

    }


    public class DomainRoleJsonConverter : JsonConverter<DomainRole> {

        public override DomainRole Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            JsonSerializer.Deserialize<DomainRole>(ref reader, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        public override void Write(Utf8JsonWriter writer, DomainRole value, JsonSerializerOptions options) {
            writer.WriteStartObject();
            {
                writer.WriteString("Id", value.Id.ToString());
                writer.WriteString("Name", value.Name);
                writer.WriteString("NormalizedName", value.NormalizedName);
                writer.WriteString("SysUser", value.SysUser);
                writer.WriteString("SysStatus", value.SysStatus.ToString());
                writer.WriteString("SysStart", value.SysStart.ToString("u"));
                writer.WriteString("SysEnd", value.SysStart.ToString("u"));
                writer.WriteString("ApplicationId", value.ApplicationId.ToString());
                if (value.Application != null)
                    writer.WriteString("ApplicationName", value.Application.Name);
            }
            writer.WriteEndObject();
        }
    }

}