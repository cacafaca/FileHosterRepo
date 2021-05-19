using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace ProCode.FileHosterRepo.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")] // Adds "/Users" to link. Instead "/Login" we get "/Users/Login".
    public class AdminController : BaseController
    {
        #region Constants
        const string claimTypeNameUserId = "userId";
        const string adminNickname = "Admin";
        #endregion

        #region Constructor
        public AdminController(Dal.DataAccess.FileHosterRepoContext context, IJwtAuthenticationManager authenticationManager)
            : base(context, authenticationManager) { }
        #endregion

        #region Actions
        [AllowAnonymous]
        [HttpPost("Register")]
        public async Task<ActionResult<string>> Register(Model.Request.UserRegister newUser)
        {
            //Validate new user data.
            if (newUser == null)
                return BadRequest("Please send data for a new user.");
            if (string.IsNullOrWhiteSpace(newUser.Email))
                return BadRequest("Please send email.");
            if (string.IsNullOrWhiteSpace(newUser.Password))
                return BadRequest("Please send password.");

            // Check if administrator exists. At this point it can be only one administrator.
            var adminsFound = await context.Users.Where(u => u.Role == Dal.Model.UserRole.Admin).ToListAsync();

            if (adminsFound.Count == 0)
            {
                var newAdminDb = new Dal.Model.User()
                {
                    Email = newUser.Email,
                    Password = EncryptPassword(newUser.Password),
                    Nickname = adminNickname,   // Administrator has fixed nickname.
                    Created = DateTime.Now,
                    Role = Dal.Model.UserRole.Admin,
                    Logged = true
                };
                context.Users.Add(newAdminDb);
                await context.SaveChangesAsync();
                return Ok(token.Generate(newAdminDb.UserId, newAdminDb.Email, Dal.Model.UserRole.Admin));
            }
            else
            {
                return Conflict($"There is already administrator with email {adminsFound.FirstOrDefault()?.Email}.");
            }
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<ActionResult<string>> Login(Model.Request.User loginUser)
        {
            var usersFound = await context.Users.Where(u => u.Email == loginUser.Email && u.Role == Dal.Model.UserRole.Admin).ToListAsync();
            switch (usersFound.Count)
            {
                case 0:
                    return NotFound($"Can't find user with email: {loginUser.Email}.");
                case 1:
                    if (usersFound.First().Password == EncryptPassword(loginUser.Password))
                    {
                        usersFound.First().Logged = true;
                        await context.SaveChangesAsync();
                        return Ok(token.Generate(usersFound.First().UserId, usersFound.First().Email, usersFound.First().Role));
                    }
                    else
                    {
                        return Unauthorized("Wrong email and password.");
                    }
                default:
                    return Conflict($"Multiple accounts error for email {loginUser.Email}. Please report this.");
            }
        }

        [HttpGet("Logout")]
        public async Task<ActionResult<string>> Logout()
        {
            // Always check at beginning!
            var loggedAdmin = await GetLoggedUserAsync();
            if (loggedAdmin != null)
            {
                loggedAdmin.Logged = false; // loggedAdmin is impossible to be null.
                await context.SaveChangesAsync();
                return Ok($"Administrator {User.Claims.Where(c => c.Type == ClaimTypes.Email).FirstOrDefault()?.Value} logged out.");
            }
            else
            {
                return GetUnauthorizedLoginResponse();
            }
        }

        [HttpGet("Info")]
        public async Task<ActionResult<Model.Response.User>> Info()
        {
            // Always check at beginning!
            var loggedAdmin = await GetLoggedUserAsync();
            if (loggedAdmin != null)
            {
                return new Model.Response.User()
                {
                    UserId = loggedAdmin.UserId,
                    Nickname = loggedAdmin.Nickname,
                    Created = loggedAdmin.Created,
                    Role = loggedAdmin.Role
                };
            }
            else
            {
                return GetUnauthorizedLoginResponse();
            }
        }

        [HttpPatch("Update")]
        public async Task<ActionResult<bool>> Update(Model.Request.UserRegister updateUser)
        {
            // Validate password.
            if (string.IsNullOrWhiteSpace(updateUser.Password))
                return Conflict("Password can not be empty.");

            // Always check at beginning!
            var loggedAdmin = await GetLoggedUserAsync();
            if (loggedAdmin != null)
            {
                var newPassword = EncryptPassword(updateUser.Password);
                if (loggedAdmin.Password != newPassword)
                {
                    loggedAdmin.Password = newPassword;
                    await context.SaveChangesAsync();
                    return Ok(true);
                }
                else
                {
                    return Ok(false);   // Return false, because there is no point updating same value.
                }
            }
            else
            {
                return GetUnauthorizedLoginResponse();
            }
        }
        #endregion

        #region Methods
        #endregion
    }
}