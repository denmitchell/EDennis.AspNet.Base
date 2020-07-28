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
insert into @Roles(Id,Name,ApplicationId) values (1,'Admin',1), (2,'User',1), (3,'Readonly',2), (4,'User',2);

declare @UserRoles table (
	UserId int,
	RoleId int
);
insert into @UserRoles(UserId,RoleId) values (1,1), (1,2), (2,3);

declare @UserClaims table (
	Id int identity,
	UserId int,
	ClaimType varchar(100),
	ClaimValue varchar(100)
)
insert into @UserClaims(UserId,ClaimType,ClaimValue) values
	(1,'email','moe@stooges.org'),
	(1,'email','boss@stooges.org'),
	(2,'email','larry@stooges.org'),
	(2,'phone','18005551212');


declare @ExpandedUsers table (
	Id int,
	UserName varchar(30),
	RolesArray varchar(max),
	ClaimsArray varchar(max),
	RolesDictionary varchar(max),
	ClaimsDictionary varchar(max)
);

insert into @ExpandedUsers(id,UserName,RolesArray,ClaimsArray,RolesDictionary,ClaimsDictionary)
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

select u.*--, x.[Key], x.[Value] 
	from @ExpandedUsers u
	cross apply openjson(json_query(RolesDictionary,'$')) as x
	where x.[Key] = 'App1'
	--where json_value(RolesDictionary,'$') = 'App1'

select u.*
	from @ExpandedUsers u
	where exists (
		select 0 
			from @Roles r
			inner join @Applications a
				on a.Id = r.ApplicationId
			inner join @UserRoles ur
				on r.Id = ur.RoleId
			where a.Name = 'App1'
				and ur.UserId = u.Id
	)

declare @UserRoles2 table (
	UserName varchar(255),
	RoleName varchar(255)
);

declare @AppName varchar(255) = 'App1'


insert into @UserRoles2(UserName,RoleName) values
	('Moe','Admin'),
	('Larry','Admin'),
	('Moe','Readonly');

	with src as
	(
		select u.Id UserId, r.Id RoleId
			from @UserRoles2 ur
			inner join @Users u
				on u.UserName = ur.UserName
			inner join @Roles r
				on r.Name = ur.RoleName
			inner join @Applications a
				on a.Id = r.ApplicationId
			where a.Name = @AppName
	)
	merge @UserRoles tgt
		using src
			on src.UserId = tgt.UserId 
				and src.RoleId = tgt.RoleId
	when not matched by target
		then insert (UserId,RoleId)
			values (src.UserId, src.RoleId)
	when not matched by source
		and RoleId in (
		select r.Id from 
			@Roles r
			inner join @Applications a
				on a.Id = r.ApplicationId
			where a.Name = @AppName
		)		
		then delete;

select * from @UserRoles


declare @AppRoles table (
	ApplicationName varchar(255),
	RoleName varchar(255)
);

declare @UserName1 varchar(255) = 'Moe'

insert into @AppRoles(ApplicationName,RoleName) values
	('App1','Admin'),
	('App2','Readonly'),
	('App2','User');

	with src as
	(
		select u.Id UserId, r.Id RoleId
			from @AppRoles ar
			inner join @Roles r
				on r.Name = ar.RoleName
			inner join @Applications a
				on a.Id = r.ApplicationId
					and a.Name = ar.ApplicationName
			inner join @Users u
				on u.UserName = @UserName1
	)
	merge @UserRoles tgt
		using src
			on src.UserId = tgt.UserId 
				and src.RoleId = tgt.RoleId
	when not matched by target
		then insert (UserId,RoleId)
			values (src.UserId, src.RoleId)
	when not matched by source
		and UserId in (
		select u.Id from 
			@Users u
			where u.UserName = @UserName1
		)		
		then delete;

select * from @UserRoles
	



declare @UserClaims2 table (
	ClaimType varchar(255),
	ClaimValue varchar(255)
);

declare @UserName varchar(255) = 'Moe'

insert into @UserClaims2(ClaimType,ClaimValue) values
	('email','topdog@stooges.org'),
	('phone','18885559999');

	with src as
	(
		select u.Id UserId, c.ClaimType, c.ClaimValue
			from @UserClaims2 c
			cross join @Users u
			where u.UserName = @UserName
	)
--	select * from src;
	merge @UserClaims tgt
		using src
			on src.UserId = tgt.UserId
				and src.ClaimType = tgt.ClaimType
				and src.ClaimValue = tgt.ClaimValue
	when not matched by target
		then insert (UserId,ClaimType,ClaimValue)
			values (src.UserId, src.ClaimType, src.ClaimValue)
	when not matched by source
		and UserId in (
		select u.Id from 
			@Users u
			where u.UserName = @UserName
		)		
		then delete;

select * from @UserClaims
