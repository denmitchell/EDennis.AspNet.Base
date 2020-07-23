using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EDennis.AspNet.Base.Security {

    /// <summary>
    /// This entity is mapped to a view with the same name.
    /// Note that in the default, SQL Server implementation,
    /// this view joins data from two materialized views --
    /// ClientApplications and UserApplicationRoles.
    /// </summary>
    public class UserClientApplicationRole {
        public Guid UserId { get; set; }
        public string ClientId { get; set; }
        public string ApplicationName { get; set; }
        public string RoleName { get; set; }
    }
}
