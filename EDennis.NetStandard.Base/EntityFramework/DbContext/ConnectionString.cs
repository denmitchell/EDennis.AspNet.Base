using System;
using System.Collections.Generic;
using System.Text;

namespace EDennis.NetStandard.Base {
    public class ConnectionString {
        public string Server { get; set; }
        public string Database { get; set; }
        public string Trusted_Connection { get; set; }
        public string MultipleActiveResultSets { get; set; }

        public string SqlServer {
            get {
                return $"Server={Server};Database={Database};Trusted_Connection={Trusted_Connection};MultipleActiveResultSets={MultipleActiveResultSets};";
            }
        }
    }
}
