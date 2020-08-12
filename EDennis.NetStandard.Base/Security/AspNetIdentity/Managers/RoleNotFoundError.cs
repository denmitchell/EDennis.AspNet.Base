using Microsoft.AspNetCore.Identity;

namespace EDennis.NetStandard.Base {
    public static class IdentityErrorDescriber_Extensions {
        public static IdentityError RoleNotFoundError(this IdentityErrorDescriber describer, string appName, string roleName)
            => new IdentityError { Code = "RoleNotFound", Description = $"Role not found for application {appName} and name {roleName}" };
    }
}
