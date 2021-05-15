using Microsoft.AspNetCore.Mvc;

namespace ProCode.FileHosterRepo.Api.Controllers
{
    public class BaseController : Controller
    {
        #region Fields
        protected readonly Dal.DataAccess.FileHosterRepoContext context;
        protected readonly IJwtAuthenticationManager authenticationManager;
        #endregion

        #region Constructor
        public BaseController(Dal.DataAccess.FileHosterRepoContext context, IJwtAuthenticationManager authenticationManager)
        {
            this.context = context;
            this.authenticationManager = authenticationManager;
        }
        #endregion
    }
}