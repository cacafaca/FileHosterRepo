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
    public class MediaController : BaseController
    {
        #region Constructor
        public MediaController(Dal.DataAccess.FileHosterRepoContext context, IJwtAuthenticationManager authenticationManager)
            : base(context, authenticationManager) { }
        #endregion

        #region Actions
        [HttpPost("Add")]
        public async Task<ActionResult<bool>> Add([FromForm] Model.Request.Media newMedia)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Methods
        #endregion
    }
}