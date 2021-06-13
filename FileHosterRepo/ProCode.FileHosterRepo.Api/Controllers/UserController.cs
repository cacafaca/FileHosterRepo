using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace ProCode.FileHosterRepo.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route(Common.Routes.User.ControlerName)] 
    public class UserController : BaseController
    {
        #region Constructor
        public UserController(Dal.DataAccess.FileHosterRepoContext context, IJwtAuthenticationManager authenticationManager)
            : base(context, authenticationManager) { }
        #endregion

        #region Actions
        [AllowAnonymous]
        [HttpPost]
        [Route(Common.Routes.User.Register)]
        public async Task<ActionResult<string>> Register(Common.Api.Request.UserRegister newUser)
        {
            // Check if there is an administrator first. Can't allow user to register before Administrator.
            var adminUser = await context.Users.Where(u => u.Role == Common.User.UserRole.Admin).ToListAsync();
            if (adminUser.Count == 0)
                return Conflict("There is no administrator. System needs administrator in order to work.");

            // Check if user exists.
            var usersFound = await context.Users.Where(u => u.Email == newUser.Email || u.Nickname == newUser.Nickname).ToListAsync();

            if (usersFound.Count == 0)
            {
                var newUserDb = new Dal.Model.User()
                {
                    Email = newUser.Email,
                    Password = EncryptPassword(newUser.Password),
                    Nickname = newUser.Nickname,
                    Created = DateTime.Now,
                    Role = Common.User.UserRole.User,
                    Logged = true
                };
                context.Users.Add(newUserDb);
                await context.SaveChangesAsync();
                return Ok(token.Generate(newUserDb.UserId, newUserDb.Email, Common.User.UserRole.User));
            }
            else
            {
                return Conflict($"There is already user with email {newUser.Email}, or with nickname {newUser.Nickname}.");
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [Route(Common.Routes.User.Login)]
        public async Task<ActionResult<string>> Login(Common.Api.Request.User loginUser)
        {
            var usersFound = await context.Users.Where(u => u.Email == loginUser.Email).ToListAsync();
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
                        return Unauthorized();
                    }
                default:
                    var admin = await context.Users.SingleOrDefaultAsync(u => u.Role == Common.User.UserRole.Admin);
                    return Conflict($"Multiple accounts error for email {loginUser.Email}. Please report this to {admin?.Email}.");
            }
        }

        [HttpGet]
        [Route(Common.Routes.User.Logout)]
        public async Task<ActionResult<string>> Logout()
        {
            var loggedUser = await context.Users.SingleOrDefaultAsync(u => u.UserId == User.GetUserId());
            if (loggedUser != null)
            {
                loggedUser.Logged = false;
                await context.SaveChangesAsync();
                return Ok($"User {User.Claims.Where(c => c.Type == ClaimTypes.Email).FirstOrDefault()?.Value} logged out.");
            }
            else
            {
                return Conflict("User is not logged in.");
            }
        }

        [HttpGet]
        [Route(Common.Routes.User.Info)]
        public async Task<ActionResult<Common.Api.Response.User>> Info(int? userId)
        {
            // Always check at beginning!
            var loggedUser = await GetLoggedUserAsync();
            if (loggedUser != null)
            {
                var user = userId == null ? loggedUser : await context.Users.SingleOrDefaultAsync(u => u.UserId == (int)userId);
                if (user != null)
                {
                    return new Common.Api.Response.User()
                    {
                        UserId = user.UserId,
                        Nickname = user.Nickname,
                        Created = user.Created,
                        Role = user.Role
                    };
                }
                else
                {
                    return NotFound($"User id '{userId}' not found.");
                }
            }
            else
            {
                return GetUnauthorizedLoginResponse();
            }
        }

        [HttpPatch]
        [Route(Common.Routes.User.Update)]
        public async Task<ActionResult<bool>> Update(Common.Api.Request.UserRegister updateUser)
        {
            // Always check at beginning!
            var loggedUser = await GetLoggedUserAsync();
            if (loggedUser != null)
            {
                // Validate nickname.
                if (string.IsNullOrWhiteSpace(updateUser.Nickname))
                    return Conflict("Nickname is empty.");
                // Validate password.
                if (string.IsNullOrWhiteSpace(updateUser.Password))
                    return Conflict("Password can not be empty.");

                var newPassword = EncryptPassword(updateUser.Password);
                if (loggedUser.Nickname != updateUser.Nickname || loggedUser.Password != newPassword)
                {
                    loggedUser.Nickname = updateUser.Nickname;
                    loggedUser.Password = newPassword;
                    await context.SaveChangesAsync();
                    return Ok(true);
                }
                else
                {
                    return Ok(false);
                }
            }
            else
            {
                return GetUnauthorizedLoginResponse();
            }
        }

        [HttpDelete()]
        [Route(Common.Routes.User.Delete)]
        public async Task<ActionResult> Delete()
        {
            // Always check at beginning!
            var loggedUser = await GetLoggedUserAsync();
            if (loggedUser != null)
            {
                context.Users.Remove(loggedUser);
                await context.SaveChangesAsync();
                return Ok($"User {loggedUser} deleted.");
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