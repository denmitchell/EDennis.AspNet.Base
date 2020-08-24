create view AspNetApplicationsView as
	select a.Name Application,

	JSON_QUERY('[' +
		STUFF((
			SELECT ',' + char(34) + r.Nomen + char(34) 
				FROM dbo.AspNetRoles r
				WHERE r.[Application] = a.Name
				FOR XML PATH('')),1,1,'') + ']' ) as Roles

	from dbo.AspNetApplications a
go
