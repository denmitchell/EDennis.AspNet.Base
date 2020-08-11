using System;
using System.Security.Claims;

namespace EDennis.NetStandard.Base {
    public interface IAppClaimComposer {

        /// <summary>
        /// Combines application name, claim type, and claim value into
        /// a regular claim by incorporating application name into either
        /// claim type or claim value.
        /// </summary>
        /// <param name="appClaim">container for holding app name, claim type, and claim value</param>
        /// <returns>regular claim with claim type and claim value (and application name embedded in type or value)</returns>
        Claim Compose(AppClaim appClaim);


        /// <summary>
        /// Decomposes a regular claim into an AppClaim by
        /// extracting application name from either the claim type or claim value
        /// </summary>
        /// <param name="claim">regular claim</param>
        /// <returns>AppClaim instance holding claim type, claim value, and extracted application name</returns>
        AppClaim Decompose(Claim claim);
    }
}