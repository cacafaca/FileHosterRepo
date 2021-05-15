using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProCode.FileHosterRepo.Api.Model;
using System.Security.Cryptography;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
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
        public async Task<ActionResult> Register(Model.Request.UserRegister newUser)
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
                return Ok(GenerateToken(newAdminDb.Id, newAdminDb.Email));
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
                        return Ok(GenerateToken(usersFound.First().Id, usersFound.First().Email));
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
            Dal.Model.User loggedAdmin;

            // Always check at beginning!
            if (!await IsAdminLoggedAsync(loggedAdmin = await GetLoggedAdminAsync())) return GetUnauthorizedLoginResponse();

            loggedAdmin.Logged = false; // loggedAdmin is impossible to be null.
            await context.SaveChangesAsync();
            return Ok($"Administrator {User.Claims.Where(c => c.Type == ClaimTypes.Email).FirstOrDefault()?.Value} logged out.");
            //return Ok(GenerateToken(0, string.Empty));
        }

        [HttpGet("Info")]
        public async Task<ActionResult<Model.Response.User>> Info(int? userId)
        {
            // Always check at beginning!
            if (!await IsAdminLoggedAsync()) return GetUnauthorizedLoginResponse();

            int userIdSearch = userId == null ? User.GetUserId() : (int)userId;

            var admin = await context.Users.SingleOrDefaultAsync(u => u.Id == userIdSearch && u.Role == Dal.Model.UserRole.Admin);
            if (admin != null)
            {
                return new Model.Response.User()
                {
                    Id = admin.Id,
                    Nickname = admin.Nickname,
                    Created = admin.Created,
                    Role = admin.Role
                };
            }
            else
            {
                return NotFound($"Administrator id '{userIdSearch}' not found.");
            }
        }

        [HttpPatch("Update")]
        public async Task<ActionResult<bool>> Update(Model.Request.UserRegister updateUser)
        {
            // Validate password.
            if (string.IsNullOrWhiteSpace(updateUser.Password))
                return Conflict("Password can not be empty.");

            Dal.Model.User loggedAdmin;

            // Always check at beginning!
            if (!await IsAdminLoggedAsync(loggedAdmin = await GetLoggedAdminAsync())) return GetUnauthorizedLoginResponse();

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
        #endregion

        #region Methods
        private static string GetPasswordHash(string password)
        {
            using var sha1 = new SHA1Managed();
            var hash = Encoding.UTF8.GetBytes(password);
            var generatedHash = sha1.ComputeHash(hash);
            var generatedHashString = Convert.ToBase64String(generatedHash);
            return generatedHashString;
        }
        private static string EncryptPassword(string password)
        {
            byte[] data = Encoding.ASCII.GetBytes(password);
            data = new SHA256Managed().ComputeHash(data);
            String hash;
            hash = Convert.ToBase64String(data);
            //hash = Encoding.ASCII.GetString(data);
            return hash;
        }

        private string GenerateToken(int userId, string email)
        {
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(claimTypeNameUserId, userId.ToString()),
                    new Claim(ClaimTypes.Email, email)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                NotBefore = DateTime.UtcNow,
                SigningCredentials = new SigningCredentials(
                    authenticationManager.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256)
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
        }

        private async Task<Dal.Model.User> GetLoggedAdminAsync()
        {
            if (User == null)
                throw new ArgumentNullException("Administrator is not logged.");

            return await context.Users.SingleOrDefaultAsync(user =>
                user.Id == User.GetUserId() &&
                user.Role == Dal.Model.UserRole.Admin &&
                user.Logged == true
                );
        }

        private async Task<bool> IsAdminLoggedAsync(Dal.Model.User loggedAdminAlreadyRead = null)
        {
            var loggedAdmin = loggedAdminAlreadyRead ?? await GetLoggedAdminAsync();
            return loggedAdmin != null && loggedAdmin.Logged;
        }

        private ActionResult GetUnauthorizedLoginResponse()
        {
            return Unauthorized($"Admin {User.GetEmail()} not logged in.");
        }
        #endregion
    }
}