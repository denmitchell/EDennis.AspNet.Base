namespace EDennis.AspNet.Base.Security.Extensions {
    public static class ApplicationModelExtensions {
        public static ApplicationEditModel ToEditModel(this DomainApplication domainModel) {
            var editModel = new ApplicationEditModel {
                Name = domainModel.Name,
                SysStatus = domainModel.SysStatus,
                SysUser = domainModel.SysUser,
                Properties = domainModel.Properties,
            };
            return editModel;
        }
    }
}
