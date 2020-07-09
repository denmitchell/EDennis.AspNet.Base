create or alter procedure di.DomainRoleManager_Update(
    @Id uniqueidentifier,
	@ApplicationId uniqueidentifier,
	@OrganizationId uniqueidentifier,
	@Title nvarchar(256))
as
begin
	set nocount on

	if (OrganizationId > 0 and @ApplicationId > 0)
		update r 
			set Name = @Title + '@' + o.Name + '@' + a.Name,
			ApplicationId = @ApplicationId,
			OrganizationId = @OrganizationId,
			RoleName = @Title,
			NormalizedName = upper(@Title + '@' + o.Name + '@' + a.Name),
			ConcurrencyStamp = newid()
			from AspNetRoles r
			inner join AspNetApplications a
				on a.Id = @ApplicationId
			inner join AspNetOrganizations o
				on o.Id = @OrganizationId
			where r.Id = @Id
	else if (OrganizationId > 0)
		update r 
			set Name = @Title + '@' + o.Name,
			ApplicationId = @ApplicationId,
			OrganizationId = @OrganizationId,
			RoleName = @Title,
			NormalizedName = upper(@Title + '@' + o.Name),
			ConcurrencyStamp = newid()
			from AspNetRoles r
			inner join AspNetOrganizations o
				on o.Id = @OrganizationId
			where r.Id = @Id
	else if (ApplicationId > 0)
		update r 
			set Name = @Title + '@' + a.Name,
			ApplicationId = @ApplicationId,
			OrganizationId = @OrganizationId,
			RoleName = @Title,
			NormalizedName = upper(@Title + '@' + a.Name),
			ConcurrencyStamp = newid()
			from AspNetRoles r
			inner join AspNetApplications a
				on a.Id = @ApplicationId
			where r.Id = @Id
	else
		update r 
			set Name = @Title,
			ApplicationId = @ApplicationId,
			OrganizationId = @OrganizationId,
			RoleName = @Title,
			NormalizedName = upper(@Title),
			ConcurrencyStamp = newid()
			from AspNetRoles r
			inner join AspNetApplications a
				on a.Id = @ApplicationId
			where r.Id = @Id

end
