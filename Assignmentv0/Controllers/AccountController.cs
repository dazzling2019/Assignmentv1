using IdentityModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Assignmentv1.Data;
using Assignmentv1.ViewModels;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Assignmentv1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IConfiguration _configuration;

        public AccountController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.signInManager = signInManager;
            _configuration = configuration;
        }

        [HttpGet("List")]
        [Authorize(Roles = Roles.Admin + "," + Roles.Student)]
        public ActionResult<IEnumerable<UserSummaryViewModel>> List()
        {
            var list = userManager
                        .Users
                        .OrderBy(u => u.Email)
                        .Select(u => new UserSummaryViewModel()
                        {
                            Id = u.Id,
                            Email = u.Email,
                            FirstName = u.Firstname,
                            LastName = u.Lastname
                        })
                        .ToList();
            return list;
        }

        [HttpGet("Summary/{id}")]
        [Authorize]
        public async Task<ActionResult<UserSummaryViewModel>> Summary(string id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            // find the user 
            var user = await userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // construct ViewModel and populate with the summary user details
            return new UserSummaryViewModel()
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.Firstname,
                LastName = user.Lastname,
            };
        }

        [HttpGet("Modify/{id}")]
        [Authorize(Roles = Roles.Admin + "," + Roles.Student)]
        public async Task<ActionResult<EditableUserViewModel>> Modify(string id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            // find the user 
            var user = await userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // get list of roles assigned to this user
            var userRoles = await userManager.GetRolesAsync(user);

            // construct ViewModel and populate with the user to be modified and the roles
            return new EditableUserViewModel()
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.Firstname,
                LastName = user.Lastname,
                Admin = userRoles.Contains(Roles.Admin),
                Student = userRoles.Contains(Roles.Student),

            };
        }

        [HttpPost]
        [Route("Register")]
        [Authorize(Roles = Roles.Admin + "," + Roles.Student)]
        public async Task<IActionResult> Register([FromBody] EditableUserViewModel model)
        {
            var userExists = await userManager.FindByNameAsync(model.Email);
            if (userExists != null)
            {
                return Problem("User already exists!", null, StatusCodes.Status500InternalServerError, "Error", nameof(ProblemDetails));
            }

            ApplicationUser user = new ApplicationUser()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Email,
                Firstname = model.FirstName,
                Lastname = model.LastName
            };
            var result = await userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                return Problem("User creation failed! Please check user details and try again.", null, StatusCodes.Status500InternalServerError, "Error", nameof(ProblemDetails));
            }

            // update the roles the stored user is assigned to 
            await AddOrRemoveRoleFromUserAsync(user, Roles.Admin, model.Admin);
            await AddOrRemoveRoleFromUserAsync(user, Roles.Student, model.Student);


            return Ok(new { user.Id });
        }

        [HttpPut("Modify/{id}")]
        [Authorize(Roles = Roles.Admin + "," + Roles.Student)]
        public async Task<IActionResult> Modify(string id, [FromBody] EditableUserViewModel modifiedUser)
        {
            try
            {
                // the id passed and the id in the object should match, if they don't reject the request
                if (id == null || id != modifiedUser.Id)
                {
                    return Problem("Id passed to call and Id in user object do not match!", null, StatusCodes.Status400BadRequest, "Error", nameof(ProblemDetails));
                }

                var user = await userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                // username and email must be the same due to quirks in ASP.NET Identity and the "out of the box" way we're using it
                user.UserName = user.Email = modifiedUser.Email;
                user.Firstname = modifiedUser.FirstName;
                user.Lastname = modifiedUser.LastName;

                // update the roles the stored user is assigned to 
                await AddOrRemoveRoleFromUserAsync(user, Roles.Admin, modifiedUser.Admin);
                await AddOrRemoveRoleFromUserAsync(user, Roles.Student, modifiedUser.Student);

                if (!string.IsNullOrEmpty(modifiedUser.Password))
                {
                    var resetPasswordToken = await userManager.GeneratePasswordResetTokenAsync(user);
                    await userManager.ResetPasswordAsync(user, resetPasswordToken, modifiedUser.Password);
                }

                await userManager.UpdateAsync(user);
            }
            catch (Exception)
            {
                throw;
            }

            return NoContent();
        }

        [HttpPost]
        [Route("Login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginUserViewModel model)
        {
            var user = await userManager.FindByNameAsync(model.Email);
            if (user != null && await userManager.CheckPasswordAsync(user, model.Password))
            {
                var userRoles = await userManager.GetRolesAsync(user);

                var authClaims = new List<Claim>
                {
                    new Claim(JwtClaimTypes.Name, user.UserName),
                    new Claim(JwtClaimTypes.GivenName, user.Firstname),
                    new Claim(JwtClaimTypes.FamilyName, user.Lastname),
                    new Claim(JwtClaimTypes.Email, user.Email),
                    new Claim(JwtClaimTypes.Id, user.Id),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(JwtClaimTypes.Role, userRole));
                }

                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

                var token = new JwtSecurityToken(
                    issuer: _configuration["JWT:ValidIssuer"],
                    audience: _configuration["JWT:ValidAudience"],
                    expires: DateTime.UtcNow.AddHours(24),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                    );

                var tokenViewModel = new TokenViewModel()
                {
                    Data = new JwtSecurityTokenHandler().WriteToken(token),
                    Expiration = token.ValidTo
                };

                return Ok(tokenViewModel);
            }
            return Unauthorized();
        }

        private async Task<bool> AddOrRemoveRoleFromUserAsync(ApplicationUser user, string role, bool addUserToRole)
        {
            // check stated role actually exists
            if (!roleManager.Roles.Select(r => r.Name).Contains(role))
            {
                return false;
            }

            // get list of roles assigned to this user
            var userRoles = await userManager.GetRolesAsync(user);

            // check to see if user already in role
            // you cannot remove a user from a role they're already in
            // and you cannot add them to a role twice
            // both are errors and we need to code to prevent them
            var alreadyInRole = userRoles.Contains(role);

            // now add or remove the role from user as necessary
            bool success = false;
            if (addUserToRole && !alreadyInRole)
            {
                var ir = await userManager.AddToRoleAsync(user, role);
                success = ir.Succeeded;
            }
            else if (!addUserToRole && alreadyInRole)
            {
                var ir = await userManager.RemoveFromRoleAsync(user, role);
                success = ir.Succeeded;
            }
            else
            {
                success = true;
            }

            return success;
        }
    }
}
