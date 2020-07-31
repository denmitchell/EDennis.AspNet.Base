using System.Collections.Generic;

namespace EDennis.NetStandard.Base {
    public class MockUserOptions {
        public bool Enabled { get; set; }
        public Dictionary<string,string[]> Claims {get; set;}
    }
}
