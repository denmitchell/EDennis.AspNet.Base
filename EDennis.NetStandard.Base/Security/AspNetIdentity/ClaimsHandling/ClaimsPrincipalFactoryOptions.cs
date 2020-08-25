namespace EDennis.NetStandard.Base {
    public class ClaimsPrincipalFactoryOptions {

        /// <summary>
        /// Whether to include roles as claims
        /// </summary>
        public bool IncludeRoles { get; set; }

        /// <summary>
        /// Whether to include role claims as claims
        /// Note: for performance reasons, it is better to cache
        /// the role claims in the application, rather than in
        /// the database as AspNetRoleClaims records
        /// </summary>
        public bool IncludeRoleClaims { get; set; }

        /// <summary>
        /// Whether to filter roles/role claims by the embedded application
        /// name.  Note: this works only if the ClaimsPrincipalFactory
        /// is used by the target application (or an embedded Identity Server).
        /// If it is used by a centralized IdentityServer, then set
        /// this value to false (default)
        /// </summary>
        public bool FilterRolesByCurrentApplicationName { get; set; }

        /// <summary>
        /// Whether to include user claims as claims
        /// </summary>
        public bool IncludeUserClaims { get; set; }

        /// <summary>
        /// Whether to filter user claims by the embedded application
        /// name.  Note: this works only if the ClaimsPrincipalFactory
        /// is used by the target application (or an embedded Identity Server).
        /// If it is used by a centralized IdentityServer, then set
        /// this value to false (default)
        /// </summary>
        public bool FilterUserClaimsByCurrentApplicationName { get; set; }

    }
}
