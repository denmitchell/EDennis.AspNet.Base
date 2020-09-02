namespace EDennis.NetStandard.Base {
    public class AppRoleChildClaim {

        public AppRoleChildClaim() { }

        public AppRoleChildClaim(string appRole, string claimType, string claimValue) {
            AppRole = appRole;
            ClaimType = claimType;
            ClaimValue = claimValue;
        }

        public string AppRole { get; set; }
        public string ClaimType { get; set; }
        public string ClaimValue { get; set; }
    }
}
