using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;

namespace EDennis.NetStandard.Base {

    public static class IdentityErrorDescriber_Extensions {

        public static IdentityError InvalidNormalizedName(this IdentityErrorDescriber _, string normalizedName) {
            return new IdentityError {
                Code = nameof(InvalidNormalizedName),
                Description = $"Could not extract valid application name from '{normalizedName}'"
            };
        }

        public static IdentityError InvalidApplicationName(this IdentityErrorDescriber _, string applicationName) {
            return new IdentityError {
                Code = nameof(InvalidApplicationName),
                Description = $"Application name '{applicationName}' does not exist in store/database."
            };

        }

        public static IdentityError InvalidOrganizationName(this IdentityErrorDescriber _, string organizationName) {
            return new IdentityError {
                Code = nameof(InvalidOrganizationName),
                Description = $"Organization name '{organizationName}' does not exist in store/database."
            };

        }


        public static IdentityError RoleNotFoundError(this IdentityErrorDescriber _, string appName, string roleName)
            => new IdentityError {
                Code = nameof(RoleNotFoundError),
                Description = $"Role not found for application {appName} and name {roleName}"
            };


        public static IdentityError DbUpdateException(this IdentityErrorDescriber _, DbUpdateException ex) {
            return new IdentityError {
                Code = nameof(DbUpdateException),
                Description = ex.Message + ": " + ex.InnerException?.Message ?? ""
            };
        }

    }

}
