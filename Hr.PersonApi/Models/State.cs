using EDennis.AspNet.Base;

namespace Hr.PersonApi.Models {
    public class State : TemporalEntity {
        public string Code { get; set; }
        public string Name { get; set; }
    }
}
