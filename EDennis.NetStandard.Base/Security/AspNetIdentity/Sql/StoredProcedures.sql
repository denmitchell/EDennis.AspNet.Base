create type di.UserRolesType as table (
	UserName varchar(255),
	RoleName varchar(255)
);
go
create type di.AppRolesType as table (
	ApplicationName varchar(255), 
	RoleName varchar(255)
);
go
create type di.DomainUserRoleType as table(
	UserId uniqueidentifier,
	RoleId uniqueidentifier,
	SysUser varchar(255),
	SysStatus int
);
go
create type di.ClaimsType as table (
	ClaimType varchar(255),
	ClaimValue varchar(255)
);
go 
create type di.DomainUserClaimsType as table (
	UserId uniqueidentifier,
	ClaimType varchar(255),
	ClaimValue varchar(255)
);
go 
create or alter procedure di.UpdateDomainUserRolesForAppUsers(
	@AppName varchar(255),
	@SysUser varchar(255),
	@DomainUserRoles di.DomainUserRolesType readonly 
) as
begin
	set nocount on;

	merge AspNetUserRoles tgt
		using @DomainUserRoles src
			on src.UserId = tgt.UserId 
				and src.RoleId = tgt.RoleId
	when not matched by target
		then insert (UserId, RoleId, SysUser, SysStatus)
			values (src.UserId, src.RoleId, src.SysUser, src.SysStatus)
	when not matched by source
		and RoleId in (
		select r.Id from 
			AspNetRoles r
			inner join AspNetApplications a
				on a.Id = r.ApplicationId
			where a.Name = @AppName
		)		
		then delete;
end

go
create or alter procedure di.UpdateUserRolesForAppUsers(
	@AppName varchar(255),
	@SysUser varchar(255),
	@UserRoles di.UserRolesType readonly 
) as
begin
	set nocount on;

	with src as
	(
		select u.Id UserId, r.Id RoleId, @SysUser SysUser, 0 SysStatus
			from @UserRoles ur
			inner join AspNetUsers u
				on u.UserName = ur.UserName
			inner join AspNetRoles r
				on r.Name = ur.RoleName
			inner join AspNetApplications a
				on a.Id = r.ApplicationId
			where a.Name = @AppName
	)
	merge AspNetUserRoles tgt
		using src
			on src.UserId = tgt.UserId 
				and src.RoleId = tgt.RoleId
	when not matched by target
		then insert (UserId, RoleId, SysUser, SysStatus)
			values (src.UserId, src.RoleId, src.SysUser, src.SysStatus)
	when not matched by source
		and RoleId in (
		select r.Id from 
			AspNetRoles r
			inner join AspNetApplications a
				on a.Id = r.ApplicationId
			where a.Name = @AppName
		)		
		then delete;


end

go
create or alter procedure di.UpdateDomainUserRolesForUser(
	@UserId uniqueidentifier,
	@SysUser varchar(255),
	@DomainUserRoles di.DomainUserRolesType readonly 
) as
begin
	set nocount on;

	merge AspNetUserRoles tgt
		using @DomainUserRoles src
			on src.UserId = tgt.UserId 
				and src.RoleId = tgt.RoleId
	when not matched by target
		then insert (UserId,RoleId, SysUser, SysStatus)
			values (src.UserId, src.RoleId, src.SysUser, src.SysStatus)
	when not matched by source
		and UserId in (
		select u.Id from 
			@AspNetUsers u
			where u.UserName = @UserName1
		)		
		then delete;
end

go
create or alter procedure di.UpdateUserRolesForUser(
	@UserId uniqueidentifier,
	@SysUser varchar(255),
	@AppRoles di.AppRolesType readonly 
) as
begin
	set nocount on;

	with src as
	(
		select u.Id UserId, r.Id RoleId, @SysUser SysUser, 0 SysStatus
			from @AppRoles ar
			inner join AspRoles r
				on r.Name = ar.RoleName
			inner join AspNetApplications a
				on a.Id = r.ApplicationId
					and a.Name = ar.ApplicationName
			inner join AspNetUsers u
				on u.UserName = @UserName1
	)
	merge AspNetUserRoles tgt
		using src
			on src.UserId = tgt.UserId 
				and src.RoleId = tgt.RoleId
	when not matched by target
		then insert (UserId,RoleId, SysUser, SysStatus)
			values (src.UserId, src.RoleId, src.SysUser, src.SysStatus)
	when not matched by source
		and UserId in (
		select u.Id from 
			@AspNetUsers u
			where u.UserName = @UserName1
		)		
		then delete;
end

go
create or alter procedure di.UpdateDomainUserClaims(
	@UserName varchar(255),
	@SysUser varchar(255),
	@DomainUserClaims di.DomainUserClaimsType readonly 
) as
begin
	set nocount on;

	merge AspNetUserClaims tgt
		using @DomainUserClaims src
			on src.UserId = tgt.UserId
				and src.ClaimType = tgt.ClaimType
				and src.ClaimValue = tgt.ClaimValue
	when not matched by target
		then insert (UserId,ClaimType,ClaimValue,SysUser,SysStatus)
			values (src.UserId, src.ClaimType, src.ClaimValue, src.SysUser, src.SysStatus)
	when not matched by source
		and UserId in (
		select u.Id from 
			AspNetUsers u
			where u.UserName = @UserName
		)		
		then delete;
end

go
create or alter procedure di.UpdateUserClaims(
	@UserName varchar(255),
	@SysUser varchar(255),
	@Claims di.ClaimsType readonly 
) as
begin
	set nocount on;
	with src as
	(
		select u.Id UserId, c.ClaimType, c.ClaimValue, @SysUser SysUser, 0 SysStatus
			from @Claims c
			inner join AspNetUsers u
				on u.UserName = @UserName
	)
	merge AspNetUserClaims tgt
		using src
			on src.UserId = tgt.UserId
				and src.ClaimType = tgt.ClaimType
				and src.ClaimValue = tgt.ClaimValue
	when not matched by target
		then insert (UserId,ClaimType,ClaimValue,SysUser,SysStatus)
			values (src.UserId, src.ClaimType, src.ClaimValue, src.SysUser, src.SysStatus)
	when not matched by source
		and UserId in (
		select u.Id from 
			AspNetUsers u
			where u.UserName = @UserName
		)		
		then delete;
end
