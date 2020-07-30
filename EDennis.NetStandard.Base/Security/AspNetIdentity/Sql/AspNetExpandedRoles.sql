create view AspNetExpandedRoles as
	select r.Id, r.Name, r.NormalizedName, r.ApplicationId, r.ApplicationName,
	r.SysUser, r.SysStatus, r.SysStart, r.SysEnd, r.Properties
		from dbo.AspNetRoles r
        inner join dbo.AspNetApplications a
            on r.ApplicationId = a.Id
go
