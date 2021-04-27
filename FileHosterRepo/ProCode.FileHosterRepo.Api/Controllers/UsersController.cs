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
    public class UsersController : Controller
    {
        #region Constants
        const string claimTypeNameUserId = "userId";
        #endregion

        #region Fields
        private readonly Dal.DataAccess.FileHosterContext context;
        private readonly IJwtAuthenticationManager authenticationManager;
        #endregion

        #region Constructor
        public UsersController(Dal.DataAccess.FileHosterContext context, IJwtAuthenticationManager authenticationManager)
        {
            this.context = context;
            this.authenticationManager = authenticationManager;
        }
        #endregion

        #region Actions
        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<ActionResult<string>> Login([FromForm] Model.Request.User loginUser)
        {
            var usersFound = await context.Users.Where(u => u.Email == loginUser.Email).ToListAsync();
            switch (usersFound.Count)
            {
                case 0:
                    return NotFound($"Can't find user with email: {loginUser.Email}.");
                case 1:
                    if (usersFound[0].Password == EncryptPassword(loginUser.Password))
                    {

                        return GenerateToken(usersFound[0].Id, usersFound[0].Email);
                    }
                    else
                    {
                        return Unauthorized();
                    }
                default:
                    return Conflict($"Multiple accounts error for email {loginUser.Email}. Please report this.");
            }
        }

        [AllowAnonymous]
        [HttpPost("Register")]
        public async Task<ActionResult<string>> Register([FromForm] Model.Request.UserRegister newUser)
        {
            // Check if user exists.
            var usersFound = await context.Users.Where(u => u.Email == newUser.Email || u.Nickname == newUser.Nickname).ToListAsync();

            if (usersFound.Count == 0)
            {
                var allUsers = await context.Users.ToListAsync();
                int maxId;
                if (allUsers.Count > 0)
                    maxId = allUsers.Max(u => u.Id) + 1;
                else
                    maxId = 1;
                var newUserDb = new Dal.Model.User()
                {
                    Id = maxId,
                    Email = newUser.Email,
                    Password = EncryptPassword(newUser.Password),
                    Nickname = newUser.Nickname,
                    Created = DateTime.Now
                };
                context.Users.Add(newUserDb);
                await context.SaveChangesAsync();
                return GenerateToken(newUserDb.Id, newUserDb.Email);
            }
            else
            {
                return Conflict($"There is already user with email {newUser.Email}, or with nickname {newUser.Nickname}.");
            }
        }

        [HttpGet("Info")]
        public async Task<ActionResult<Model.Response.User>> Info(int? userId)
        {
            int userIdSearch = userId == null ? GetLoggedUserId() : (int)userId;

            var userFound = await context.Users.Where(u => u.Id == userIdSearch).FirstOrDefaultAsync();
            if (userFound != null)
            {
                return new Model.Response.User()
                {
                    Id = userFound.Id,
                    Nickname = userFound.Nickname,
                    Created = userFound.Created
                };
            }
            else
            {
                return NotFound($"User id '{userIdSearch}' not found.");
            }
        }

        [HttpDelete("Delete")]
        public async Task<ActionResult> Delete()
        {
            if (User != null)
            {
                var claim = User.Claims.Where(c => c.Type == claimTypeNameUserId).FirstOrDefault();
                if (claim != null)
                {
                    var userFound = await context.Users.Where(u => u.Id.ToString() == claim.Value).FirstOrDefaultAsync();
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
                else
                {
                    return Conflict("Claim not found");
                }
            }
            else
            {
                return Conflict("HTTP context not found.");
            }
        }

        [HttpGet("Logout")]
        public ActionResult<string> Logout()
        {
            return ExpireToken();
        }

        [HttpPatch("Update")]
        public async Task<ActionResult> Update([FromForm] Model.Request.UserRegister updateUser)
        {
            if (string.IsNullOrWhiteSpace(updateUser.Nickname))
                return Conflict("Nickname is empty.");

            var loggedUserId = GetLoggedUserId();
            var userFound = await context.Users.Where(user => user.Id == loggedUserId).FirstOrDefaultAsync();
            if (userFound != null)
            {
                userFound.Nickname = updateUser.Nickname;

                if (!string.IsNullOrWhiteSpace(updateUser.Password))
                    userFound.Password = EncryptPassword(updateUser.Password);

                await context.SaveChangesAsync();
                return Ok("Updated.");
            }
            else
            {
                return NotFound($"Can't find logged user id {loggedUserId}.");
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

        private string ExpireToken()
        {
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(User.Claims),
                Expires = DateTime.UtcNow.AddYears(-1),
                NotBefore = DateTime.MinValue,
                SigningCredentials = new SigningCredentials(
                    authenticationManager.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256)
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
        }

        private int GetLoggedUserId()
        {
            var claim = User.Claims.Where(c => c.Type == claimTypeNameUserId).FirstOrDefault();
            if (claim != null)
            {
                int userId;
                if (int.TryParse(claim.Value, out userId))
                {
                    return userId;
                }
                else
                {
                    throw new Exception("Claim not valid.");
                }
            }
            else
            {
                throw new Exception("Claim not found.");
            }
        }
    }
    #endregion
}