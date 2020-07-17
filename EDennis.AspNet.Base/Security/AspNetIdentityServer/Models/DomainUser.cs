using IdentityServer4.Models;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json;

namespace EDennis.AspNet.Base.Security {
    public class DomainUser : IdentityUser<Guid>, ITemporalEntity {

        public const int SHA256_LENGTH = 64; //"a8a2f6ebe286697c527eb35a58b5539532e9b3ae3b64d4eb0a46fb657b41562c";
        public const int SHA512_LENGTH = 128; //"f3bf9aa70169e4ab5339f20758986538fe6c96d7be3d184a036cde8161105fcf53516428fa096ac56247bb88085b0587d5ec8e56a6807b1af351305b2103d74b";
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



        public Dictionary<string, string> Properties { get; set; }

        public DomainOrganization Organization { get; set; }
        public ICollection<DomainUserClaim> UserClaims { get; set; }
        public ICollection<DomainUserRole> UserRoles { get; set; }
        public ICollection<DomainUserLogin> UserLogins { get; set; }
        public ICollection<DomainUserToken> UserTokens { get; set; }
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
                        case "UserClaims":
                        case "userClaims":
                            var claims = prop.Value.EnumerateArray()
                                .Select(e => DomainUserClaim.DeserializeInto(e, new DomainUserClaim(), modelState));
                            if (mergeCollections && UserClaims != null) {
                                var newClaims = claims.Where(n => !UserClaims.Any(e => e.ClaimType == n.ClaimType && e.ClaimValue == n.ClaimValue));
                                UserClaims = UserClaims.Union(newClaims).ToArray();
                            } else
                                UserClaims = claims.ToArray();
                            break;
                        case "UserRoles":
                        case "userRoles":
                            var roles = prop.Value.EnumerateArray()
                                .Select(e => DomainUserRole.DeserializeInto(e, new DomainUserRole(), modelState));
                            if (mergeCollections && UserRoles != null) {
                                var newRoles = roles.Where(n => !UserRoles.Any(e => e.RoleId == n.RoleId));
                                UserRoles = UserRoles.Union(newRoles).ToArray();
                            } else
                                UserRoles = roles.ToArray();
                            break;
                        case "UserLogins":
                        case "userLogins":
                            var logins = prop.Value.EnumerateArray()
                                .Select(e => DomainUserLogin.DeserializeInto(e, new DomainUserLogin(), modelState));
                            if (mergeCollections && UserLogins != null) {
                                var newLogins = logins.Where(n => !UserLogins.Any(e => e.LoginProvider == n.LoginProvider && e.ProviderKey == n.ProviderKey));
                                UserLogins = UserLogins.Union(newLogins).ToArray();
                            } else
                                UserLogins = logins.ToArray();
                            break;
                        case "UserTokens":
                        case "userTokens":
                            var tokens = prop.Value.EnumerateArray()
                                .Select(e => DomainUserToken.DeserializeInto(e, new DomainUserToken(), modelState));                            
                            if (mergeCollections && UserTokens != null) {
                                var newTokens = tokens.Where(n => !UserTokens.Any(e => e.LoginProvider == n.LoginProvider && e.Name == n.Name));
                                UserTokens = UserTokens.Union(newTokens).ToArray();
                            } else
                                UserTokens = tokens.ToArray();
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
