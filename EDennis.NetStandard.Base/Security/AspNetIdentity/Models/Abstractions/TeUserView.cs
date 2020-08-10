
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace EDennis.NetStandard.Base {
    public class TeUserView<U, O, UC, UL, UT, R, A, RC, UR> : IdentityUser<Guid>
        where U : TeUser<U, O, UC, UL, UT, R, A, RC, UR>
        where O : TeOrganization<U, O, UC, UL, UT, R, A, RC, UR>
        where UC : TeUserClaim<U, O, UC, UL, UT, R, A, RC, UR>
        where UL : TeUserLogin<U, O, UC, UL, UT, R, A, RC, UR>
        where UT : TeUserToken<U, O, UC, UL, UT, R, A, RC, UR>
        where R : TeRole<U, O, UC, UL, UT, R, A, RC, UR>
        where A : TeApplication<U, O, UC, UL, UT, R, A, RC, UR>
        where RC : TeRoleClaim<U, O, UC, UL, UT, R, A, RC, UR>
        where UR : TeUserRole<U, O, UC, UL, UT, R, A, RC, UR> {

        public string OrganizationName { get; set; }

        public string RolesDictionary { get; set; }
        public string ClaimsDictionary { get; set; }

    }
}
