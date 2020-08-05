create view ClientApplications
	with schemabinding as 
	select c.ClientId, ar.Name ApplicationName 
		from dbo.ApiResources ar
        inner join dbo.ClientScopes cs
            on cs.Scope = ar.Name
        inner join dbo.Clients c
            on cs.ClientId = c.Id;
go
create unique clustered index 
	ucidx_ClientApplications
		on ClientApplications(ClientId,ApplicationName);
go
create nonclustered index
	idx_ClientApplications_AppName
		on ClientApplications(ApplicationName);
go
create view UserApplicationRoles
	with schemabinding as 
	select u.Id UserId, a.Name ApplicationName, r.Name RoleName 
		from dbo.AspNetUsers u
        inner join dbo.AspNetUserRoles ur
			on ur.UserId = u.Id
		inner join dbo.AspNetRoles r
			on ur.RoleId = r.Id
        inner join dbo.AspNetApplications a
            on r.ApplicationId = a.Id
go
create unique clustered index 
	ucidx_UserApplicationRoles
		on UserApplicationRoles(UserId,ApplicationName,RoleName);
go
create nonclustered index
	idx_UserApplicationRoles_AppName
		on UserApplicationRoles(ApplicationName);
go
create view UserClientApplicationRoles
	as
	select uar.UserId, ca.ClientId, uar.ApplicationName, uar.RoleName
		from UserApplicationRoles uar
		inner join ClientApplications ca
			on uar.ApplicationName = ca.ApplicationName
