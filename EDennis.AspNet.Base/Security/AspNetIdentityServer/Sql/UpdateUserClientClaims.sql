/*
create table UserClientClaims (
	UserId uniqueidentifier,
	ClientId nvarchar(200),
	Claims nvarchar(max)
	constraint pkUserClientClaims
		primary key (UserId,ClientId),
	constraint fkUserClientClaims_User
		foreign key (UserId)
			references AspNetUsers (Id)
				on delete cascade,
	constraint fkUserClientClaims_Client
		foreign key (ClientId)
			references Clients (Id)
				on delete cascade,
)
go
*/
create or alter procedure di.UpdateUserClientClaims (
	@UserId  uniqueidentifier,
	@ClientId nvarchar(200)
)
as
begin
	set nocount on;
	with usr as (
		select u.Id, u.Organization
			from AspNetUsers u
			inner join AspNetOrganizations o
				on u.OrganizationId = o.OrganizationId
			where u.Id = @UserId
	),
	roles as (
		select r.Id, r.Name, ISNULL(r.Organization,usr.Organization) Organization
			from AspNetRoles r
			inner join AspNetUserRoles ur
				on ur.RoleId = r.Id
					and ur.UserId = @UserId
			inner join usr 
				on usr.Id = ur.UserId
			inner join AspNetApplications a	
				on a.Id = r.ApplicationId
			left outer join AspNetOrganizations o	
				on o.Id = r.OrganizationId
			inner join ClientScopes cs
				on cs.ClientId = @ClientId
					and cs.Scope = a.Name
	),
	src as (
		select @UserId UserId, @ClientId ClientId, (
			select ClaimType, ClaimValue
				from (
					select 'role' ClaimType, r.Name + '@' + r.Organization ClaimValue
						from roles r
					union
					select 'organization', usr.Organization
						from usr
					union
					select rc.ClaimType, rc.ClaimValue
						from AspNetRoleClaims rc
						inner join roles r
							on r.Id = rc.RoleId
					union
					select uc.ClaimType, uc.ClaimValue
						from AspNetUserClaims uc
						inner join ApiClaims ac
							on ac.Type = uc.ClaimType
						inner join ApiResources ar
							on ar.Id = ac.ApiResourceId
						inner join ClientScopes cs
							on cs.Scope = ar.Name
						inner join Clients c
							on c.Id = cs.ClientId
						where uc.UserId = @UserId
							and c.ClientId = @ClientId
						group by uc.ClaimType, uc.ClaimValue
				) claims
			for json path
		) Claims
	)
	merge UserClientClaims u using src
		on u.UserId = src.UserId
			and u.ClientId = src.ClientId
		when matched
			then update set Claims = src.Claims
		when not matched by source
			then delete
		when not matched by target
			then insert (UserId, ClientId, Claims)
				values (src.UserId, src.ClientId, src.Claims);
end