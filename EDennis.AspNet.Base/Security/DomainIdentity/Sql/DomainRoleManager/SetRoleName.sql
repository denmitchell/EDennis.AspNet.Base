create or alter procedure di.DomainRoleManager_SetRoleName(
    @Id nvarchar(450),
	@NewName nvarchar(256) )
as
begin
	set nocount on

	declare @sep1 int = charindex('@', @NewName, 1)
	declare @sep2 int = charindex('@', @NewName, @sep1 + 1)
	declare @len int = len(@NewName)

	if @sep1 = 0
		update AspNetRoles set Name = @NewName
	else if @sep2 > 0
		update r
			set Name = @NewName,
				Title = substring(@NewName, 1, @sep1 - 1),
				OrganizationId = o.Id, ApplicationId = a.Id
			from AspNetRoles r
			inner join AspNetOrganizations o
			  on o.Name = substring(@NewName, @sep1 + 1, @sep2 - @sep1 - 1)
			inner join AspNetApplications a
			  on a.Name = substring(@NewName, @sep2 + 1, @len - @sep2)
			where r.Id = @Id
	else
		update r
			set Name = @NewName,
				Title = substring(@NewName, 1, @sep1 - 1),
				OrganizationId = o.Id, ApplicationId = a.Id
			from AspNetRoles r
			left outer join AspNetOrganizations o
			  on o.Name = substring(@NewName, @sep1 + 1, @len - @sep1)
			left outer join AspNetApplications a
			  on a.Name = substring(@NewName, @sep1 + 1, @len - @sep1)
			where r.Id = @Id

end
