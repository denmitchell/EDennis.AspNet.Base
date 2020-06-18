using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDennis.AspNet.Base.Middleware {
    public class JwtParamSets : List<JwtParams> { }
    public class JwtParams {
        public string kid {get; set;}
        public JwtKty kty { get; set; }
        public string k { get; set; }
        public string n { get; set; }
        public string e { get; set; }
        public string d { get; set; }
        public string p { get; set; }
        public string q { get; set; }
        public string dp { get; set; }
        public string dq { get; set; }
        public string qi { get; set; }
    }
}
