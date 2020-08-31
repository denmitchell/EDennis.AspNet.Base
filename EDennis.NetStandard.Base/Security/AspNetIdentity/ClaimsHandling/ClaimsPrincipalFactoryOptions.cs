namespace EDennis.NetStandard.Base {
    public class ClaimsPrincipalFactoryOptions {

        /// <summary>
        /// Whether to filter claims by the embedded application
        /// name.  Note: this works only if the ClaimsPrincipalFactory
        /// is used by the target application (or an embedded Identity Server).
        /// If it is used by a centralized IdentityServer, then set
        /// this value to false (default)
        /// </summary>
        public bool FilterClaimsByCurrentApplicationName { get; set; }

    }
}
