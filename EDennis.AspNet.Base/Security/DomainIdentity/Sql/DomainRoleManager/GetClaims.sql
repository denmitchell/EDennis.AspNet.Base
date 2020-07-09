create or alter procedure di.DomainRoleManager_GetClaims(
	@RoleNames StringTableType readonly)
as
begin
	set nocount on

select rc.ClaimType, rc.ClaimValue 
  from AspNetRoleClaims rc
  inner join AspNetRoles r
     on r.Id = rc.RoleId
  inner join @RoleNames rn
	on r.Name = rn.Value
end
