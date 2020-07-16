using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json;

namespace EDennis.AspNet.Base {
    public interface ICrudEntity {
        SysStatus SysStatus { get; set; }
        string SysUser { get; set; }

        void Patch(JsonElement jsonElement, ModelStateDictionary modelState, bool mergeCollections = true);
        void Update(object updated);
    }
}