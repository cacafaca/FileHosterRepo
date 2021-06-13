using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProCode.FileHosterRepo.WebAppBlazor.ViewModel.User
{
    public interface IUserViewModel: IUserActions, IIsLogged, IToken
    {
        public Task<bool> IsAdminRegistredAsync();
        public Task<bool> IsUserAdminAsync();
    }
}
