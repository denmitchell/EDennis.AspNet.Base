create or alter procedure di.DomainRoleManager_Create(
	@Id uniqueidentifier,
	@ApplicationId uniqueidentifier,
	@OrganizationId uniqueidentifier,
	@Title nvarchar(256))
as
begin
	set nocount on

	if (OrganizationId is not null and @ApplicationId is not null)
		insert into AspNetRoles(Id,Name,NormalizedName,ApplicationId,OrganizationId,RoleName,ConcurrencyStamp) 
			select	isnull(@Id,newsequentialid()),
					@Title + '@' + o.Name + '@' + a.Name,
					upper(@Title + '@' + o.Name + '@' + a.Name),
					@ApplicationId, @OrganizationId,
					@Title, newid()
			from AspNetRoles r
			inner join AspNetApplications a
				on a.Id = @ApplicationId
			inner join AspNetOrganizations o
				on o.Id = @OrganizationId
	else if (OrganizationId is not null)
		insert into AspNetRoles(Id,Name,NormalizedName,ApplicationId,OrganizationId,RoleName,ConcurrencyStamp) 
			select	isnull(@Id,newsequentialid()),
					@Title + '@' + o.Name,
					upper(@Title + '@' + o.Name),
					@ApplicationId, @OrganizationId,
					@Title, newid()
			from AspNetRoles r
			inner join AspNetOrganizations o
				on o.Id = @OrganizationId
	else if (ApplicationId is not null)
		insert into AspNetRoles(Id,Name,NormalizedName,ApplicationId,OrganizationId,RoleName,ConcurrencyStamp) 
			select	isnull(@Id,newsequentialid()),
					@Title + '@' + a.Name,
					upper(@Title + '@' + a.Name),
					@ApplicationId, @OrganizationId,
					@Title, newid()
			from AspNetRoles r
			inner join AspNetApplications a
				on a.Id = @ApplicationId
	else
		insert into AspNetRoles(Id,Name,NormalizedName,ApplicationId,OrganizationId,RoleName,ConcurrencyStamp) 
			select	isnull(@Id,newsequentialid()),
					@Title,
					upper(@Title),
					@ApplicationId, @OrganizationId,
					@Title, newid()
			from AspNetRoles r

end
