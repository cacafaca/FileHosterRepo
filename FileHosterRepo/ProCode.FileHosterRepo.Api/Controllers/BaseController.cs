using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;

namespace ProCode.FileHosterRepo.Api.Controllers
{
    public abstract class BaseController : Controller
    {
        #region Fields
        protected readonly Dal.DataAccess.FileHosterRepoContext context;
        protected readonly IJwtAuthenticationManager authenticationManager;
        protected readonly Token token;
        #endregion

        #region Constructor
        public BaseController(Dal.DataAccess.FileHosterRepoContext context, IJwtAuthenticationManager authenticationManager)
        {
            this.context = context;
            this.authenticationManager = authenticationManager;
            this.token = new Token(authenticationManager);
        }
        #endregion

        #region Methods
        protected async Task<Dal.Model.User> GetLoggedUserAsync()
        {
            if (User == null)
                throw new ArgumentNullException("User is not logged.");

            return await context.Users.SingleOrDefaultAsync(user =>
                user.UserId == User.GetUserId() &&
                user.Logged == true &&
                user.Role == User.GetRole()
                );
        }

        protected ActionResult GetUnauthorizedLoginResponse()
        {
            return Unauthorized($"{User.GetRole()} {User.GetEmail()} not logged in.");
        }

        protected static string GetPasswordHash(string password)
        {
            using var sha1 = new SHA1Managed();
            var hash = Encoding.UTF8.GetBytes(password);
            var generatedHash = sha1.ComputeHash(hash);
            var generatedHashString = Convert.ToBase64String(generatedHash);
            return generatedHashString;
        }

        protected static string EncryptPassword(string password)
        {
            byte[] data = Encoding.ASCII.GetBytes(password);
            data = new SHA256Managed().ComputeHash(data);
            String hash;
            hash = Convert.ToBase64String(data);
            //hash = Encoding.ASCII.GetString(data);
            return hash;
        }
        #endregion
    }
}