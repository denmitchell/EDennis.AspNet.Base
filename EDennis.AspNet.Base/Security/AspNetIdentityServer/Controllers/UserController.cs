using EDennis.AspNet.Base;
using EDennis.AspNet.Base.EntityFramework.Entity;
using EDennis.AspNet.Base.Middleware;
using EDennis.AspNet.Base.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EDennis.AspNetBase.Security {

    [Authorize(Policy = "AdministerIDP")]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase {

        private readonly UserManager<DomainUser> _userManager;
        private readonly DomainIdentityDbContext _dbContext;

        public UserController(UserManager<DomainUser> userManager,
            DomainIdentityDbContext dbContext) {
            _userManager = userManager;
            _dbContext = dbContext;
        }

        public static Regex idPattern = new Regex("[0-9a-f]{8}-([0-9a-f]{4}-){3}[0-9a-f]{12}");
        public static Regex lpPattern = new Regex("(?:\\(\\s*loginProvider\\s*=\\s*'?)([^,']+)(?:'?\\s*,\\s*providerKey\\s*=\\s*'?)([^)']+)(?:'?\\s*\\))");


        [HttpGet]
        public async Task<IActionResult> GetAsync([FromQuery] string appName = null, [FromQuery] string orgName = null,
            [FromQuery] int? pageNumber = 1, [FromQuery] int? pageSize = 100) {


            var skip = (pageNumber ?? 1 - 1) * pageSize ?? 100;
            var take = pageSize ?? 100;

            string sql = "";

            var parameters = new Dictionary<string, object> {
                {"@skip", skip },
                {"@take", take }
            };

            if (appName == null && orgName == null) {
                sql = QRY_SQL_USER;
            } else if (appName != null && orgName != null) {
                sql = QRY_SQL_USER_ORG_APP;
                parameters.Add("@appName", appName);
                parameters.Add("@orgName", orgName);
            } else if (orgName != null) {
                sql = QRY_SQL_USER_ORG;
                parameters.Add("@orgName", orgName);
            } else if (appName != null) {
                sql = QRY_SQL_USER_APP;
                parameters.Add("@appName", appName);
            }

            var json = await _dbContext.GetFromJsonSqlAsync(sql, parameters);
            var users = JsonSerializer.Deserialize<List<UserEditModel>>(json);

            return Ok(users);

        }


        [HttpGet("{pathParameter}")]
        public async Task<IActionResult> GetAsync([FromRoute] string pathParameter) {
            var user = await FindAsync(pathParameter);
            if (user == null)
                return NotFound();
            else {
                var currentRoles = await _userManager.GetRolesAsync(user);
                var currentClaims = await _userManager.GetClaimsAsync(user);
                var userEditModel = new UserEditModel {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    Roles = currentRoles,
                    Claims = currentClaims
                };

                return Ok(userEditModel);

            }
        }


        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] UserEditModel userEditModel) {

            var existingUser = _dbContext.Set<DomainUser>().FirstOrDefault(u => u.UserName == userEditModel.Name);

            if (existingUser != null) {
                ModelState.AddModelError("Name", $"A user with Name/UserName='{userEditModel.Name}' already exists.");
            }

            DomainOrganization existingOrganization;

            if (userEditModel.Organization != null) {
                existingOrganization = _dbContext.Set<DomainOrganization>().FirstOrDefault(o => o.Name == userEditModel.Organization);

                if (existingOrganization == null) {
                    ModelState.AddModelError("Organization", $"An organization with Name ='{userEditModel.Organization}' does not exists.");
                }
            }

            IEnumerable<DomainRole> existingRoles;
            List<DomainUserRole> userRoles;

            if (userEditModel.Roles != null && userEditModel.Roles.Count() > 0) {
                existingRoles = _dbContext.Set<DomainRole>().Where(o => userEditModel.Roles.Contains(o.Name));

                foreach(var role in userEditModel.Roles.Except(existingRoles.Select(r => r.Name))) { 
                    ModelState.AddModelError("Roles", $"A role with Name ='{role}' does not exists.");
                }

                if (ModelState.ErrorCount == 0) {
                    userRoles = new List<DomainUserRole>();
                    foreach (var role in existingRoles) {
                        userRoles.Add(new DomainUserRole {
                            Role = role,
                            RoleId = role.Id,
                            User = existingUser,
                            UserId = existingUser.Id,
                            SysStatus = SysStatus.Normal
                        });
                    }
                }

            }


            if (ModelState.ErrorCount > 0)
                return Conflict(ModelState);


            var user = new DomainUser {
                Id = Guid.NewGuid(),
                Email = userEditModel.Email,
                NormalizedEmail = userEditModel.Email.ToUpper(),
                UserName = userEditModel.Name,
                NormalizedUserName = userEditModel.Name.ToUpper()
            };
            if (userEditModel.Password.Length == 256 || userEditModel.Password.Length == 512)
                user.PasswordHash = userEditModel.Password;
            else
                user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, userEditModel.Password);


            var results = new List<IdentityResult> {
                await _userManager.CreateAsync(user)
            };


            if (userEditModel.Roles != null && userEditModel.Roles.Count() > 0)
                results.Add(await _userManager.AddToRolesAsync(user, userEditModel.Roles));

            if (userEditModel.Claims != null && userEditModel.Claims.Count() > 0)
                results.Add(await _userManager.AddClaimsAsync(user, userEditModel.Claims.ToClaimEnumerable()));

            var failures = results.SelectMany(r => r.Errors);

            if (failures.Count() > 0)
                return Conflict(failures);
            else
                return Ok(userEditModel);

        }


        [HttpPatch("{pathParameter}")]
        public async Task<IActionResult> PatchAsync([FromBody] JsonElement jsonElement, [FromRoute] string pathParameter) {

            var user = await FindAsync(pathParameter);
            if (user == null)
                return NotFound();

            bool securityUpdated = false;

            if (jsonElement.TryGetString("Email", out string email)) {
                user.Email = email;
                user.NormalizedEmail = email.ToUpper();
            }

            if (jsonElement.TryGetString("PhoneNumber", out string phoneNumber)) {
                user.PhoneNumber = phoneNumber;
            }

            if (jsonElement.TryGetString("UserName", out string userName)) {
                user.UserName = userName;
                user.NormalizedUserName = userName.ToUpper();
                securityUpdated = true;
            }

            if (jsonElement.TryGetString("Password", out string password)) {
                user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, password);
                securityUpdated = true;
            }

            if (jsonElement.TryGetBoolean("EmailConfirmed", out bool emailConfirmed))
                user.EmailConfirmed = emailConfirmed;
            if (jsonElement.TryGetBoolean("PhoneNumberConfirmed", out bool phoneNumberConfirmed))
                user.PhoneNumberConfirmed = phoneNumberConfirmed;

            if (jsonElement.TryGetBoolean("TwoFactorEnabled", out bool twoFactorEnabled))
                user.TwoFactorEnabled = twoFactorEnabled;
            if (jsonElement.TryGetBoolean("LockoutEnabled", out bool lockoutEnabled))
                user.LockoutEnabled = lockoutEnabled;
            if (jsonElement.TryGetDateTimeOffset("LockoutEnd", out DateTimeOffset? lockoutEnd))
                user.LockoutEnd = lockoutEnd;
            if (jsonElement.TryGetInt32("AccessFailedCount", out int accessFailedCount))
                user.AccessFailedCount = accessFailedCount;



            user.ConcurrencyStamp = Guid.NewGuid().ToString();
            if (securityUpdated)
                user.SecurityStamp = Guid.NewGuid().ToString();

            var results = new List<IdentityResult> {
                await _userManager.UpdateAsync(user)
            };


            var hasRoles = jsonElement.TryGetProperty("Roles", out JsonElement _);
            var hasClaims = jsonElement.TryGetProperty("Claims", out JsonElement _);

            var userEditModel = JsonSerializer.Deserialize<UserEditModel>(jsonElement.GetRawText());

            results.AddRange(await UpdateRolesAndClaimsAsync(user, userEditModel, hasRoles, hasClaims));

            var failures = results.SelectMany(r=>r.Errors);

            if (failures.Count() > 0)
                return BadRequest(failures);
            else
                return NoContent();
        }


        [HttpDelete("{pathParameter}")]
        public async Task<IActionResult> Delete([FromRoute] string pathParameter) {
            var user = await FindAsync(pathParameter);
            if (user == null)
                return NotFound();

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                return Conflict(result.Errors);
            else
                return NoContent();
        }


        private async Task<IEnumerable<IdentityResult>> UpdateRolesAndClaimsAsync(DomainUser user,
            UserEditModel userEditModel, bool hasRoles, bool hasClaims) {

            List<IdentityResult> results = new List<IdentityResult>();


            if (hasRoles) {
                var currentRoles = await _userManager.GetRolesAsync(user);
                results.Add(await _userManager.RemoveFromRolesAsync(user, currentRoles.Except(userEditModel.Roles)));
                results.Add(await _userManager.AddToRolesAsync(user, userEditModel.Roles.Except(currentRoles)));
            }

            if (hasClaims) {
                var currentClaims = await _userManager.GetClaimsAsync(user);
                results.Add(await _userManager.RemoveClaimsAsync(user, currentClaims.Except(userEditModel.Claims.ToClaimEnumerable())));
                results.Add(await _userManager.AddClaimsAsync(user, userEditModel.Claims.ToClaimEnumerable().Except(currentClaims)));
            }

            return results;

        }


        private async Task<DomainUser> FindAsync(string pathParameter) {
            if (pathParameter.Contains("@"))
                return await _userManager.FindByEmailAsync(pathParameter);
            else if (idPattern.IsMatch(pathParameter))
                return await _userManager.FindByIdAsync(pathParameter);
            else
                return await _userManager.FindByNameAsync(pathParameter);
        }


        private const string QRY_SQL_USER = @"
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
";

        private const string QRY_SQL_USER_ORG = @"
