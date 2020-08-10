using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace EDennis.NetStandard.Base {
    public abstract class TeApplication<U, O, UC, UL, UT, R, A, RC, UR> : ITemporalEntity
        where U : TeUser<U, O, UC, UL, UT, R, A, RC, UR>
        where O : TeOrganization<U, O, UC, UL, UT, R, A, RC, UR>
        where UC : TeUserClaim<U, O, UC, UL, UT, R, A, RC, UR>
        where UL : TeUserLogin<U, O, UC, UL, UT, R, A, RC, UR>
        where UT : TeUserToken<U, O, UC, UL, UT, R, A, RC, UR>
        where R : TeRole<U, O, UC, UL, UT, R, A, RC, UR>
        where A : TeApplication<U, O, UC, UL, UT, R, A, RC, UR>
        where RC : TeRoleClaim<U, O, UC, UL, UT, R, A, RC, UR>
        where UR : TeUserRole<U, O, UC, UL, UT, R, A, RC, UR> {

        public Guid Id { get; set; }
        public string Name { get; set; }

        public string SysUser { get; set; }
        public SysStatus SysStatus { get; set; }
        public DateTime SysStart { get; set; }
        public DateTime SysEnd { get; set; }

        public ICollection<TeRole<U, O, UC, UL, UT, R, A, RC, UR>> Roles { get; set; }

        public abstract void Patch(JsonElement jsonElement, ModelStateDictionary modelState);

        public abstract void Update(object updated);

    }
}