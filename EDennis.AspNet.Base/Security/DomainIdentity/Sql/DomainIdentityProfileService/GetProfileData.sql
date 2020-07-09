create or alter procedure di.DomainIdentityProfileService_GetProfileData(
    @UserId varchar(450),
	@ClientId int,
	@RequestedResourceApiScopes StringTableType readonly,
	@RequestedUserClaimTypes StringTableType readonly
	)
	as 
	set nocount on
	with r as (
		select r.* 
			from AspNetRoles r
		inner join AspNetApplications a
			on a.Id = r.ApplicationId
		inner join AspNetUserRoles ur
			on ur.RoleId = r.Id
				and ur.UserId = @UserId
		inner join ClientScopes cs
			on cs.ClientId = @ClientId
		inner join @RequestedResourceApiScopes rras
			on rras.Value = cs.Scope
		inner join ApiResources ar
			on ar.Name = cs.Scope
		left outer join ApiProperties ap
			on ap.[Key] = 'ApplicationName'
			   and ap.ApiResourceId = ar.Id
		where a.Name = isnull(ap.Value,ar.Name)
	)
	select uc.ClaimType, uc.ClaimValue
		from AspNetUserClaims uc
		inner join @RequestedUserClaimTypes ruct
			on uc.ClaimType = ruct.Value
		where uc.UserId = @UserId
	union select 'role', r.Name
		from r
	union select rc.ClaimType, rc.ClaimValue
		from AspNetRoleClaims rc
		inner join r
			on r.Id = rc.RoleId;			