
using System.Security.Claims;

namespace EDennis.NetStandard.Base {

    /// <summary>
    /// Singleton service that handles composition and decomposition of
    /// AppClaim values and Role.NormalizedName, where the ClaimValue
    /// is prefixed by the ApplicationName and a colon.
    /// 
    /// NOTE: Other implementations of IAppClaimEncoder are possible.
    /// </summary>
    public class DefaultAppClaimEncoder : IAppClaimEncoder {

        /// <summary>
        /// Combines application name, claim type, and claim value into
        /// a regular claim by incorporating application name into either
        /// claim type or claim value.
        /// </summary>
        /// <param name="appClaim">container for holding app name, claim type, and claim value</param>
        /// <returns>regular claim with claim type and claim value (and application name embedded in type or value)</returns>
        public Claim Encode(AppClaim appClaim)
            => new Claim($"{appClaim.ClaimType}",$"{appClaim.Application}:{appClaim.ClaimValue}");

        /// <summary>
        /// Decomposes a regular claim into an AppClaim by
        /// extracting application name from either the claim type or claim value
        /// </summary>
        /// <param name="claim">regular claim</param>
        /// <returns>AppClaim instance holding claim type, claim value, and extracted application name</returns>
        public AppClaim Decode(Claim claim) {
            var separatorIndex = claim.Value.IndexOf(":");
            if (separatorIndex == -1)
                return new AppClaim {
                    Application = null,
                    ClaimType = claim.Type,
                    ClaimValue = claim.Value
                };
            else
                return new AppClaim {
                    Application = claim.Value.Substring(0, separatorIndex),
                    ClaimType = claim.Value,
                    ClaimValue = claim.Value.Substring(separatorIndex + 1) 
                };
        }


        /// <summary>
        /// Combines application name and role name into a single string
        /// </summary>
        /// <param name="appRole">container for holding app name and role name</param>
        /// <returns>string that combines application name and role name</returns>
        public string Encode(AppRole appRole)
            => $"{appRole.Application}:{appRole.RoleName}";


        /// <summary>
        /// Decomposes a single role string into an AppRole by
        /// extracting application name
        /// </summary>
        /// <param name="roleString">string that combines application name and role name</param>
        /// <returns>AppRole instance holding role name and extracted application name</returns>
        public AppRole Decode(string roleString) {
            var separatorIndex = roleString.IndexOf(":");
            if (separatorIndex == -1)
                return new AppRole {
                    Application = null,
                    RoleName = roleString
                };
            else
                return new AppRole {
                    Application = roleString.Substring(0, separatorIndex),
                    RoleName = roleString.Substring(separatorIndex + 1)
                };
        }

    }
}
