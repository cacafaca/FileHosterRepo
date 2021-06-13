using System.Threading.Tasks;

namespace ProCode.FileHosterRepo.WebAppBlazor.ViewModel.Admin
{
    public interface IAdminViewModel : IUserActions, IIsLogged, IToken
    {
        public Task<bool> IsRegisteredAsync();
    }
}
