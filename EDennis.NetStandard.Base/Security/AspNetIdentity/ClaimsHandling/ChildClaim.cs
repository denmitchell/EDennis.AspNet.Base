namespace EDennis.NetStandard.Base {
    public class ChildClaim {

        public ChildClaim() { }

        public ChildClaim(string parentType, string parentValue, string claimType, string claimValue) {
            ParentType = parentType;
            ParentValue = parentValue;
            ClaimType = claimType;
            ClaimValue = claimValue;
        }
        public string ParentType { get; set; }
        public string ParentValue { get; set; }
        public string ClaimType { get; set; }
        public string ClaimValue { get; set; }
    }
}
