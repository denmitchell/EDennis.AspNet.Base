using IdentityModel;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System.Linq;

namespace EDennis.NetStandard.Base {

    /// <summary>
    /// Defines an Authorization message handler that is pre-configured 
    /// to request the following scopes/claim types (which must be
    /// registered in ApiResourceScopes or ApiResourceClaims:
    /// <list type="bullet">
    ///     <item>sub</item>
    ///     <item>name</item>
    ///     <item>email</item>
    ///     <item>phone_number</item>
    ///     <item>organization</item>
    ///     <item>organization_admin_for</item>
    ///     <item>super_admin</item>
    ///     <item>locked</item>
    ///     <item>role:{AppName}, where {AppName} is the name of the target application</item>
    /// </list>
    /// </summary>
    public class DomainIdentityMessageHandler : ConfigurableScopesMessageHandler {
        public virtual string TargetApplicationName { get; }
        public override string[] Scopes {
            get {
                return new string[] {
                    JwtClaimTypes.Subject,
                    JwtClaimTypes.Name,
                    JwtClaimTypes.Email,
                    JwtClaimTypes.PhoneNumber,
                    DomainClaimTypes.Organization,
                    DomainClaimTypes.OrganizationAdminFor,
                    DomainClaimTypes.SuperAdmin,
                    DomainClaimTypes.Locked
                };
            }
        }

        public DomainIdentityMessageHandler(IAccessTokenProvider provider, NavigationManager navigationManager)
            : base(provider, navigationManager) {
            ConfigureHandler(
                       authorizedUrls: new[] { navigationManager.BaseUri },
                       scopes: Scopes.Union(new string[] {
                            DomainClaimTypes.ApplicationRole(TargetApplicationName)
                            }).ToArray()
          );              
                       
        }
    }
}
