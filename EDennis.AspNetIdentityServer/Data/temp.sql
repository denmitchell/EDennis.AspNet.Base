with u as (select Users.*,
	(select r.* 
		from AspNetRoles r 
		inner join AspNetUserRoles ur
			on ur.RoleId = r.Id
		where Users.Id = ur.UserId
			and r.Name like '%Admin'
		for json path) as Roles,
	(select uc.*
		from AspNetUserClaims uc
		where Users.Id = uc.UserId
		for json path) as Claims
	from AspNetUsers Users
	where exists (
		select 0 
			from AspNetUserRoles ur
			inner join AspNetRoles r
				on ur.RoleId = r.Id
			where r.Name like '%Admin'
				and ur.UserId = Users.Id)
	order by Users.UserName
	offset 0 rows
	fetch next 10 rows only
) select * from u for json path	