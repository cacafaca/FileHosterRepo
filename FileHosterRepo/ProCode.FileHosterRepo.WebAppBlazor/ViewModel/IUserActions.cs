using System.Threading.Tasks;

namespace ProCode.FileHosterRepo.WebAppBlazor.ViewModel
{
    public interface IUserActions
    {
        public Task<bool> RegisterAsync(Common.Api.Request.UserRegister userRegister, string confirmPassword);
        public Task<bool> LoginAsync(Common.Api.Request.User user);
        public Task<string> LogoutAsync();
        public Task<Common.Api.Response.User> GetInfoAsync();
    }
}