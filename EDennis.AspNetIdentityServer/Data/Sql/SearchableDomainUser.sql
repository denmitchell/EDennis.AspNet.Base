CREATE OR ALTER VIEW [dbo].[SearchableDomainUser] AS
SELECT u.Id, u.UserName, u.Email, u.PhoneNumber, u.Organization, u.OrganizationAdmin,
  stuff((SELECT DISTINCT ',' + a.Name 
	FROM AspNetApplications a
	WHERE EXISTS (
		SELECT 0 from 
		AspNetUserClaims uc 
		WHERE uc.UserId = u.Id 
			AND ( 
				( uc.ClaimType = 'app:role' AND uc.ClaimValue like a.Name + ':%' ) OR
				( uc.ClaimType = '*:role' AND uc.ClaimValue = '*:admin' )
			)
	)
	FOR XML PATH('')),1,1,'') Applications
	FROM AspNetUsers u
/*
CREATE OR ALTER VIEW [dbo].[SearchableDomainUser] 
WITH SCHEMABINDING AS
SELECT u.Id, a.Name Application, u.UserName, u.Email, u.PhoneNumber, u.Organization, u.OrganizationAdmin, COUNT_BIG(*) ApplicationCount
	FROM dbo.AspNetUsers u
	INNER JOIN dbo.AspNetUserClaims uc 
		ON uc.UserId = u.Id 
	INNER JOIN dbo.AspNetApplications a
		ON  a.Name = LEFT(uc.ClaimValue,CHARINDEX(':',uc.ClaimValue+':')-1)
	WHERE uc.ClaimType = 'app:role'
	GROUP BY u.Id, a.Name, u.UserName, u.Email, u.PhoneNumber, u.Organization, u.OrganizationAdmin
GO
CREATE UNIQUE CLUSTERED INDEX ucidx_SearchableDomainUser
	ON dbo.SearchableDomainUser (Id, [Application])
GO
CREATE INDEX idx_SearchableDomainUserSB_UserName
	ON dbo.SearchableDomainUser (UserName)
GO
CREATE INDEX idx_SearchableDomainUserSB_Organization
	ON dbo.SearchableDomainUser (Organization)
GO
CREATE INDEX idx_SearchableDomainUserSB_Application
	ON dbo.SearchableDomainUser ([Application])
GO
*/