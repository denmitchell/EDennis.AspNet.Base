using Microsoft.AspNetCore.Mvc;

namespace EDennis.NetStandard.Base {
    public class ObjectResult<TResult> : ObjectResult {
        public TResult TypedValue { get; }
        public ObjectResult(object value) : base(value) {
            try {
                TypedValue = (TResult)value;
            } catch {
            }
        }
    }
}
