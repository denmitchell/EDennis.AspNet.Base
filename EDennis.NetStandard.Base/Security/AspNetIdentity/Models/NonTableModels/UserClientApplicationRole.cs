using System;

namespace EDennis.NetStandard.Base {

    /// <summary>
    /// This entity is mapped to a view with the same name.
    /// Note that in the default, SQL Server implementation,
    /// this view joins data from two materialized views --
    /// ClientApplications and UserApplicationRoles.
    /// </summary>
    public class UserClientApplicationRole {
        public int UserId { get; set; }
        public string ClientId { get; set; }
        public string ApplicationName { get; set; }
        public string RoleName { get; set; }
    }
}
