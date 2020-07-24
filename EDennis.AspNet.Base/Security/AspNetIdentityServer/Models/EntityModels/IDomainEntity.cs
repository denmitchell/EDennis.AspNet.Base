using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Text.Json;

namespace EDennis.AspNet.Base.Security {
    public interface IDomainEntity {
        DateTime SysEnd { get; set; }
        DateTime SysStart { get; set; }
        SysStatus SysStatus { get; set; }
        string SysUser { get; set; }

        void DeserializeInto(JsonElement source, ModelStateDictionary modelState);
    }
}