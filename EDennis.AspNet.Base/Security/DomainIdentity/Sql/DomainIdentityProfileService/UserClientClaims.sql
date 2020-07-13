--create sequence seqUserClientClaims as int 
--	start with 1
--	increment by 1

create table UserClientClaims (
	--Id int not null default next value for seqUserClientClaims,
	UserId /*uniqueidentifier*/ nvarchar(450),
	ClientId int,
	Claims nvarchar(max)
	--ClaimType nvarchar(250),
	--ClaimValue nvarchar(250),
	constraint pkUserClientClaims
		primary key (UserId,ClientId),
	constraint fkUserClientClaims_User
		foreign key (UserId)
			references AspNetUsers (Id)
				on delete cascade,
	constraint fkUserClientClaims_Client
		foreign key (ClientId)
			references Clients (Id)
				on delete cascade,
)
create index idxUserClientClaims
	on UserClientClaims (UserId,ClientId);
go
create or alter procedure di.UpdateUserClientClaims (
	@UserId  /*uniqueidentifier*/ nvarchar(450),
	@ClientId int
)
as
begin
	set nocount on;
	with roles as (
		select r.Id, r.Name
			from AspNetRoles r
			inner join AspNetUserRoles ur
				on ur.RoleId = r.Id
					and ur.UserId = @UserId
			inner join AspNetApplications a	
				on a.Id = r.ApplicationId
			inner join ClientScopes cs
				on cs.ClientId = @ClientId
					and cs.Scope = a.Name
	),
	src as (
		select @UserId UserId, @ClientId ClientId, (
			select 'role', r.Name
				from roles r
			union
			select rc.ClaimType, rc.ClaimValue
				from AspNetRoleClaims rc
				inner join roles r
					on r.Id = rc.RoleId
			union
			select uc.ClaimType, uc.ClaimValue
				from AspNetUserClaims uc
				where uc.UserId = @UserId
			for json path
		) Claims
	)
	merge UserClientClaims u using src
		on u.UserId = src.UserId
			and u.ClientId = src.ClientId
		when matched
			then update set Claims = src.Claims
		when not matched by source
			then delete
		when not matched by target
			then insert (UserId, ClientId, Claims)
				values (src.UserId, src.ClientId, src.Claims);
end