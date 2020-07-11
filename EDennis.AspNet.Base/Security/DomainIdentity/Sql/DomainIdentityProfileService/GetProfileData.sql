create or alter procedure di.DomainIdentityProfileService_GetProfileData(
    @UserId uniqueidentifier,
	@ClientId int,
	@RequestedResourceApiScopes StringTableType readonly,
	@RequestedUserClaimTypes StringTableType readonly
	)
	as 
	set nocount on

	select 'Organization' ClaimType, o.Name ClaimValue
		from AspNetUsers u
		inner join AspNetOrganizations o
			on o.Id = u.OrganizationId
		where u.Id = @UserId
	union
	select uc.ClaimType, uc.ClaimValue
		from AspNetUserClaims uc
		inner join @RequestedUserClaimTypes ruct
			on uc.ClaimType = ruct.Value
		where uc.UserId = @UserId
	union
	select 'role_id' ClaimType, ur.RoleId ClaimValue
		from AspNetUserRoles ur
		inner join AspNetRoles r
			on ur.RoleId = r.Id
				and ur.UserId = @UserId
		inner join AspNetApplications a
			on a.Id = r.ApplicationId
		inner join ClientScopes cs
			on cs.ClientId = @ClientId
				and cs.Scope = a.Name
		inner join @RequestedResourceApiScopes rras
				on rras.Value = cs.Scope
;			