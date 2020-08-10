namespace EDennis.NetStandard.Base {

    /// <summary>
    /// Ensures that RoleManager requires uniqueness of combination of 
    ///   ApplicationId and NormalizedRoleName, rather than just NormalizedRoleName
    ///   
    /// NOTE: In Startup.ConfigureServices:
    ///         services.AddDefaultIdentity<DomainUser>(options => options.SignIn.RequireConfirmedAccount = true)
    ///            .AddEntityFrameworkStores<DomainIdentityDbContext>()
    ///            .AddRoleValidator<DomainRoleValidator>(); //add this line
    ///   
    /// Adapted from https://stackoverflow.com/a/59466933
    /// </summary>
    public class DomainRoleValidator : DomainRoleValidator<DomainUser,DomainOrganization,DomainUserClaim,DomainUserLogin,DomainUserToken,DomainRole,DomainApplication,DomainRoleClaim,DomainUserRole>{
    }
}
