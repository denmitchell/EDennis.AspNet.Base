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
