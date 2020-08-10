
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace EDennis.NetStandard.Base {
    public abstract class TeUser<U, O, UC, UL, UT, R, A, RC, UR> : IdentityUser<Guid>, ITemporalEntity
        where U : TeUser<U, O, UC, UL, UT, R, A, RC, UR>
        where O : TeOrganization<U, O, UC, UL, UT, R, A, RC, UR>
        where UC : TeUserClaim<U, O, UC, UL, UT, R, A, RC, UR>
        where UL : TeUserLogin<U, O, UC, UL, UT, R, A, RC, UR>
        where UT : TeUserToken<U, O, UC, UL, UT, R, A, RC, UR>
        where R : TeRole<U, O, UC, UL, UT, R, A, RC, UR>
        where A : TeApplication<U, O, UC, UL, UT, R, A, RC, UR>
        where RC : TeRoleClaim<U, O, UC, UL, UT, R, A, RC, UR>
        where UR : TeUserRole<U, O, UC, UL, UT, R, A, RC, UR> {

        public string SysUser { get; set; }
        public SysStatus SysStatus { get; set; }
        public DateTime SysStart { get; set; }
        public DateTime SysEnd { get; set; }

        public Guid OrganizationId { get; set; }
        public TeApplication<U, O, UC, UL, UT, R, A, RC, UR> Organization { get; set; }

        public ICollection<TeUserClaim<U, O, UC, UL, UT, R, A, RC, UR>> Claims { get; set; }
        public ICollection<TeUserLogin<U, O, UC, UL, UT, R, A, RC, UR>> Logins { get; set; }
        public ICollection<TeUserToken<U, O, UC, UL, UT, R, A, RC, UR>> Tokens { get; set; }
        public ICollection<TeUserRole<U, O, UC, UL, UT, R, A, RC, UR>> UserRoles { get; set; }


        public abstract void Patch(JsonElement jsonElement, ModelStateDictionary modelState);

        public abstract void Update(object updated);


    }
}
