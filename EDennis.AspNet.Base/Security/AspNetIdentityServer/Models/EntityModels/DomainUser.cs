using IdentityServer4.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EDennis.AspNet.Base.Security {

    [JsonConverter(typeof(DomainUserJsonConverter))]
    public class DomainUser : IdentityUser<Guid>, IDomainEntity, IHasStringProperties {

        public const int SHA256_LENGTH = 64; //"a8a2f6ebe286697c527eb35a58b5539532e9b3ae3b64d4eb0a46fb657b41562c";
        public const int SHA512_LENGTH = 128; //"f3bf9aa70169e4ab5339f20758986538fe6c96d7be3d184a036cde8161105fcf53516428fa096ac56247bb88085b0587d5ec8e56a6807b1af351305b2103d74b";
        public Guid OrganizationId { get; set; }

        public DateTimeOffset? LockoutBegin { get; set; }

        [NotMapped]
        public override bool LockoutEnabled {
            get => LockoutBegin <= DateTime.Now && LockoutEnd > DateTime.Now;
            set {
                if (value) {
                    LockoutBegin = DateTime.Now;
                    if (LockoutEnd == null || LockoutEnd < DateTime.Now)
                        LockoutEnd = DateTime.MaxValue;
                } else {
                    LockoutBegin = null;
                    LockoutEnd = null;
                }
            }
        }

        private string _passwordHash;


        public override string PasswordHash { 
            get => _passwordHash; 
            set {

                if (value.Length == SHA256_LENGTH || value.Length == SHA512_LENGTH)
                    _passwordHash = value;
                else
                    _passwordHash = value.Sha256();
                
            }
        }



        public string Properties { get; set; }

        public DomainOrganization Organization { get; set; }
        public ICollection<DomainUserClaim> UserClaims { get; set; }
        public ICollection<DomainUserRole> UserRoles { get; set; }
        public ICollection<DomainUserLogin> UserLogins { get; set; }
        public ICollection<DomainUserToken> UserTokens { get; set; }
        public DateTime SysEnd { get; set; }
        public DateTime SysStart { get; set; }
        public SysStatus SysStatus { get; set; }
        public string SysUser { get; set; }


        public void DeserializeInto(JsonElement source, ModelStateDictionary modelState) {
            bool hasWrittenProperties = false;
            using var ms = new MemoryStream();
            using var jw = new Utf8JsonWriter(ms);

            foreach (var prop in source.EnumerateObject()) {
                try {
                    switch (prop.Name) {
                        case "AccessFailedCount":
                        case "accessFailedCount":
                            AccessFailedCount = prop.Value.GetInt32();
                            break;
                        case "ConcurrencyStamp":
                        case "concurrencyStamp":
                            ConcurrencyStamp = prop.Value.GetString();
                            break;
                        case "Email":
                        case "email":
                            Email = prop.Value.GetString();
                            NormalizedEmail = Email.ToUpper();
                            break;
                        case "EmailConfirmed":
                        case "emailConfirmed":
                            EmailConfirmed = prop.Value.GetBoolean();
                            break;
                        case "Id":
                        case "id":
                            Id = prop.Value.GetGuid();
                            break;
                        case "LockoutBegin":
                        case "lockoutBegin":
                            LockoutBegin = prop.Value.GetDateTime();
                            break;
                        case "LockoutEnabled":
                        case "lockoutEnabled":
                            LockoutEnabled = prop.Value.GetBoolean();
                            break;
                        case "LockoutEnd":
                        case "lockoutEnd":
                            LockoutEnd = prop.Value.GetDateTime();
                            break;
                        case "NormalizedEmail":
                        case "normalizedEmail":
                            NormalizedEmail = prop.Value.GetString();
                            break;
                        case "NormalizedUserName":
                        case "normalizedUserName":
                            NormalizedUserName = prop.Value.GetString();
                            break;
                        case "OrganizationId":
                        case "organizationId":
                            OrganizationId = prop.Value.GetGuid();
                            break;
                        case "PasswordHash":
                        case "passwordHash":
                            PasswordHash = prop.Value.GetString();
                            break;
                        case "PhoneNumber":
                        case "phoneNumber":
                            PhoneNumber = prop.Value.GetString();
                            break;
                        case "PhoneNumberConfirmed":
                        case "phoneNumberConfirmed":
                            PhoneNumberConfirmed = prop.Value.GetBoolean();
                            break;
                        case "SecurityStamp":
                        case "securityStamp":
                            SecurityStamp = prop.Value.GetString();
                            break;
                        case "SysUser":
                        case "sysUser":
                            SysUser = prop.Value.GetString();
                            break;
                        case "SysStatus":
                        case "sysStatus":
                            SysStatus = (SysStatus)Enum.Parse(typeof(SysStatus), prop.Value.GetString());
                            break;
                        case "SysStart":
                        case "sysStart":
                            SysStart = prop.Value.GetDateTime();
                            break;
                        case "SysEnd":
                        case "sysEnd":
                            SysEnd = prop.Value.GetDateTime();
                            break;
                        case "TwoFactorEnabled":
                        case "twoFactorEnabled":
                            TwoFactorEnabled = prop.Value.GetBoolean();
                            break;
                        case "UserName":
                        case "userName":
                            UserName = prop.Value.GetString();
                            NormalizedUserName = UserName.ToUpper();
                            break;
                        case "UserClaims__Packed":
                        case "userClaims__Packed":
                            var unpackedClaims = new List<DomainUserClaim>();
                            foreach (var packedClaim in prop.Value.EnumerateObject())
                                foreach (var packedValue in packedClaim.Value.EnumerateArray())
                                    unpackedClaims.Add(new DomainUserClaim { ClaimType = packedClaim.Name, ClaimValue = packedValue.GetString() });
                            UserClaims = unpackedClaims.ToArray();
                            break;
                        case "UserClaims":
                        case "userClaims":
                            var claims = new List<DomainUserClaim>();
                            foreach (var item in prop.Value.EnumerateArray()) {
                                var claim = new DomainUserClaim();
                                claim.DeserializeInto(item, modelState);
                                claims.Add(claim);
                            }
                            UserClaims = claims.ToArray();
                            break;
                        case "UserRoles":
                        case "userRoles":
                            var roles = prop.Value.EnumerateArray()
                                .Select(e => DomainUserRole.DeserializeInto(e, new DomainUserRole(), modelState));
                            if (UserRoles != null) {
                                var newRoles = roles.Where(n => !UserRoles.Any(e => e.RoleId == n.RoleId));
                                UserRoles = UserRoles.Union(newRoles).ToArray();
                            } else
                                UserRoles = roles.ToArray();
                            break;
                        case "UserLogins":
                        case "userLogins":
                            var logins = prop.Value.EnumerateArray()
                                .Select(e => DomainUserLogin.DeserializeInto(e, new DomainUserLogin(), modelState));
                            if (UserLogins != null) {
                                var newLogins = logins.Where(n => !UserLogins.Any(e => e.LoginProvider == n.LoginProvider && e.ProviderKey == n.ProviderKey));
                                UserLogins = UserLogins.Union(newLogins).ToArray();
                            } else
                                UserLogins = logins.ToArray();
                            break;
                        case "UserTokens":
                        case "userTokens":
                            var tokens = prop.Value.EnumerateArray()
                                .Select(e => DomainUserToken.DeserializeInto(e, new DomainUserToken(), modelState));                            
                            if (UserTokens != null) {
                                var newTokens = tokens.Where(n => !UserTokens.Any(e => e.LoginProvider == n.LoginProvider && e.Name == n.Name));
                                UserTokens = UserTokens.Union(newTokens).ToArray();
                            } else
                                UserTokens = tokens.ToArray();
                            break;
                        default:
                            if (!hasWrittenProperties)
                                jw.WriteStartObject();
                            prop.WriteTo(jw);
                            break;
                    }
                } catch (InvalidOperationException ex) {
                    modelState.AddModelError(prop.Name, $"{ex.Message}: Cannot parse value for {prop.Value} from {GetType().Name} JSON");
                }
            }
        }
    }

    public class DomainUserJsonConverter : JsonConverter<DomainUser> {
        public override DomainUser Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => JsonSerializer.Deserialize<DomainUser>(ref reader, options);

        public override void Write(Utf8JsonWriter writer, DomainUser value, JsonSerializerOptions options) {
            writer.WriteStartObject();
            {
                writer.WriteString("Id", value.Id.ToString());
                writer.WriteString("UserName", value.UserName);
                if (value.AccessFailedCount != default)
                    writer.WriteNumber("AccessFailedCount", value.AccessFailedCount);
                writer.WriteString("Email", value.Email);
                writer.WriteBoolean("EmailConfirmed", value.EmailConfirmed);
                writer.WriteBoolean("LockoutEnabled", value.LockoutEnabled);
                if(value.LockoutBegin != default)
                    writer.WriteString("LockoutBegin", value.LockoutBegin.Value.ToString("u"));
                if (value.LockoutEnd != default)
                    writer.WriteString("LockoutEnd", value.LockoutEnd.Value.ToString("u"));
                if (value.OrganizationId != default)
                    writer.WriteString("OrganizationId", value.OrganizationId.ToString());
                if (value.Organization != null)
                    writer.WriteString("OrganizationName", value.Organization.Name);
                if (value.PhoneNumber != default)
                    writer.WriteString("PhoneNumber",value.PhoneNumber);
                if (value.PhoneNumberConfirmed != default)
                    writer.WriteBoolean("PhoneNumberConfirmed", value.PhoneNumberConfirmed);
                if (value.TwoFactorEnabled != default)
                    writer.WriteBoolean("TwoFactorEnabled", value.TwoFactorEnabled);
                if(value.UserClaims != null && value.UserClaims.Count > 0) {
                    var dict = value.UserClaims.ToDictionary();
                    writer.WriteStartObject("Claims");
                    {
                        foreach (var entry in dict) {
                            writer.WritePropertyName(entry.Key);
                            writer.WriteStartArray();
                            {
                                foreach (var item in entry.Value)
                                    writer.WriteStringValue(item);
                            }
                            writer.WriteEndArray();
                        }
                    }
                    writer.WriteEndObject();
                }
                if (value.UserRoles != null && value.UserRoles.Count > 0) {
                    var roles = value.UserRoles.Select(r=>r.Role.Name);
                    writer.WriteStartArray("Roles");
                    {
                        foreach (var role in roles)
                            writer.WriteStringValue(role);
                    }
                    writer.WriteEndObject();
                }

                writer.WriteString("SysUser", value.SysUser);
                writer.WriteString("SysStatus", value.SysStatus.ToString());
                writer.WriteString("SysStart", value.SysStart.ToString("u"));
                writer.WriteString("SysEnd", value.SysStart.ToString("u"));
                //extract catch-all properties and promote to top-level in JSON
                if (value.Properties != null) {
                    using var doc = JsonDocument.Parse(value.Properties);
                    foreach (var prop in doc.RootElement.EnumerateObject())
                        prop.WriteTo(writer);
                }
            }
            writer.WriteEndObject();
        }

    }

}
