using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProCode.FileHosterRepo.WebAppBlazor.ViewModel.User
{
    public interface IUserViewModel
    {
        /// <summary>
        /// Returns a JWT token.
        /// </summary>
        /// <param name="userRegister"></param>
        /// <returns></returns>
        public Task<bool> RegisterAsync(Dto.Api.Request.UserRegister userRegister);

        public Task<Dto.Api.Response.User> GetInfoAsync();
    }
}
