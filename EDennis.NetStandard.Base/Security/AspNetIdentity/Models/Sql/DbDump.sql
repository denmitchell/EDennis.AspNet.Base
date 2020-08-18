USE [AspNetIdentityServer]
GO

/****** Object:  View [dbo].[DbDump]    Script Date: 8/18/2020 1:18:52 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER VIEW [dbo].[DbDump] AS
SELECT('{' + STUFF((
SELECT 
	JSON_QUERY(
		STUFF((
			SELECT ar.Name,
				JSON_QUERY('[' + stuff((SELECT ', "' + s.Scope + '"' FROM ApiResourceScopes s WHERE s.ApiResourceId = ar.Id FOR XML PATH('')),1,1,'') + ']') Scopes,
				JSON_QUERY('[' + stuff((SELECT ', "' + c.Type + '"' FROM ApiResourceClaims c WHERE c.ApiResourceId = ar.Id FOR XML PATH('')),1,1,'') + ']') Claims
				FROM ApiResources ar
				FOR JSON PATH),1,1,'')) ApiResources,
	JSON_QUERY('[' + 
		STUFF((
			SELECT c.ClientId, c.RequireConsent, c.RequirePkce, c.AllowAccessTokensViaBrowser, c.AllowOfflineAccess, c.ClientClaimsPrefix,
				JSON_QUERY('[' + stuff((SELECT ', "' + g.GrantType + '"' FROM ClientGrantTypes g WHERE g.ClientId = c.Id FOR XML PATH('')),1,1,'') + ']') GrantTypes,	
				JSON_QUERY('[' + stuff((SELECT ', "' + u.RedirectUri + '"' FROM ClientRedirectUris u WHERE u.ClientId = c.Id FOR XML PATH('')),1,1,'') + ']') RedirectUris,	
				JSON_QUERY('[' + stuff((SELECT ', "' + u.PostLogoutRedirectUri + '"' FROM ClientPostLogoutRedirectUris u WHERE u.ClientId = c.Id FOR XML PATH('')),1,1,'') + ']') PostLogoutRedirectUris,	
				JSON_QUERY('[' + stuff((SELECT ', "' + s.Scope + '"' FROM ClientScopes s WHERE s.ClientId = c.Id FOR XML PATH('')),1,1,'') + ']') Scopes,
				JSON_QUERY('{' 
					+ STUFF((
						SELECT ',' + char(34) + Type + char(34) + ':[' + ClaimValues + ']' 
							FROM (
								SELECT cc2.Type,
									STUFF((
										SELECT ',' + char(34) + cc.Value + char(34) 
											FROM ClientClaims cc
											WHERE cc.ClientId = c.Id
												AND cc.Type = cc2.Type
											FOR XML PATH('')),1,1,'') as ClaimValues
									FROM ClientClaims cc2
									GROUP BY cc2.Type
									) stf
							FOR XML PATH('')),1,1,'') + '}' ) as Claims
				FROM Clients c
				FOR JSON PATH),1,1,'')
				+ ']') Clients,
	JSON_QUERY('[' +
		STUFF((
			SELECT u.Id, u.UserName, u.Email, u.EmailConfirmed, u.PhoneNumber, 
				u.LockoutBegin, u.LockoutEnd, u.LockoutEnabled, u.Organization,  
				JSON_QUERY('{' 
					+ STUFF((
						SELECT ',' + char(34) + [Application] + char(34) + ':[' + RoleNames + ']' 
							FROM (
								SELECT a.Name [Application],
									STUFF((
										SELECT ',' + char(34) + r.Name + char(34) 
											FROM dbo.AspNetRoles r
											INNER JOIN dbo.AspNetUserRoles ur
												ON ur.RoleId = r.Id
											WHERE u.Id = ur.UserId
												AND r.Application = a.Name
											FOR XML PATH('')),1,1,'') as RoleNames
									FROM dbo.AspNetApplications a
									GROUP BY a.Name
									) stf
							FOR XML PATH('')),1,1,'') + '}' ) as Roles,
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
							FOR XML PATH('')),1,1,'') + '}' ) as Claims
	
				FROM dbo.AspNetUsers u
				INNER JOIN dbo.AspNetUserRoles ur
					ON ur.UserId = u.Id
				INNER JOIN dbo.AspNetRoles r
					ON ur.RoleId = r.Id
	
				FOR JSON PATH),1,1,'')
								+ ']') Users
	FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
),1,1,'')) as json
