create view AspNetExpandedUsers as
	select u.Id, u.UserName, u.Email, u.EmailConfirmed, u.PhoneNumber, u.PhoneNumberConfirmed,
	u.TwoFactorEnabled, u.LockoutBegin, u.LockoutEnd, u.LockoutEnabled, u.AccessFailedCount,
	o.Name OrganizationName,

	JSON_QUERY('{' 
		+ STUFF((
			SELECT ',' + char(34) + ApplicationName + char(34) + ':[' + RoleNames + ']' 
				FROM (
					SELECT a.Id, a.Name ApplicationName,
						STUFF((
							SELECT ',' + char(34) + r.Name + char(34) 
								FROM dbo.AspNetRoles r
								INNER JOIN dbo.AspNetUserRoles ur
									ON ur.RoleId = r.Id
								WHERE u.Id = ur.UserId
									AND r.ApplicationId = a.Id
								FOR XML PATH('')),1,1,'') as RoleNames
						FROM dbo.AspNetApplications a
						GROUP BY a.Id, a.Name
						) stf
				FOR XML PATH('')),1,1,'') + '}' ) as RolesDictionary,
	JSON_QUERY('{' 
		+ STUFF((
			SELECT ',' + char(34) + ClaimType + char(34) + ':[' + ClaimValues + ']' 
				FROM (
					SELECT uc2.ClaimType,
						STUFF((
							SELECT ',' + char(34) + uc.ClaimValue + char(34) 
								FROM dbo.AspNetUserClaims uc
								WHERE u.Id = uc.UserId
									and uc.ClaimType = uc2.ClaimType
								FOR XML PATH('')),1,1,'') as ClaimValues
						FROM dbo.AspNetUserClaims uc2
						GROUP BY uc2.ClaimType
						) stf
				FOR XML PATH('')),1,1,'') + '}' ) as ClaimsDictionary

	
		from dbo.AspNetUsers u
		inner join dbo.AspNetOrganizations o
			on o.id = u.OrganizationId
        inner join dbo.AspNetUserRoles ur
			on ur.UserId = u.Id
		inner join dbo.AspNetRoles r
			on ur.RoleId = r.Id
        inner join dbo.AspNetApplications a
            on r.ApplicationId = a.Id
go
