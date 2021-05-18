using Microsoft.AspNetCore.Mvc;

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

        #endregion
    }
}