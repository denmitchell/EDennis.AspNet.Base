using System;
using System.Security.Claims;

namespace EDennis.NetStandard.Base {


    /// <summary>
    /// Provides some flexibility for combining application name and role name into a 
    /// fully qualified role name, which is stored in Role.NormalizedName.  
    /// </summary>
    public interface IAppClaimEncoder {

        /// <summary>
        /// Combines application name, claim type, and claim value into
        /// a regular claim by incorporating application name into either
        /// claim type or claim value.
        /// 
        /// </summary>
        /// <param name="appClaim">container for holding app name, claim type, and claim value</param>
        /// <returns>regular claim with claim type and claim value (and application name embedded in type or value)</returns>
        Claim Encode(AppClaim appClaim);


        /// <summary>
        /// Decomposes a regular claim into an AppClaim by
        /// extracting application name from either the claim type or claim value
        /// </summary>
        /// <param name="claim">regular claim</param>
        /// <returns>AppClaim instance holding claim type, claim value, and extracted application name</returns>
        AppClaim Decode(Claim claim);


        /// <summary>
        /// Combines application name and role name into a single string.
        /// NOTE: This is used to set Role.NormalizedName when creating or updating a role
        /// </summary>
        /// <param name="appRole">container for holding app name and role name</param>
        /// <returns>string that combines application name and role name</returns>
        string Encode(AppRole appRole);


        /// <summary>
        /// Decomposes a single role string into an AppRole by
        /// extracting application name.  
        /// Note: this is used to update Role.Name and Role.Application when 
        /// Role.NormalizedName is set and it isn't consistent with Name and Application
        /// </summary>
        /// <param name="roleString">string that combines application name and role name</param>
        /// <returns>AppRole instance holding role name and extracted application name</returns>
        AppRole Decode(string roleString);
 
    }
}