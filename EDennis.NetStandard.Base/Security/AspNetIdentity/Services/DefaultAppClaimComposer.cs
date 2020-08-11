
using System.Security.Claims;

namespace EDennis.NetStandard.Base {

    /// <summary>
    /// Singleton service that handles composition and decomposition of
    /// AppClaim values, where the application name is embedded into
    /// either the ClaimType or ClaimValue
    /// </summary>
    public class DefaultAppClaimComposer : IAppClaimComposer {

        /// <summary>
        /// Combines application name, claim type, and claim value into
        /// a regular claim by incorporating application name into either
        /// claim type or claim value.
        /// </summary>
        /// <param name="appClaim">container for holding app name, claim type, and claim value</param>
        /// <returns>regular claim with claim type and claim value (and application name embedded in type or value)</returns>
        public Claim Compose(AppClaim appClaim)
            => new Claim($"{appClaim.ApplicationName}:{appClaim.ClaimType}",appClaim.ClaimValue);

        /// <summary>
        /// Decomposes a regular claim into an AppClaim by
        /// extracting application name from either the claim type or claim value
        /// </summary>
        /// <param name="claim">regular claim</param>
        /// <returns>AppClaim instance holding claim type, claim value, and extracted application name</returns>
        public AppClaim Decompose(Claim claim) {
            var separatorIndex = claim.Type.IndexOf(":");
            if (separatorIndex == -1)
                return new AppClaim {
                    ApplicationName = null,
                    ClaimType = claim.Type,
                    ClaimValue = claim.Value
                };
            else
                return new AppClaim {
                    ApplicationName = claim.Type.Substring(0, separatorIndex),
                    ClaimType = claim.Type.Substring(separatorIndex + 1),
                    ClaimValue = claim.Value 
                };
        }

    }
}
