using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EDennis.AspNet.Base.Security {

    /// <summary>
    /// This entity is updated by a stored procedure named
    /// di.UpdateUserClientClaims
    /// </summary>
    public class UserClientClaims {
        public Guid UserId { get; set; }

        [MaxLength(200)]
        public string ClientId { get; set; }

        [Column(TypeName ="nvarchar(max)")]
        public string Claims { get; set; }
        
    }
}
