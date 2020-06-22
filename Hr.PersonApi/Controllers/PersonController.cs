using EDennis.AspNet.Base;
using Hr.PersonApi.Models;

namespace Hr.PersonApi.Controllers {
    public class PersonController : CrudController<HrContext, Person> {
        public PersonController(HrContext context) : base(context) {
        }
    }
}
