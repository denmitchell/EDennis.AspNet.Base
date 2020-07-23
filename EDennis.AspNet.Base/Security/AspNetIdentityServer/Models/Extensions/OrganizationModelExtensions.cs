namespace EDennis.AspNet.Base.Security.Extensions {
    public static class OrganizationModelExtensions {
        public static OrganizationEditModel ToEditModel(this DomainOrganization domainModel) {
            var editModel = new OrganizationEditModel {
                Name = domainModel.Name,
                SysStatus = domainModel.SysStatus,
                SysUser = domainModel.SysUser,
                Properties = domainModel.Properties,
            };
            return editModel;
        }
    }
}
