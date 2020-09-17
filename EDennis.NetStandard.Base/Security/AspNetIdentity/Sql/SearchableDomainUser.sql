CREATE OR ALTER VIEW [dbo].[SearchableDomainUser] AS
SELECT u.Id, u.UserName, u.Email, u.PhoneNumber, u.Organization, u.OrganizationAdmin, u.SuperAdmin,
  stuff((SELECT DISTINCT ',' + a.Name 
	FROM AspNetApplications a
	WHERE 
		u.OrganizationAdmin = 1 OR u.SuperAdmin = 1
	OR EXISTS (
		SELECT 0 from 
		AspNetUserClaims uc 
		WHERE uc.UserId = u.Id AND uc.ClaimType = 'role:' +  a.Name			
	)
	FOR XML PATH('')),1,1,'') Applications
	FROM AspNetUsers u
