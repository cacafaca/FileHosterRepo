using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProCode.FileHosterRepo.Api.Models;

namespace ProCode.FileHosterRepo.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : Controller
    {
        private readonly Dal.DataAccess.FileHosterContext _context;

        public UsersController(Dal.DataAccess.FileHosterContext context)
        {
            _context = context;
        }

        [HttpPost("Login")]
        public async Task<ActionResult<UserWithToken>> Login([FromForm] Dal.Model.User user)
        {
            user = await _context.Users.Where(u => u.Email == user.Email && u.Password == user.Password).FirstOrDefaultAsync();
            if (user != null)
            {
                return new UserWithToken(user);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost("Register")]
        public async Task<ActionResult<UserWithToken>> Register([FromForm] Dal.Model.User newUser)
        {
            // Check if user exists.
            var existingUser = await _context.Users.Where(u => u.Email == newUser.Email).FirstOrDefaultAsync();
            if(existingUser==null)
            {
                newUser.Id = int.MinValue;
                newUser.Created = DateTime.Now;
                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();
                return new UserWithToken(newUser);
            }
            else
            {
                return Conflict();
            }
        }
    }

}
