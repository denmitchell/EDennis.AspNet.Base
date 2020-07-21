namespace EDennis.AspNet.Base.Security.Extensions {
    public static class RoleModelExtensions {
        public static RoleEditModel ToEditModel(this DomainRole domainModel) {
            var editModel = new RoleEditModel {
                Name = domainModel.Name,
                SysStatus = domainModel.SysStatus,
                SysUser = domainModel.SysUser,
                Properties = domainModel.Properties,
                Organization = domainModel.Organization.Name,
                Application = domainModel.Application.Name,
                Claims = domainModel.RoleClaims.ToDictionary()                        
            };
            return editModel;
        }

    }
}
