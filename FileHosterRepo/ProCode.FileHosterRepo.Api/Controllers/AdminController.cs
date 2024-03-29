﻿using Microsoft.AspNetCore.Mvc;
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
    [Route(Common.ApiRoutes.Admin.ControlerName)] 
    public class AdminController : BaseController
    {

        #region Constants
        const string adminNickname = "Admin";
        #endregion

        #region Constructor
        public AdminController(Dal.DataAccess.FileHosterRepoContext context, IJwtAuthenticationManager authenticationManager)
            : base(context, authenticationManager) { }
        #endregion

        #region Actions
        [AllowAnonymous]
        [HttpPost]
        [Route("/" + Common.ApiRoutes.Admin.Register)]
        public async Task<ActionResult<string>> Register(Common.Api.Request.UserRegister newUser)
        {
            //Validate new user data.
            if (newUser == null)
                return BadRequest("Please send data for a new user.");
            if (string.IsNullOrWhiteSpace(newUser.Email))
                return BadRequest("Please send email.");
            if (string.IsNullOrWhiteSpace(newUser.Password))
                return BadRequest("Please send password.");

            // Check if administrator exists. At this point it can be only one administrator.
            var adminsFound = await context.Users.Where(u => u.Role == Common.User.UserRole.Admin).ToListAsync();

            if (adminsFound.Count == 0)
            {
                var newAdminDb = new Dal.Model.User()
                {
                    Email = newUser.Email,
                    Password = EncryptPassword(newUser.Password),
                    Nickname = adminNickname,                           // Administrator has fixed nickname.
                    Created = DateTime.Now,
                    Role = Common.User.UserRole.Admin,
                    Logged = true
                };
                context.Users.Add(newAdminDb);
                await context.SaveChangesAsync();
                return Ok(token.Generate(newAdminDb.UserId, newAdminDb.Email, Common.User.UserRole.Admin));
            }
            else
            {
                return Conflict($"There is already administrator with email {adminsFound.FirstOrDefault()?.Email}.");
            }
        }

        [AllowAnonymous]
        [HttpPost()]
        [Route("/" + Common.ApiRoutes.Admin.Login)]
        public async Task<ActionResult<string>> Login(Common.Api.Request.User loginUser)
        {
            var usersFound = await context.Users.Where(u => u.Email == loginUser.Email && u.Role == Common.User.UserRole.Admin).ToListAsync();
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

        [HttpGet]
        [Route("/" + Common.ApiRoutes.Admin.Logout)]
        public async Task<ActionResult<string>> Logout()
        {
            // Always check at beginning!
            var loggedAdmin = await GetLoggedAdminAsync();
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

        [HttpGet]
        [Route("/" + Common.ApiRoutes.Admin.Info)]
        public async Task<ActionResult<Common.Api.Response.User>> Info()
        {
            // Always check at beginning!
            var loggedAdmin = await GetLoggedAdminAsync();
            if (loggedAdmin != null)
            {
                return new Common.Api.Response.User()
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

        [HttpPatch]
        [Route("/" + Common.ApiRoutes.Admin.Update)]
        public async Task<ActionResult<bool>> Update(Common.Api.Request.UserRegister updateUser)
        {
            // Validate password.
            if (string.IsNullOrWhiteSpace(updateUser.Password))
                return Conflict("Password can not be empty.");

            // Always check at beginning!
            var loggedAdmin = await GetLoggedAdminAsync();
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

        [AllowAnonymous]
        [HttpGet]
        [Route("/" + Common.ApiRoutes.Admin.IsRegistered)]
        public async Task<ActionResult<bool>> IsRegistered()
        {
            return Ok(await context.Users.CountAsync(u => u.Role == Common.User.UserRole.Admin) > 0);
        }
        #endregion

        #region Methods
        #endregion
    }
}