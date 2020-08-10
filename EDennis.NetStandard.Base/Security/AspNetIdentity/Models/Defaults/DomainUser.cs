using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EDennis.NetStandard.Base {

    [JsonConverter(typeof(DomainUserJsonConverter))]
    public class DomainUser : DomainUser<DomainUser, DomainOrganization, DomainUserClaim, DomainUserLogin, DomainUserToken, DomainRole, DomainApplication, DomainRoleClaim, DomainUserRole> {
        public override void Patch(JsonElement jsonElement, ModelStateDictionary modelState) {
            bool normalizedUserNameProvided = false;
            bool normalizedEmailProvided = false;
            bool securityStampNeeded = false;
            foreach (var prop in jsonElement.EnumerateObject()) {
                switch (prop.Name) {
                    case "AccessFailedCount":
                    case "accessFailedCount":
                        AccessFailedCount = prop.Value.GetInt32();
                        break;
                    case "ConcurrencyStamp":
                    case "concurrencyStamp":
                        ConcurrencyStamp = Guid.NewGuid().ToString();
                        break;
                    case "Email":
                    case "email":
                        Email = prop.Value.GetString();
                        break;
                    case "EmailConfirmed":
                    case "emailConfirmed":
                        EmailConfirmed = prop.Value.GetBoolean();
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
                        normalizedEmailProvided = true;
                        break;
                    case "UserName":
                    case "userName":
                        var newUserName = prop.Value.GetString();
                        PasswordHash = newUserName;
                        if (newUserName != UserName)
                            securityStampNeeded = true;
                        UserName = prop.Value.GetString();
                        break;
                    case "NormalizedUserName":
                    case "normalizedUserName":
                        NormalizedUserName = prop.Value.GetString();
                        normalizedUserNameProvided = true;
                        break;
                    case "OrganizationId":
                    case "organizationId":
                        OrganizationId = prop.Value.GetGuid();
                        break;
                    case "PasswordHash":
                    case "passwordHash":
                        var newPasswordHash = prop.Value.GetString();
                        PasswordHash = newPasswordHash;
                        if (newPasswordHash != PasswordHash)
                            securityStampNeeded = true;
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
                    case "TwoFactorEnabled":
                    case "twoFactorEnabled":
                        TwoFactorEnabled = prop.Value.GetBoolean();
                        break;
                    case "Claims":
                    case "claims":
                        Claims = new List<DomainUserClaim>();
                        foreach (var item in prop.Value.EnumerateArray()) {
                            var claim = new DomainUserClaim();
                            foreach (var prop2 in item.EnumerateObject())
                                switch (prop2.Name) {
                                    case "UserId":
                                    case "userId":
                                        claim.UserId = prop2.Value.GetGuid();
                                        break;
                                    case "ClaimType":
                                    case "claimType":
                                        claim.ClaimType = prop2.Value.GetString();
                                        break;
                                    case "ClaimValue":
                                    case "claimvalue":
                                        claim.ClaimValue = prop2.Value.GetString();
                                        break;
                                    case "SysUser":
                                    case "sysUser":
                                        claim.SysUser = prop2.Value.GetString();
                                        break;
                                    case "SysStatus":
                                    case "sysStatus":
                                        claim.SysStatus = (SysStatus)Enum.Parse(typeof(SysStatus), prop.Value.GetString());
                                        break;
                                }
                            Claims.Add(claim);
                        }
                        break;
                    case "Roles":
                    case "roles":
                        UserRoles = new List<DomainUserRole>();
                        foreach (var item in prop.Value.EnumerateArray()) {
                            var userRole = new DomainUserRole();
                            foreach (var prop2 in item.EnumerateObject())
                                switch (prop2.Name) {
                                    case "UserId":
                                    case "userId":
                                        userRole.UserId = prop2.Value.GetGuid();
                                        break;
                                    case "RoleId":
                                    case "roleId":
                                        userRole.RoleId = prop2.Value.GetGuid();
                                        break;
                                    case "SysUser":
                                    case "sysUser":
                                        userRole.SysUser = prop2.Value.GetString();
                                        break;
                                    case "SysStatus":
                                    case "sysStatus":
                                        userRole.SysStatus = (SysStatus)Enum.Parse(typeof(SysStatus), prop.Value.GetString());
                                        break;
                                }
                            UserRoles.Add(userRole);
                        }
                        break;
                }
            }
            if (!normalizedUserNameProvided)
                NormalizedUserName = UserName.ToUpper();
            if (!normalizedEmailProvided)
                NormalizedEmail = Email.ToUpper();
            if (securityStampNeeded)
                SecurityStamp = Guid.NewGuid().ToString();

        }

        public override void Update(object updated) {
            var entity = updated as DomainUser;

            if (SecurityStamp == default || PasswordHash != entity.PasswordHash || UserName != entity.UserName)
                SecurityStamp = Guid.NewGuid().ToString();

            ConcurrencyStamp = Guid.NewGuid().ToString();

            AccessFailedCount = entity.AccessFailedCount;
            Id = entity.Id;
            UserName = entity.UserName;
            NormalizedUserName = entity.NormalizedUserName ?? entity.UserName.ToUpper();
            Email = entity.Email;
            NormalizedEmail = entity.NormalizedEmail ?? entity.Email.ToUpper();
            EmailConfirmed = entity.EmailConfirmed;
            LockoutBegin = entity.LockoutBegin;
            LockoutEnd = entity.LockoutEnd;
            LockoutEnabled = entity.LockoutEnabled;
            OrganizationId = entity.OrganizationId;
            PasswordHash = entity.PasswordHash;
            PhoneNumber = entity.PhoneNumber;
            PhoneNumberConfirmed = entity.PhoneNumberConfirmed;
            SysUser = entity.SysUser;
            SysStatus = entity.SysStatus;
            SysStart = entity.SysStart;
            SysEnd = entity.SysEnd;

            if (entity.UserRoles != null && entity.UserRoles.Count > 0)
                UserRoles = entity.UserRoles;

            if (entity.Claims != null && entity.Claims.Count > 0)
                Claims = entity.Claims;

        }
    }


    public class DomainUserJsonConverter : JsonConverter<DomainUser> {

        public override DomainUser Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            JsonSerializer.Deserialize<DomainUser>(ref reader, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });


        public override void Write(Utf8JsonWriter writer, DomainUser value, JsonSerializerOptions options) {
            writer.WriteStartObject();
            {
                writer.WriteString("Id", value.Id.ToString());
                writer.WriteString("UserName", value.UserName);
                writer.WriteString("NormalizedUserName", value.NormalizedUserName);
                if (value.AccessFailedCount != default)
                    writer.WriteNumber("AccessFailedCount", value.AccessFailedCount);
                writer.WriteString("Email", value.Email);
                writer.WriteString("NormalizedEmail", value.NormalizedEmail);
                writer.WriteBoolean("EmailConfirmed", value.EmailConfirmed);
                writer.WriteBoolean("LockoutEnabled", value.LockoutEnabled);
                if (value.LockoutBegin != default)
                    writer.WriteString("LockoutBegin", value.LockoutBegin.Value.ToString("u"));
                if (value.LockoutEnd != default)
                    writer.WriteString("LockoutEnd", value.LockoutEnd.Value.ToString("u"));
                if (value.OrganizationId != default)
                    writer.WriteString("OrganizationId", value.OrganizationId.ToString());
                if (value.Organization != null)
                    writer.WriteString("OrganizationName", value.Organization.Name);
                if (value.PhoneNumber != default)
                    writer.WriteString("PhoneNumber", value.PhoneNumber);
                if (value.PhoneNumberConfirmed != default)
                    writer.WriteBoolean("PhoneNumberConfirmed", value.PhoneNumberConfirmed);
                if (value.TwoFactorEnabled != default)
                    writer.WriteBoolean("TwoFactorEnabled", value.TwoFactorEnabled);
                writer.WriteString("SysUser", value.SysUser);
                writer.WriteString("SysStatus", value.SysStatus.ToString());
                writer.WriteString("SysStart", value.SysStart.ToString("u"));
                writer.WriteString("SysEnd", value.SysStart.ToString("u"));

                if (value.Claims != null) {
                    writer.WriteStartArray("Claims");
                    {
                        foreach (var claim in value.Claims) {
                            writer.WriteStartObject();
                            {
                                writer.WriteString("ClaimType", claim.ClaimType);
                                writer.WriteString("ClaimValue", claim.ClaimValue);
                                writer.WriteString("SysUser", value.SysUser);
                                writer.WriteString("SysStatus", value.SysStatus.ToString());
                                writer.WriteString("SysStart", value.SysStart.ToString("u"));
                                writer.WriteString("SysEnd", value.SysStart.ToString("u"));
                            }
                            writer.WriteEndObject();
                        }
                    }
                    writer.WriteEndArray();
                }

                if (value.UserRoles != null) {
                    writer.WriteStartArray("Roles");
                    {
                        foreach (var role in value.UserRoles) {
                            writer.WriteStartObject();
                            {
                                writer.WriteString("RoleId", role.RoleId);
                                if (role.Role != null) {
                                    if (role.Role.Application != null)
                                        writer.WriteString("ApplicationName", role.Role.Application.Name);
                                    writer.WriteString("RoleName", role.Role.Name);
                                }
                                writer.WriteString("SysUser", value.SysUser);
                                writer.WriteString("SysStatus", value.SysStatus.ToString());
                                writer.WriteString("SysStart", value.SysStart.ToString("u"));
                                writer.WriteString("SysEnd", value.SysStart.ToString("u"));
                            }
                            writer.WriteEndObject();
                        }
                    }
                    writer.WriteEndArray();
                }

            }
            writer.WriteEndObject();
        }

    }

}