WITH u as (
  SELECT u.*, o.Name OrganizationName
    FROM AspNetUsers u 
    LEFT OUTER JOIN AspNetOrganizations o
        on o.Id = u.OrganizationId
    WHERE 
      EXISTS (
        SELECT 0 
          FROM AspNetUserClaims uc
          WHERE uc.ClaimValue = @orgName
            AND uc.ClaimType = 'Organization'
            AND u.Id = uc.UserId
      )
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
";

        private const string QRY_SQL_USER_APP = @"
WITH u as (
  SELECT u.*, o.Name OrganizationName
    FROM AspNetUsers u 
    LEFT OUTER JOIN AspNetOrganizations o
        on o.Id = u.OrganizationId
    WHERE 
      EXISTS (
        SELECT 0 
          FROM AspNetRoles r
          INNER JOIN AspNetUserRoles ur
            ON r.Id = ur.RoleId
          WHERE r.Name Like @appName + '%'
            AND u.Id = ur.UserId
      )
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
			and r.Name Like @appName + '%'
		FOR XML PATH('')),1,1,'') + ']' ) as Roles,
	(
		SELECT uc.ClaimType, uc.ClaimValue
		FROM AspNetUserClaims uc
		WHERE Users.Id = uc.UserId
		FOR JSON PATH
	) as Claims
	FROM u Users 
	FOR JSON PATH
";

        private const string QRY_SQL_USER_ORG_APP = @"
WITH u as (
  SELECT u.*, o.Name OrganizationName
    FROM AspNetUsers u 
    LEFT OUTER JOIN AspNetOrganizations o
        on o.Id = u.OrganizationId
    WHERE 
      EXISTS (
        SELECT 0 
          FROM AspNetUserClaims uc
          WHERE uc.ClaimValue = @orgName
            AND uc.ClaimType = 'Organization'
            AND u.Id = uc.UserId
      )
      AND
      EXISTS (
        SELECT 0 
          FROM AspNetRoles r
          INNER JOIN AspNetUserRoles ur
            ON r.Id = ur.RoleId
          WHERE r.Name Like @appName + '%'
            AND u.Id = ur.UserId
      )
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
            AND r.Name Like @appName + '%'
		FOR XML PATH('')),1,1,'') + ']' ) as Roles,
	(
		SELECT uc.ClaimType, uc.ClaimValue
		FROM AspNetUserClaims uc
		WHERE Users.Id = uc.UserId
		FOR JSON PATH
	) as Claims
	FROM u Users 
	FOR JSON PATH
";

    }
}
