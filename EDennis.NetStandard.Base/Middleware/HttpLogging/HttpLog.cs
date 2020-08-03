using System;
using System.Collections.Generic;

namespace EDennis.NetStandard.Base {
    public class HttpLog {
        public string Path { get; set; }
        public string QueryString { get; set; }
        public string DisplayUrl { get; set; }
        public string Method { get; set; }
        public Dictionary<string, List<string>> Claims { get; set; }
        public Dictionary<string, List<string>> Headers { get; set; }
        public string Payload { get; set; }
        public string Response { get; set; }
        public string ResponseCode { get; set; }
        public DateTime RequestedOn { get; set; }
        public DateTime RespondedOn { get; set; }
        public bool IsSuccessStatusCode { get; set; }
    }
}
