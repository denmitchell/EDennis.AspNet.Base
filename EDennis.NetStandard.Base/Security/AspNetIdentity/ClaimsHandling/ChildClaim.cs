namespace EDennis.NetStandard.Base {
    public class ChildClaim {

        public ChildClaim() { }

        public ChildClaim(string parentType, string parentValue, string childType, string childValue) {
            ParentType = parentType;
            ParentValue = parentValue;
            ChildType = childType;
            ChildValue = childValue;
        }
        public string ParentType { get; set; }
        public string ParentValue { get; set; }
        public string ChildType { get; set; }
        public string ChildValue { get; set; }
    }
}
