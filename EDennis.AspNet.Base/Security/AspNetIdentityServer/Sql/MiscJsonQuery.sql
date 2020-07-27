declare @Applications table (
	Id int,
	Name varchar(30)
);
insert into @Applications(Id,Name) values (1,'App1'), (2,'App2');
declare @Users table (
	Id int,
	UserName varchar(30)
);
insert into @Users(Id,UserName) values (1,'Moe'), (2,'Larry');
declare @Roles table (
	Id int,
	Name varchar(30),
	ApplicationId int
);
insert into @Roles(Id,Name,ApplicationId) values (1,'Admin',1), (2,'User',1), (3,'Readonly',2);

declare @UserRoles table (
	UserId int,
	RoleId int
);
insert into @UserRoles(UserId,RoleId) values (1,1), (1,2), (2,3);

declare @UserClaims table (
	UserId int,
	ClaimType varchar(100),
	ClaimValue varchar(100)
)
insert into @UserClaims(UserId,ClaimType,ClaimValue) values
	(1,'email','moe@stooges.org'),
	(1,'email','boss@stooges.org'),
	(2,'email','larry@stooges.org'),
	(2,'phone','18005551212');


select u.*,
	JSON_QUERY('['
		+ STUFF((
			SELECT ',' + char(34) + a.Name + ':' + r.Name + char(34)
				FROM @Roles r
				INNER JOIN @UserRoles ur
					ON ur.RoleId = r.Id
				INNER JOIN @Applications a
					ON a.Id = r.ApplicationId
				WHERE u.Id = ur.UserId
				FOR XML PATH('')),1,1,'') + ']' ) as Roles,
	JSON_QUERY('['
		+ STUFF((
			SELECT ',' + char(34) + uc.ClaimType + ':' + uc.ClaimValue + char(34)
				FROM @UserClaims uc
				WHERE u.Id = uc.UserId
				FOR XML PATH('')),1,1,'') + ']' ) as Claims,
	JSON_QUERY('{' 
		+ STUFF((
			SELECT ',' + char(34) + ApplicationName + char(34) + ':[' + RoleNames + ']' 
				FROM (
					SELECT a.Id, a.Name ApplicationName,
						STUFF((
							SELECT ',' + char(34) + r.Name + char(34) 
								FROM @Roles r
								INNER JOIN @UserRoles ur
									ON ur.RoleId = r.Id
								WHERE u.Id = ur.UserId
									AND r.ApplicationId = a.Id
								FOR XML PATH('')),1,1,'') as RoleNames
						FROM @Applications a
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
								FROM @UserClaims uc
								WHERE u.Id = uc.UserId
									and uc.ClaimType = uc2.ClaimType
								FOR XML PATH('')),1,1,'') as ClaimValues
						FROM @UserClaims uc2
						GROUP BY uc2.ClaimType
						) stf
				FOR XML PATH('')),1,1,'') + '}' ) as ClaimsDictionary
	FROM @Users u
	FOR JSON PATH
