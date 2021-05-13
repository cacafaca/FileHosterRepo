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
    [Route("[controller]")] // "[controller]" adds "/Users" to link. Instead "/Login" we get "/Users/Login".
    public class UsersController : Controller
    {
        #region Constants
        #endregion

        #region Fields
        private readonly Dal.DataAccess.FileHosterRepoContext context;
        private readonly IJwtAuthenticationManager authenticationManager;
        #endregion

        #region Constructor
        public UsersController(Dal.DataAccess.FileHosterRepoContext context, IJwtAuthenticationManager authenticationManager)
        {
            this.context = context;
            this.authenticationManager = authenticationManager;
        }
        #endregion

        #region Actions
        [AllowAnonymous]
        [HttpPost("Register")]
        public async Task<ActionResult<string>> Register([FromForm] Model.Request.UserRegister newUser)
        {
            // Check if there is an administrator first. Can't allow user to register before Administrator.
            var adminUser = await context.Users.Where(u => u.Role == Dal.Model.UserRole.Admin).ToListAsync();
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
                    Role = Dal.Model.UserRole.PlainUser,
                    Logged = true
                };
                context.Users.Add(newUserDb);
                await context.SaveChangesAsync();
                return Ok(GenerateToken(newUserDb.Id, newUserDb.Email));
            }
            else
            {
                return Conflict($"There is already user with email {newUser.Email}, or with nickname {newUser.Nickname}.");
            }
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<ActionResult<string>> Login([FromForm] Model.Request.User loginUser)
        {
            var usersFound = await context.Users.Where(u => u.Email == loginUser.Email && u.Role != Dal.Model.UserRole.Admin).ToListAsync();
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
                        return Unauthorized();
                    }
                default:
                    var admin = await context.Users.SingleOrDefaultAsync(u => u.Role == Dal.Model.UserRole.Admin);
                    return Conflict($"Multiple accounts error for email {loginUser.Email}. Please report this to {admin?.Email}.");
            }
        }

        [HttpGet("Logout")]
        public async Task<ActionResult<string>> Logout()
        {
            var loggedUser = await context.Users.SingleOrDefaultAsync(u => u.Id == User.GetUserId());
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

        [HttpGet("Info")]
        public async Task<ActionResult<Model.Response.User>> Info(int? userId)
        {
            int userIdSearch = userId == null ? User.GetUserId() : (int)userId; // If userId is null, means get it for its self.

            var userFound = await context.Users.Where(u => u.Id == userIdSearch).FirstOrDefaultAsync();
            if (userFound != null)
            {
                return new Model.Response.User()
                {
                    Id = userFound.Id,
                    Nickname = userFound.Nickname,
                    Created = userFound.Created,
                    Role = userFound.Role
                };
            }
            else
            {
                return NotFound($"User id '{userIdSearch}' not found.");
            }
        }

        [HttpPatch("Update")]
        public async Task<ActionResult> Update([FromForm] Model.Request.UserRegister updateUser)
        {
            if (string.IsNullOrWhiteSpace(updateUser.Nickname))
                return Conflict("Nickname is empty.");

            var loggedUser = await GetLoggedUserAsync();
            if (loggedUser != null)
            {
                loggedUser.Nickname = updateUser.Nickname;

                if (!string.IsNullOrWhiteSpace(updateUser.Password))
                    loggedUser.Password = EncryptPassword(updateUser.Password);

                await context.SaveChangesAsync();
                return Ok("Updated.");
            }
            else
            {
                return NotFound($"Can't find logged user id {User.GetUserId()}.");
            }
        }

        [HttpDelete("Delete")]
        public async Task<ActionResult> Delete()
        {
            var userFound = await context.Users.SingleOrDefaultAsync(u => u.Id == User.GetUserId());
            if (userFound != null)
            {
                try
                {
                    context.Users.Remove(userFound);
                    await context.SaveChangesAsync();
                    return Ok($"User {userFound} deleted.");
                }
                catch (Exception ex)
                {
                    return Conflict(ex);
                }
            }
            else
            {
                return Conflict("User not found.");
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
                    new Claim(LoggedUser.ClaimTypeNameUserId, userId.ToString()),
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

        private async Task<Dal.Model.User> GetLoggedUserAsync()
        {
            if (User == null)
                throw new ArgumentNullException("User not logged.");
            
            return await context.Users.SingleOrDefaultAsync(user => user.Id == User.GetUserId()); ;
        }
    }
    #endregion
}