namespace EDennis.NetStandard.Base {
    public class ClaimsPrincipalFactoryOptions {

        /// <summary>
        /// Whether to filter the claims by the embedded application
        /// name.  Note: this works only if the ClaimsPrincipalFactory
        /// is used by the target application (or an embedded Identity Server).
        /// If it is used by a centralized IdentityServer, then set
        /// this value to false (default)
        /// </summary>
        public bool FilterByCurrentApplicationName { get; set; }

        /// <summary>
        /// Note: for performance reasons, it is better to cache
        /// the role claims in the application, rather than in
        /// the database as AspNetRoleClaims records
        /// </summary>
        public bool IncludeRoleClaims { get; set; }
    }
}
