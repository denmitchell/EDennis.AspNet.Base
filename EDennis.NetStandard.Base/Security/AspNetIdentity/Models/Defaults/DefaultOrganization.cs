using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json;

namespace EDennis.NetStandard.Base {
    public class DefaultOrganization : TeOrganization<DefaultUser, DefaultOrganization, DefaultUserClaim, DefaultUserLogin, DefaultUserToken, DefaultRole, DefaultApplication, DefaultRoleClaim, DefaultUserRole> {
        public override void Patch(JsonElement jsonElement, ModelStateDictionary modelState)
            => this.DefaultPatch(jsonElement, modelState);

        public override void Update(object updated)
            => this.DefaultUpdate(updated);
    }
}