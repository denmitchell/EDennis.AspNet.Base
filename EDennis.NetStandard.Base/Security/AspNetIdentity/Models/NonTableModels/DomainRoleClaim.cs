namespace EDennis.NetStandard.Base {

    /// <summary>
    /// Role claims are omitted purposefully from the DbContext model.
    /// Resolution of roles to associated child claims is deemed to be the
    /// responsibility of each individual application.  This delegated
    /// responsibility provides some benefits:
    /// <list type="bullet">
    ///     <item>Specification of role-associated claims is configured in the application by developers
    ///     and hence provides for a simpler development experience, especially when roles are mapped
    ///     to many claims</item>
    ///     <item>Even when ASP.NET Identity tables are centralized and apply to multiple applications,
    ///     each application can keep track of its own role-associated claims without having to
    ///     qualify these claims with an application prefix</item>
    ///     <item>If role-associated claims are cached as a singleton (and possibly resolved via an
    ///     IClaimsTransformation class), then these claims would not need to be included in the
    ///     access token, making the access token quicker to build and leaner</item>
    /// </list>
    /// </summary>
    public class DomainRoleClaim {
        public string RoleName { get; set; }
        public string ClaimType { get; set; }
        public string ClaimValue { get; set; }

    }

}