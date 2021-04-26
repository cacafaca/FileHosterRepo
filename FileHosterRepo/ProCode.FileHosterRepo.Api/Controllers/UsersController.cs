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
                    return NotFound();
                case 1:
                    if (usersFound[0].Password == EncryptPassword(loginUser.Password))
                    {
                        var tokenDescriptor = new SecurityTokenDescriptor
                        {
                            Subject = new ClaimsIdentity(new Claim[]
                            {
                                new Claim(ClaimTypes.Sid, usersFound[0].Id.ToString()),
                                new Claim(ClaimTypes.Email, usersFound[0].Email)
                            }),
                            Expires = DateTime.UtcNow.AddMinutes(1),
                            SigningCredentials = new SigningCredentials(
                                authenticationManager.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256)
                        };
                        var tokenHandler = new JwtSecurityTokenHandler();
                        return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
                    }
                    else
                    {
                        return Unauthorized();
                    }
                default:
                    return Conflict();
            }
        }

        [AllowAnonymous]
        [HttpPost("Register")]
        public async Task<ActionResult<Model.Response.User>> Register([FromForm] Model.Request.UserRegister newUser)
        {
            // Check if user exists.
            var usersFound = await context.Users.Where(u => u.Email == newUser.Email).ToListAsync();
            switch (usersFound.Count)
            {
                case 0:
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
                    return new Model.Response.User()
                    {
                        Id = newUserDb.Id,
                        Nickname = newUserDb.Nickname,
                        Created = newUserDb.Created
                    };
                case 1:
                    return Conflict();
                default:
                    return Conflict();
            }
        }

        [HttpGet("Info")]
        public async Task<ActionResult<Model.Response.User>> Info(int userId)
        {
            var userFound = await context.Users.Where(u => u.Id == userId).FirstOrDefaultAsync();
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
                return Conflict();
            }
        }

        [HttpDelete("Delete")]
        public async Task<ActionResult<Model.Response.User>> Delete()
        {
            if (HttpContext.User != null)
            {
                var claim = HttpContext.User.Claims.Where(c => c.Type == ClaimTypes.Sid).FirstOrDefault();
                if (claim != null)
                {
                    var userFound = await context.Users.Where(u => u.Id.ToString() == claim.Value).FirstOrDefaultAsync();
                    if (userFound != null)
                    {
                        try
                        {
                            context.Users.Remove(userFound);
                            await context.SaveChangesAsync();
                            return new Model.Response.User()
                            {
                                Id = userFound.Id,
                                Nickname = userFound.Nickname,
                                Created = userFound.Created
                            };
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
        public async Task<ActionResult<Model.Response.User>> Logout()
        {
            if (HttpContext.User != null)
            {
                HttpContext.
            }
            else
            {
                return Conflict("HTTP context not found.");
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

        private string GenerateToken(string email)
        {
            var token = new JwtSecurityToken(
                claims: new Claim[]
                {
                    new Claim(ClaimTypes.Email, email)
                },
                notBefore: new DateTimeOffset(DateTime.Now).DateTime,
                expires: new DateTimeOffset(DateTime.Now.AddMinutes(60)).DateTime,
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes("secret_key!replace!")), SecurityAlgorithms.HmacSha256)
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        #endregion
    }

}
