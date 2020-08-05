using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json;

namespace EDennis.NetStandard.Base {
    public interface ICrudEntity {
        string SysUser { get; set; }

        void Patch(JsonElement jsonElement, ModelStateDictionary modelState);
        void Update(object updated);
    }
}