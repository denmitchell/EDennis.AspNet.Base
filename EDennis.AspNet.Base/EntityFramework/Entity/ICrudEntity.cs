using System.Text.Json;

namespace EDennis.AspNet.Base {
    public interface ICrudEntity {
        SysStatus SysStatus { get; set; }
        string SysUser { get; set; }

        void Patch(JsonElement jsonElement);
        void Update(object updated);
    }
}