using CsvHelper;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace EDennis.NetStandard.Base {

    /// <summary>
    /// Singleton cache used by IClaimsTransformation.
    /// This cache is an application/project-specific cache that is used to
    /// hold claims that are specific to the application.  This cache can be
    /// helpful in reducing size of identity or access tokens and/or for 
    /// implementing role-like functionality without using ASP.NET Identity 
    /// Roles. If desired, many highly granular child claims can be 
    /// maintained in the cache without much impact on performance.
    /// </summary>
    public class AppRoleChildClaimCache : IChildClaimCache {

        /// <summary>
        /// The part of the parent claim type that precedes the colon.  This
        /// is used by the ChildClaimsTransformer to find matching child claims.
        /// 
        /// NOTE: if this property is left as null, IHostEnvironment.ApplicationName 
        /// is used as the default. 
        /// </summary>
        public virtual string ApplicationName { get; set; }


        /// <summary>
        /// The path to a configuration file that holds the app role
        /// child claims.
        /// 
        /// NOTE: This is used by the default implementation of the 
        /// GetAppRoleChildClaims() method.
        /// </summary>
        public virtual string ConfigFilePath { get; set; } = "appRoles.csv";


        /// <summary>
        /// 
        /// By default, retrieve all child claims from the CSV config file,
        /// (without header row) consisting of three fields per row:
        /// <list type="number">
        ///   <item>app role -- the part of the claim value after the colon</item>
        ///   <item>claim type -- the type of the child claim</item>
        ///   <item>claim value -- the value of the child claim</item>
        /// </list>
        /// 
        /// NOTE: the CSV format was chosen because of its compactness and
        /// because it would be relatively easy to enter the child claims
        /// in a spreadsheet application and export to csv.  This method
        /// can be overridden to specify a different source or to
        /// hard-code the child claims.
        /// 
        /// </summary>
        public virtual IEnumerable<AppRoleChildClaim> GetAppRoleChildClaims() {             
            using var reader = new StreamReader(ConfigFilePath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            var records = new List<AppRoleChildClaim>();
            csv.Configuration.HasHeaderRecord = false;
            while (csv.Read()) {
                var record = new AppRoleChildClaim {
                    AppRole = csv.GetField<string>(0),
                    ClaimType = csv.GetField<string>(1),
                    ClaimValue = csv.GetField<string>(2)
                };
                //add the record if it isn't the header row
                if(!(record.ClaimType.Equals("ClaimType",StringComparison.OrdinalIgnoreCase)
                    && record.ClaimValue.Equals("ClaimValue",StringComparison.OrdinalIgnoreCase)))
                    records.Add(record);
            }
            return records;            
        }

        /// <summary>
        /// Constructs a new AppRoleChildClaimCache object, populating
        /// the child claims with data retrieved from GetAppRoleChildClaims()
        /// and combined with the application name
        /// </summary>
        /// <param name="env"></param>
        public AppRoleChildClaimCache(IHostEnvironment env = null) {

            ChildClaims = GetAppRoleChildClaims().Select(a => 
                new ChildClaim {
                ParentType = DomainClaimTypes.ApplicationRole(ApplicationName ?? env.ApplicationName),
                ParentValue = a.AppRole,
                ClaimType = a.ClaimType,
                ClaimValue = a.ClaimValue
            }) ;
        }

        public IEnumerable<ChildClaim> ChildClaims { get;  }
    }
}
