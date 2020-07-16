WITH u as (
  SELECT u.*, o.Name OrganizationName
    FROM AspNetUsers u 
    LEFT OUTER JOIN AspNetOrganizations o
        on o.Id = u.OrganizationId
    ORDER BY UserName 
    OFFSET @skip ROWS 
    FETCH NEXT @take ROWS ONLY
)
SELECT 
    Users.Id, Users.UserName, Users.Email, 
    CASE Users.EmailConfirmed WHEN 1 then 'true' else 'false' end EmailConfirmed, 
    Users.PasswordHash, Users.SecurityStamp, Users.ConcurrencyStamp, Users.PhoneNumber, 
    CASE Users.PhoneNumberConfirmed WHEN 1 then 'true' else 'false' end PhoneNumberConfirmed,
    CASE Users.TwoFactorEnabled WHEN 1 then 'true' else 'false' end TwoFactorEnabled, 
    Users.LockoutStart, Users.LockoutEnd, Users.AccessFailedCount, Users.OrganizationName,
	JSON_QUERY('[' 
		+ STUFF((SELECT ',' + char(34) + r.Name + char(34)
		FROM AspNetRoles r
		INNER JOIN AspNetUserRoles ur
			ON ur.RoleId = r.Id
		WHERE Users.Id = ur.UserId
		FOR XML PATH('')),1,1,'') + ']' ) as Roles,
	(
		SELECT uc.ClaimType, uc.ClaimValue
		FROM AspNetUserClaims uc
		WHERE Users.Id = uc.UserId
		FOR JSON PATH
	) as Claims
	FROM u Users 
	FOR JSON PATH
