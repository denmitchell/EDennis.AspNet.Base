using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace EDennis.AspNet.Base.Security {
    public class DomainUser : IdentityUser<Guid>, ITemporalEntity {

        public Guid OrganizationId { get; set; }

        public DateTime? LockoutBegin { get; set; }

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

        public Dictionary<string, string> Properties { get; set; }

        public DomainOrganization Organization { get; set; }
        public ICollection<DomainUserClaim> UserClaims { get; set; }
        public ICollection<DomainUserRole> UserRoles { get; set; }
        public ICollection<DomainUserLogin> UserLogins { get; set; }
        public ICollection<DomainUserLogin> UserTokens { get; set; }
        public DateTime SysEnd { get; set; }
        public DateTime SysStart { get; set; }
        public SysStatus SysStatus { get; set; }
        public string SysUser { get; set; }

        public void Patch(JsonElement jsonElement, ModelStateDictionary modelState, bool mergeCollections = true) {
            foreach (var prop in jsonElement.EnumerateObject()) {
                try {
                    switch (prop.Name) {
                        case "AccessFailedCount":
                        case "accessFailedCount":
                            AccessFailedCount = prop.Value.GetInt32();
                            break;
                        /*
                AccessFailedCount = obj.AccessFailedCount;
                ConcurrencyStamp = Guid.NewGuid().ToString();
                Email = obj.Email;
                EmailConfirmed = obj.EmailConfirmed;
                Id = obj.Id;
                LockoutBegin = obj.LockoutBegin;
                LockoutEnabled = obj.LockoutEnabled;
                LockoutEnd = obj.LockoutEnd;
                NormalizedEmail = obj.Email.ToUpper();
                NormalizedUserName = obj.UserName.ToUpper();
                Organization = obj.Organization;
                OrganizationId = obj.OrganizationId;
                PasswordHash = obj.PasswordHash;
                PhoneNumber = obj.PhoneNumber;
                PhoneNumberConfirmed = obj.PhoneNumberConfirmed;
                Properties = obj.Properties;
                SecurityStamp = obj.SecurityStamp;
                SysStart = obj.SysStart;
                SysEnd = obj.SysEnd;
                SysStatus = obj.SysStatus;
                SysUser = obj.SysUser;
                TwoFactorEnabled = obj.TwoFactorEnabled;
                UserClaims = obj.UserClaims;
                UserLogins = obj.UserLogins;
                UserName = obj.UserName;
                UserRoles = obj.UserRoles;
                UserTokens = obj.UserTokens;
                        */
                        case "LoginProvider":
                        case "loginProvider":
                            LoginProvider = prop.Value.GetString();
                            break;
                        case "UserId":
                        case "userId":
                            UserId = prop.Value.GetGuid();
                            break;
                        case "Name":
                        case "name":
                            LoginProvider = prop.Value.GetString();
                            break;
                        case "Properties":
                        case "properties":
                            var properties = new Dictionary<string, string>();
                            prop.Value.EnumerateObject().ToList().ForEach(e => {
                                properties.Add(e.Name, e.Value.GetString());
                            });
                            if (mergeCollections && Properties != null)
                                foreach (var entry in properties)
                                    if (Properties.ContainsKey(entry.Key))
                                        Properties[entry.Key] = entry.Value;
                                    else
                                        Properties.Add(entry.Key, entry.Value);
                            else
                                Properties = properties;
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
                        default:
                            break;
                    }
                } catch (InvalidOperationException ex) {
                    modelState.AddModelError(prop.Name, $"{ex.Message}: Cannot parse value for {prop.Value} from {GetType().Name} JSON");
                }
            }
        }

        public void Update(object updated) {
            var obj = updated as DomainUser;
            AccessFailedCount = obj.AccessFailedCount;
            ConcurrencyStamp = Guid.NewGuid().ToString();
            Email = obj.Email;
            EmailConfirmed = obj.EmailConfirmed;
            Id = obj.Id;
            LockoutBegin = obj.LockoutBegin;
            LockoutEnabled = obj.LockoutEnabled;
            LockoutEnd = obj.LockoutEnd;
            NormalizedEmail = obj.Email.ToUpper();
            NormalizedUserName = obj.UserName.ToUpper();
            Organization = obj.Organization;
            OrganizationId = obj.OrganizationId;
            PasswordHash = obj.PasswordHash;
            PhoneNumber = obj.PhoneNumber;
            PhoneNumberConfirmed = obj.PhoneNumberConfirmed;
            Properties = obj.Properties;
            SecurityStamp = obj.SecurityStamp;
            SysStart = obj.SysStart;
            SysEnd = obj.SysEnd;
            SysStatus = obj.SysStatus;
            SysUser = obj.SysUser;
            TwoFactorEnabled = obj.TwoFactorEnabled;
            UserClaims = obj.UserClaims;
            UserLogins = obj.UserLogins;
            UserName = obj.UserName;
            UserRoles = obj.UserRoles;
            UserTokens = obj.UserTokens;
        }
    }
}
