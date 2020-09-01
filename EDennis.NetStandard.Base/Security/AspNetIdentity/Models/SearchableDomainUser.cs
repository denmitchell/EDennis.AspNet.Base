namespace EDennis.NetStandard.Base {
    public class SearchableDomainUser {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Organization { get; set; }
        public bool OrganizationAdmin { get; set; }
        public string Applications { get; set; } 
        //public long ApplicationCount { get; set; }
    }
}
