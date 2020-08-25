create view AspNetUsersView as
	select u.Id, u.UserName, u.NormalizedUserName, u.Email NormalizedEmail, u.EmailConfirmed, u.PhoneNumber, u.PhoneNumberConfirmed,
	u.TwoFactorEnabled, u.LockoutBegin, u.LockoutEnd, u.LockoutEnabled, u.AccessFailedCount,
	u.Organization,

	JSON_QUERY('{' 
		+ STUFF((
			SELECT ',' + char(34) + ApplicationName + char(34) + ':[' + RoleNames + ']' 
				FROM (
					SELECT a.Name ApplicationName,
						STUFF((
							SELECT ',' + char(34) + r.Name + char(34) 
								FROM dbo.AspNetRoles r
								INNER JOIN dbo.AspNetUserRoles ur
									ON ur.RoleId = r.Id
								WHERE u.Id = ur.UserId
									AND r.[Application] = a.Name
								FOR XML PATH('')),1,1,'') as RoleNames
						FROM dbo.AspNetApplications a
						GROUP BY a.Name
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
go
